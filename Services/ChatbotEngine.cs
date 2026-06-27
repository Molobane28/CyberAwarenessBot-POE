using System;
using System.Collections.Generic;

namespace CyberAwarenessBot
{
    public delegate void MemoryUpdatedHandler(UserMemory memory);

    public class ChatbotEngine
    {
        private readonly UserMemory memory;
        private readonly ConversationState state;
        private readonly SentimentAnalyzer sentiment;
        private readonly KeywordResponseProvider keywords;
        private readonly MemoryUpdatedHandler onMemoryUpdated;
        private readonly Random rng = new Random(Guid.NewGuid().GetHashCode());
        private int lastGeneralTipIndex = -1;

        private static readonly string[] generalTips = new[]
        {
            "🚨 Tip: Enable Two-Factor Authentication (2FA) on your important accounts to block unauthorized access.",
            "🔒 Tip: Use strong, unique passwords or a password manager to avoid credential reuse.",
            "⚠️ Tip: Never click links in unexpected emails - type the website address directly into your browser.",
            "💾 Tip: Keep regular backups of important files (follow the 3-2-1 rule) to recover from ransomware.",
            "🛡️ Tip: Keep your operating system and applications updated to patch known vulnerabilities."
        };

        private static readonly List<string> FollowUpPhrases = new List<string>()
        {
            "tell me more", "another tip", "more", "give me another",
            "continue", "next tip", "another example", "explain more"
        };

        private static readonly List<string> ExitPhrases = new List<string>()
        {
            "bye", "goodbye", "exit", "quit", "see you", "later"
        };

        public ChatbotEngine(
            SentimentAnalyzer.SentimentChangedHandler onSentimentChanged,
            MemoryUpdatedHandler onMemoryUpdated)
        {
            this.onMemoryUpdated = onMemoryUpdated;
            memory = new UserMemory();
            state = new ConversationState();
            keywords = new KeywordResponseProvider();

            sentiment = new SentimentAnalyzer();
            sentiment.OnSentimentChanged += onSentimentChanged;
        }

        public string GetWelcomeMessage() =>
            "👋 Hello! I'm CyberGuard AI - your personal cybersecurity awareness assistant.\n\n" +
            "I'm here to help you stay safe online with tips on passwords, phishing, privacy, and more.\n\n" +
            "Before we start - what's your name?";

        public string ProcessInput(string rawInput)
        {
            if (string.IsNullOrWhiteSpace(rawInput))
                return "I didn't catch that - could you type something?";

            string input = rawInput.Trim();
            string lower = input.ToLowerInvariant();
            state.RecordInput(input);

            if (state.Phase != OnboardingPhase.Chatting)
                return HandleOnboarding(input, lower);

            foreach (var phrase in ExitPhrases)
                if (lower.Contains(phrase)) return BuildFarewell();

            var sentiment = this.sentiment.Analyse(lower);

            if (IsFollowUp(lower) && state.LastTopic != null)
            {
                string fu = keywords.GetFollowUpFor(state.LastTopic);
                if (fu != null) return GetFollowUpSentimentPrefix(sentiment) + fu;
            }

            string detectedTopic = ExtractTopic(lower);
            if (string.Equals(sentiment.Label, "Neutral", StringComparison.OrdinalIgnoreCase) &&
                !string.IsNullOrEmpty(detectedTopic) &&
                !memory.HasTopic)
            {
                memory.Set(UserMemory.KeyTopic, detectedTopic);
                onMemoryUpdated(memory);
                return $"I'll remember that you're interested in {detectedTopic}. It's a crucial part of staying safe online.";
            }

            if (!string.Equals(sentiment.Label, "Neutral", StringComparison.OrdinalIgnoreCase))
            {
                string topic = ExtractTopic(lower) ?? state.LastTopic;
                string tip = null;

                if (!string.IsNullOrEmpty(topic))
                    tip = keywords.GetFollowUpFor(topic);

                if (string.IsNullOrEmpty(tip))
                    tip = GetGeneralTip();

                switch (sentiment.Label)
                {
                    case "Worried":
                        return "It's completely understandable to feel that way. Cybersecurity can feel overwhelming - many people worry about scams or breaches. Let me share some tips to help you stay safe:\n\n" + PersonalizeTip(tip, topic);

                    case "Curious":
                        return "That's a great question! I love your curiosity about cybersecurity. Here's something useful for you:\n\n" + PersonalizeTip(tip, topic);

                    case "Frustrated":
                        return "I hear your frustration - cybersecurity can be confusing at first. Let me break this down simply:\n\n" + PersonalizeTip(tip, topic);

                    default:
                        return PersonalizeTip(tip, topic);
                }
            }

            string reply = keywords.GetResponse(lower, state);
            if (reply != null)
            {
                string topicForReply = state.LastTopic ?? ExtractTopic(lower);
                string personalized = PersonalizeReply(reply, topicForReply);
                return GetQuestionSentimentPrefix(sentiment) + personalized;
            }

            if (lower.Contains("help") || lower.Contains("topics"))
                return BuildHelpMessage();

            return GetQuestionSentimentPrefix(sentiment) +
                   "🤖 I'm not sure I understand. Try topics like: passwords, phishing, privacy, malware, 2FA.\n" +
                   "You can also say: add task, show tasks, start quiz, or show activity log.";
        }

