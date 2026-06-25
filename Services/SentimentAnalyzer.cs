// Summary of comments:
// - This file contains a small rule-based SentimentAnalyzer that detects simple user sentiment
//   from keywords, notifies listeners when the sentiment changes, and produces a SentimentResult
//   containing a label, emoji and tone prefix used by the bot when forming replies.

using System; // For StringComparison and other basic types
using System.Collections.Generic; // For Dictionary and List
using System.Linq; // For LINQ helpers like Any

namespace CyberAwarenessBot // Application namespace grouping analysis and other services
{
    public class SentimentAnalyzer // Detects basic sentiment via keyword matching and emits change events
    {
        public delegate void SentimentChangedHandler(string sentiment, string emoji); // Delegate signature for change notifications
        public event SentimentChangedHandler OnSentimentChanged; // Event fired when detected sentiment changes

        private string _lastSentiment = "Neutral"; // Tracks last emitted sentiment to avoid duplicate notifications

        // Map sentiment labels to lists of indicative keywords/phrases
        private static readonly Dictionary<string, List<string>> _keywordMap = new Dictionary<string, List<string>>()
        {
            ["Worried"] = new List<string>
            {
                // WORRIED keywords (assignment-specified)
                "worried", "concerned", "anxious", "nervous", "scared", "afraid",
                "unsafe", "vulnerable", "hacked", "stolen", "breach", "attack",
                "danger", "threat", "risk"
            },
            ["Curious"] = new List<string>
            {
                // CURIOUS keywords (assignment-specified)
                "curious", "interest", "interesting", "learn", "want to know",
                "how does", "what is", "why", "tell me about", "explain", "teach me"
            },
            ["Frustrated"] = new List<string>
            {
                // FRUSTRATED keywords (assignment-specified)
                "frustrated", "annoying", "difficult", "hard", "complicated",
                "confusing", "don't understand", "too many", "hate", "tired", "overwhelming", "stuck"
            }
        };

        // Tone prefixes that can be prepended to bot replies to match the sentiment
        private static readonly Dictionary<string, string> _tonePrefixes = new Dictionary<string, string>()
        {
            ["Worried"] = "I completely understand your concern — you're right to take security seriously. 💙\n",
            ["Curious"] = "Great question! I love your curiosity about cybersecurity! 🌟\n",
            ["Frustrated"] = "I'm sorry this feels overwhelming — let me simplify things for you. 😊\n",
            ["Neutral"] = ""
        };

        // Emoji mapping for UI sentiment indicator
        private static readonly Dictionary<string, string> _emojiMap = new Dictionary<string, string>()
        {
            ["Worried"] = "😟",
            ["Curious"] = "🤔",
            ["Frustrated"] = "😤",
            ["Neutral"] = "😐"
        };

        // Analyse the input string and return a SentimentResult describing the detected sentiment
        public SentimentResult Analyse(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return BuildResult("Neutral"); // Empty input => Neutral

            foreach (var kv in _keywordMap) // Iterate sentiment categories and check for any keyword match
            {
                var sentiment = kv.Key; // Sentiment label (e.g., "Worried")
                var keywords = kv.Value; // Associated keyword list
                if (keywords.Any(kw => input.IndexOf(kw, StringComparison.OrdinalIgnoreCase) >= 0)) // Case-insensitive search
                {
                    if (sentiment != _lastSentiment) // If sentiment changed since last time
                    {
                        _lastSentiment = sentiment; // Update last sentiment
                        if (OnSentimentChanged != null) OnSentimentChanged.Invoke(sentiment, _emojiMap[sentiment]); // Notify subscribers
                    }
                    return BuildResult(sentiment); // Return result for detected sentiment
                }
            }

            if (_lastSentiment != "Neutral") // If previously we were in a non-neutral state, reset to Neutral and notify
            {
                _lastSentiment = "Neutral"; // Reset stored sentiment
                OnSentimentChanged?.Invoke("Neutral", _emojiMap["Neutral"]); // Safe-invoke notification
            }
            return BuildResult("Neutral"); // Default to neutral result
        }

        // Helper that builds a SentimentResult from a label using the emoji and prefix maps
        private static SentimentResult BuildResult(string label)
        {
            return new SentimentResult(label, _emojiMap[label], _tonePrefixes[label]); // Construct result object
        }
    }
}
