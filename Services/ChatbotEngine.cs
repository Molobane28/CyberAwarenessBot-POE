// Summary of comments:
// - ChatbotEngine orchestrates conversation flow, onboarding, sentiment analysis and keyword responses.
// - Each line below is annotated with a short comment describing its purpose, fields, methods and control flow.

using System; // Basic system types and exceptions
using System.Collections.Generic; // Collection types like List<T> and Dictionary<TKey,TValue>
using System.Linq; // LINQ helpers (not heavily used here but included)

namespace CyberAwarenessBot // Application namespace
{
    // Delegate type used to notify subscribers when user memory changes
    public delegate void MemoryUpdatedHandler(UserMemory memory);

    public class ChatbotEngine // Core engine class responsible for processing user inputs
    {
        private readonly UserMemory _memory; // In-memory store for user attributes
        private readonly ConversationState _state; // Per-session conversation state
        private readonly SentimentAnalyzer _sentiment; // Sentiment analysis component
        private readonly KeywordResponseProvider _keywords; // Keyword-driven response provider
        private readonly MemoryUpdatedHandler _onMemoryUpdated; // Callback invoked when memory updates
        private readonly Random _rng = new Random(Guid.NewGuid().GetHashCode()); // RNG for general tips
        private int _lastGeneralTipIndex = -1; // Remember last general tip to avoid immediate repeats

        // General cybersecurity tips used when no specific topic can be determined
        private static readonly string[] _generalTips = new[]
        {
            "🚨 Tip: Enable Two-Factor Authentication (2FA) on your important accounts to block unauthorized access.",
            "🔒 Tip: Use strong, unique passwords or a password manager to avoid credential reuse.",
            "⚠️ Tip: Never click links in unexpected emails — type the website address directly into your browser.",
            "💾 Tip: Keep regular backups of important files (follow the 3-2-1 rule) to recover from ransomware.",
            "🛡️ Tip: Keep your operating system and applications updated to patch known vulnerabilities."
        };

        // Common follow-up phrases that trigger continuation of the last topic
        private static readonly List<string> FollowUpPhrases = new List<string>()
        {
            "tell me more", "another tip", "more", "give me another",
            "continue", "next tip", "another example", "explain more"
        };

        // Phrases indicating the user wants to exit the conversation
        private static readonly List<string> ExitPhrases = new List<string>()
        {
            "bye", "goodbye", "exit", "quit", "see you", "later"
        };

        // Constructor accepting callbacks for sentiment changes and memory updates
        public ChatbotEngine(
            SentimentAnalyzer.SentimentChangedHandler onSentimentChanged,
            MemoryUpdatedHandler onMemoryUpdated)
        {
            _onMemoryUpdated = onMemoryUpdated; // Store memory update callback
            _memory = new UserMemory(); // Initialize user memory store
            _state = new ConversationState(); // Initialize conversation state
            _keywords = new KeywordResponseProvider(); // Initialize keyword response provider

            _sentiment = new SentimentAnalyzer(); // Create sentiment analyzer
            _sentiment.OnSentimentChanged += onSentimentChanged; // Hook external callback into analyzer
        }

        // Default welcome message shown to new users
        public string GetWelcomeMessage() =>
            "👋 Hello! I'm CyberGuard AI — your personal cybersecurity awareness assistant.\n\n" +
            "I'm here to help you stay safe online with tips on passwords, phishing, privacy, and more.\n\n" +
            "Before we start — what's your name?";