        private string GetGeneralTip()
        {
            int idx = rng.Next(generalTips.Length);
            int attempts = 0;
            while (idx == lastGeneralTipIndex && generalTips.Length > 1 && attempts < 6)
            {
                idx = rng.Next(generalTips.Length);
                attempts++;
            }
            lastGeneralTipIndex = idx;
            return generalTips[idx];
        }

        private string GetQuestionSentimentPrefix(SentimentResult sentiment)
        {
            return sentiment?.TonePrefix ?? string.Empty;
        }

        private string GetFollowUpSentimentPrefix(SentimentResult sentiment)
        {
            if (sentiment == null) return string.Empty;
            switch (sentiment.Label)
            {
                case "Curious":
                    return "I love your curiosity! 🌟\n";
                case "Worried":
                    return "I understand - here's another tip: 💙\n";
                case "Frustrated":
                    return "Absolutely! Here's another tip: 🎯\n";
                default:
                    return "Here's another tip: 💡\n";
            }
        }

        private string HandleOnboarding(string input, string lower)
        {
            switch (state.Phase)
            {
                case OnboardingPhase.AskName:
                    string name = input.Length > 30 ? input.Substring(0, 30).Trim() : input.Trim();
                    memory.Set(UserMemory.KeyName, name);
                    onMemoryUpdated(memory);
                    state.Phase = OnboardingPhase.AskTopic;
                    return $"Great to meet you, {name}!\n\nWhat's your favourite cybersecurity topic?\n" +
                           "(e.g. passwords, phishing, privacy, malware, 2FA)";

                case OnboardingPhase.AskTopic:
                    string topic = ExtractTopic(lower) ?? input.Trim();
                    memory.Set(UserMemory.KeyTopic, topic);
                    onMemoryUpdated(memory);
                    state.Phase = OnboardingPhase.AskLevel;
                    state.LastTopic = topic;
                    return $"I'll remember that you're interested in {topic}. It's a crucial part of staying safe online.\n\n" +
                           "Last question: what's your experience level?\n(beginner / intermediate / expert)";

                case OnboardingPhase.AskLevel:
                    string level = ExtractLevel(lower);
                    memory.Set(UserMemory.KeyLevel, level);
                    onMemoryUpdated(memory);
                    state.Phase = OnboardingPhase.Chatting;
                    return $"Perfect - I'll tailor my advice for a {level}. 💪\n\n" +
                           "Ask me about: passwords, phishing, privacy, malware, 2FA, scams, and more!\n" +
                           "Type 'help' anytime for the full topic list.";

                default:
                    state.Phase = OnboardingPhase.Chatting;
                    return "Let's chat! Ask me anything about cybersecurity.";
            }
        }

        private static bool IsFollowUp(string lower)
        {
            foreach (var p in FollowUpPhrases)
                if (lower.Contains(p)) return true;
            return false;
        }

        private static string ExtractTopic(string lower)
        {
            string[] known =
            {
                "password", "phishing", "privacy", "malware", "2fa", "scam",
                "ransomware", "data breach", "breach", "vpn", "encryption"
            };

            foreach (var t in known)
                if (lower.Contains(t)) return t;

            return null;
        }

        private static string ExtractLevel(string lower)
        {
            if (lower.Contains("expert")) return "expert";
            if (lower.Contains("intermediate")) return "intermediate";
            return "beginner";
        }

        private string BuildHelpMessage() =>
            "🤖 Available Topics\n\n" +
            "🔒 passwords\n🎣 phishing\n🛡️ privacy\n🦠 malware\n" +
            "🔑 2FA\n💥 data breach\n⚠️ scam\n💰 ransomware\n\n" +
            "Extra commands:\n" +
            "- add task\n" +
            "- show tasks\n" +
            "- start quiz\n" +
            "- show activity log\n\n" +
            "Say 'tell me more' or 'another tip' to continue on the last topic.";

        private string BuildFarewell() =>
            "👋 Stay safe online! Remember:\n\n" +
            "- Use strong, unique passwords\n" +
            "- Enable 2FA everywhere\n" +
            "- Think before you click\n";

        private string PersonalizeTip(string tip, string topic)
        {
            string namePart = memory.HasName ? $"{memory.UserName}, " : string.Empty;
            string topicPart = !string.IsNullOrEmpty(topic) ? $"As someone interested in {topic}, you might want to:\n" : string.Empty;
            string adjusted = AdjustTipForLevel(tip, memory.ExperienceLevel);
            return namePart + topicPart + adjusted;
        }

        private string PersonalizeReply(string reply, string topic)
        {
            string namePart = memory.HasName ? $"{memory.UserName}, " : string.Empty;
            string topicPart = !string.IsNullOrEmpty(topic) ? $"As someone interested in {topic}, you might:\n" : string.Empty;
            string adjusted = AdjustTipForLevel(reply, memory.ExperienceLevel);
            return namePart + topicPart + adjusted;
        }

        private string AdjustTipForLevel(string text, string level)
        {
            if (string.IsNullOrEmpty(level)) return text;

            switch (level.ToLowerInvariant())
            {
                case "expert":
                    return text;
                case "intermediate":
                    return text + "\n(If you want more detail, ask for examples or deeper steps.)";
                default:
                    return text + "\n\nQuick action: try this step now to improve security.";
            }
        }
    }
}