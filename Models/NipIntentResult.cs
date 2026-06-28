namespace CyberAwarenessBot
{
    public class NlpIntentResult
    {
        public string Intent { get; set; }
        public string ExtractedTitle { get; set; }
        public string ExtractedTopic { get; set; }
        public string ExtractedTime { get; set; }   // NEW: For "remind me" time extraction
        public bool WantsReminder { get; set; }
        public string RawInput { get; set; }
    }
}