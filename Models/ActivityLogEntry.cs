using System;

namespace CyberAwarenessBot
{
    /// <summary>
    /// Represents a single activity log entry with timestamp
    /// </summary>
    public class ActivityLogEntry
    {
        public DateTime Timestamp { get; set; }
        public string ActionType { get; set; }
        public string Message { get; set; }

        public string DisplayText => $"[{Timestamp:yyyy-MM-dd HH:mm:ss}] {ActionType}: {Message}";
    }
}