        // Main entry point for processing raw user input and returning a bot reply
        public string ProcessInput(string rawInput)
        {
            if (string.IsNullOrWhiteSpace(rawInput)) // Ignore empty submissions
                return "I didn't catch that — could you type something?";

            string input = rawInput.Trim(); // Trim whitespace from ends
            string lower = input.ToLowerInvariant(); // Lowercase version for simple matching
            _state.RecordInput(input); // Record raw input in the conversation history

            if (_state.Phase != OnboardingPhase.Chatting) // If still onboarding, delegate to onboarding handler
                return HandleOnboarding(input, lower);

            foreach (var phrase in ExitPhrases) // Check for exit phrases and return a farewell if found
                if (lower.Contains(phrase)) return BuildFarewell();

            var sentiment = _sentiment.Analyse(lower); // Analyze sentiment to update UI and get tone info

            // Check for follow-up requests first and use a shorter follow-up sentiment prefix
            if (IsFollowUp(lower) && _state.LastTopic != null) // If user asks for follow-up and we have a last topic
            {
                string fu = _keywords.GetFollowUpFor(_state.LastTopic); // Get follow-up hint from keywords provider
                if (fu != null) return GetFollowUpSentimentPrefix(sentiment) + fu; // Use short follow-up prefix + tip
            }

            // If there is no strong sentiment, but the user mentioned a topic and we don't yet store it,
            // remember the topic and confirm explicitly as required by the assignment.
            string detectedTopic = ExtractTopic(lower);
            if (string.Equals(sentiment.Label, "Neutral", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(detectedTopic) && !_memory.HasTopic)
            {
                _memory.Set(UserMemory.KeyTopic, detectedTopic);
                _onMemoryUpdated(_memory);
                return $"I'll remember that you're interested in {detectedTopic}. It's a crucial part of staying safe online.";
            }

            // If user expresses a clear sentiment (worried/curious/frustrated), provide encouragement + relevant tip automatically
            if (!string.Equals(sentiment.Label, "Neutral", StringComparison.OrdinalIgnoreCase))
            {
                // Try to determine the topic from the user's message or fall back to last discussed topic
                string topic = ExtractTopic(lower) ?? _state.LastTopic;
                string tip = null;
                if (!string.IsNullOrEmpty(topic))
                {
                    tip = _keywords.GetFollowUpFor(topic); // Get a topic-relevant tip
                }
                // If no topic-specific tip is available, use a general cybersecurity tip
                if (string.IsNullOrEmpty(tip))
                {
                    tip = GetGeneralTip();
                }

                // Build supportive message according to detected sentiment and append the tip
                switch (sentiment.Label)
                {
                    case "Worried":
                        return "It's completely understandable to feel that way. Cybersecurity can feel overwhelming — many people worry about scams or breaches. Let me share some tips to help you stay safe: \n\n" + PersonalizeTip(tip, topic);
                    case "Curious":
                        return "That's a great question! I love your curiosity about cybersecurity. Here's something useful for you: \n\n" + PersonalizeTip(tip, topic);
                    case "Frustrated":
                        return "I hear your frustration - cybersecurity can be confusing at first. Let me break this down simply: \n\n" + PersonalizeTip(tip, topic);
                    default:
                        return PersonalizeTip(tip, topic);
                }
            }

            // Not a sentiment-triggered message: regular flow — try keyword-driven response next
            string reply = _keywords.GetResponse(lower, _state); // Try keyword-driven response first
            if (reply != null) return GetQuestionSentimentPrefix(sentiment) + reply; // Use regular (full) sentiment prefix + reply
            if (reply != null)
            {
                // Personalize non-sentiment replies when possible
                string topicForReply = _state.LastTopic ?? ExtractTopic(lower);
                string personalized = PersonalizeReply(reply, topicForReply);
                return GetQuestionSentimentPrefix(sentiment) + personalized;
            }

            if (lower.Contains("help") || lower.Contains("topics")) // Explicit help request
                return BuildHelpMessage(); // Provide help message

            // Fallback message when nothing matches
            return GetQuestionSentimentPrefix(sentiment) +
                   "🤖 I'm not sure I understand. Try topics like: passwords, phishing, privacy, malware, 2FA.\n" +
                   "Type 'help' to see the full topic list.";
        }

        // Return a general tip, avoiding immediate repeats when possible
        private string GetGeneralTip()
        {
            int idx = _rng.Next(_generalTips.Length);
            int attempts = 0;
            while (idx == _lastGeneralTipIndex && _generalTips.Length > 1 && attempts < 6)
            {
                idx = _rng.Next(_generalTips.Length);
                attempts++;
            }
            _lastGeneralTipIndex = idx;
            return _generalTips[idx];
        }

        // Returns the sentiment prefix used for new questions (keeps the longer tone from SentimentAnalyzer)
        private string GetQuestionSentimentPrefix(SentimentResult sentiment)
        {
            // Keep the existing tone prefix produced by the SentimentAnalyzer for full-question replies
            return sentiment?.TonePrefix ?? string.Empty;
        }

        // Returns a shorter sentiment prefix used specifically for follow-up requests like "tell me more"
        private string GetFollowUpSentimentPrefix(SentimentResult sentiment)
        {
            if (sentiment == null) return string.Empty;
            switch (sentiment.Label)
            {
                case "Curious":
                    return "I love your curiosity! 🌟\n"; // Short encouragement for curiosity follow-ups
                case "Worried":
                    return "I understand — here's another tip: 💙\n"; // Short reassurance for worried follow-ups
                case "Frustrated":
                    return "Absolutely! Here's another tip: 🎯\n"; // Short supportive prefix for frustrated follow-ups
                default:
                    return "Here's another tip: 💡\n"; // Neutral short follow-up prefix
            }
        }

        // Handle the onboarding phases (name -> topic -> level)
        private string HandleOnboarding(string input, string lower)
        {
            switch (_state.Phase)
            {
                case OnboardingPhase.AskName:
                    string name = input.Length > 30 ? input.Substring(0, 30).Trim() : input.Trim(); // Truncate overly long names
                    _memory.Set(UserMemory.KeyName, name); // Save name in memory
                    _onMemoryUpdated(_memory); // Notify UI of memory change
                    _state.Phase = OnboardingPhase.AskTopic; // Advance phase
                    // Exact confirmation required by assignment followed by onboarding prompt
                    return $"Great to meet you, {name}!\n\nWhat's your favourite cybersecurity topic?\n" +
                           "(e.g. passwords, phishing, privacy, malware, 2FA)"; // Prompt for topic

                case OnboardingPhase.AskTopic:
                    string topic = ExtractTopic(lower) ?? input.Trim(); // Try to extract known topic or use raw input
                    _memory.Set(UserMemory.KeyTopic, topic); // Save topic
                    _onMemoryUpdated(_memory); // Update UI memory panel
                    _state.Phase = OnboardingPhase.AskLevel; // Advance phase
                    _state.LastTopic = topic; // Remember last topic for follow-ups
                    // Explicit confirmation message required by assignment (must include exact phrase)
                    _memory.Set(UserMemory.KeyTopic, topic); // ensure stored (already done)
                    _onMemoryUpdated(_memory);
                    return $"I'll remember that you're interested in {topic}. It's a crucial part of staying safe online.\n\n" +
                           "Last question: what's your experience level?\n(beginner / intermediate / expert)"; // Ask level

                case OnboardingPhase.AskLevel:
                    string level = ExtractLevel(lower); // Determine level from input
                    _memory.Set(UserMemory.KeyLevel, level); // Save experience level
                    _onMemoryUpdated(_memory); // Notify UI
                    _state.Phase = OnboardingPhase.Chatting; // Enter normal chatting phase
                    return $"Perfect — I'll tailor my advice for a {level}. 💪\n\n" +
                           "Ask me about: passwords, phishing, privacy, malware, 2FA, scams, and more!\n" +
                           "Type 'help' anytime for the full topic list."; // Final onboarding message

                default:
                    _state.Phase = OnboardingPhase.Chatting; // Ensure phase is set to chatting
                    return "Let's chat! Ask me anything about cybersecurity."; // Generic fallback
            }
        }

        // Check if input looks like a follow-up request
        private static bool IsFollowUp(string lower)
        {
            foreach (var p in FollowUpPhrases)
                if (lower.Contains(p)) return true; // Return true on first match
            return false; // No follow-up phrase found
        }

        // Try to extract a known topic keyword from the input
        private static string ExtractTopic(string lower)
        {
            // Extended known topic keywords to match the knowledge base and common user phrasing
            string[] known = { "password", "phishing", "privacy", "malware", "2fa", "scam", "ransomware", "data breach", "breach", "vpn", "encryption" };
            foreach (var t in known)
                if (lower.Contains(t)) return t; // Return first known topic found
            return null; // No known topic detected
        }

        // Determine user's experience level from input text
        private static string ExtractLevel(string lower)
        {
            if (lower.Contains("expert")) return "expert"; // Expert detected
            if (lower.Contains("intermediate")) return "intermediate"; // Intermediate detected
            return "beginner"; // Default to beginner
        }

        // Build a help message listing available topics
        private string BuildHelpMessage() =>
            "🤖 **Available Topics**\n\n" +
            "🔒 passwords\n🎣 phishing\n🛡️ privacy\n🦠 malware\n" +
            "🔑 2fa\n💥 data breach\n⚠️ scam\n💰 ransomware\n\n" +
            "Say 'tell me more' or 'another tip' to continue on the last topic.";

        // Build a friendly farewell message when the user exits
        private string BuildFarewell() =>
            "👋 Stay safe online! Remember:\n\n• Use strong, unique passwords\n• Enable 2FA everywhere\n• Think before you click\n\nCyberGuard AI is always here when you need it. 🛡️";

        // Personalize a tip by prefixing with user name or topic context when available
        private string PersonalizeTip(string tip, string topic)
        {
            string namePart = _memory.HasName ? $"{_memory.UserName}, " : string.Empty;
            string topicPart = !string.IsNullOrEmpty(topic) ? $"As someone interested in {topic}, you might want to: \n" : string.Empty;
            // Adjust tip phrasing based on user's experience level to tailor complexity
            string adjusted = AdjustTipForLevel(tip, _memory.ExperienceLevel);
            return (namePart + topicPart + adjusted);
        }

        // Personalize a non-sentiment reply with name/topic context
        private string PersonalizeReply(string reply, string topic)
        {
            string namePart = _memory.HasName ? $"{_memory.UserName}, " : string.Empty;
            string topicPart = !string.IsNullOrEmpty(topic) ? $"As someone interested in {topic}, you might: \n" : string.Empty;
            string adjusted = AdjustTipForLevel(reply, _memory.ExperienceLevel);
            return (namePart + topicPart + adjusted);
        }

        // Adjust tip/reply complexity based on user's experience level
        private string AdjustTipForLevel(string text, string level)
        {
            if (string.IsNullOrEmpty(level)) return text; // No adjustment if level unknown
            switch (level.ToLowerInvariant())
            {
                case "expert":
                    // Experts prefer concise, technical tips
                    return text;
                case "intermediate":
                    // Intermediate: keep detail but slightly explanatory
                    return text + "\n(If you want more detail, ask for examples or deeper steps.)";
                default: // beginner
                    // Beginners: add a simple actionable step
                    return text + "\n\nQuick action: try this step now to improve security.";
            }
        }
    }
}