// Summary of comments:
// - This file defines a simple model `SentimentResult` used to carry sentiment analysis output.
// - Each line is annotated with a concise comment describing its purpose: properties and constructor behavior.

namespace CyberAwarenessBot // Application namespace containing models and services
{
    public class SentimentResult // Model representing the result of sentiment analysis
    {
        public string Label { get; set; } // Sentiment label (e.g., "happy", "worried")
        public string Emoji { get; set; } // Emoji to visually represent the sentiment
        public string TonePrefix { get; set; } // Optional tonal prefix text used before bot replies

        public SentimentResult(string label, string emoji, string tonePrefix) // Constructor initializing all properties
        {
            Label = label; // Assign provided label to the property
            Emoji = emoji; // Assign provided emoji to the property
            TonePrefix = tonePrefix; // Assign provided tone prefix to the property
        }
    }
}
