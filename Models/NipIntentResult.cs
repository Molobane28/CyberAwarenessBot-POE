namespace CyberAwarenessBot
{
    /// <summary>
    /// Result of NLP processing containing intent and extracted information
    /// </summary>
    public class NlpIntentResult
    {
        public string Intent { get; set; }
        public string ExtractedTitle { get; set; }
        public string ExtractedTopic { get; set; }
        public string ExtractedTime { get; set; }
        public bool WantsReminder { get; set; }
        public string RawInput { get; set; }
    }
}