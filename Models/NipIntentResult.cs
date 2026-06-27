namespace CyberAwarenessBot
{
    public class NlpIntentResult
    {
        public string Intent { get; set; }
        public string ExtractedTitle { get; set; }
        public string ExtractedTopic { get; set; }
        public bool WantsReminder { get; set; }
        public string RawInput { get; set; }
    }
}