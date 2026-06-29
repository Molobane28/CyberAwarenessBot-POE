using System;

namespace CyberAwarenessBot
{
    /// <summary>
    /// Represents a cybersecurity task with reminder functionality
    /// </summary>
    public class CyberTask
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime? ReminderAt { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime CreatedAt { get; set; }

        public string StatusText => IsCompleted ? "Completed" : "Pending";

        public string ReminderText =>
            ReminderAt.HasValue ? ReminderAt.Value.ToString("yyyy-MM-dd HH:mm") : "No reminder";

        public override string ToString()
        {
            return $"#{Id} - {Title} [{StatusText}] | Reminder: {ReminderText}";
        }
    }
}