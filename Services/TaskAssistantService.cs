using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace CyberAwarenessBot
{
    public class TaskAssistantService
    {
        private readonly ITaskRepository repository;
        private readonly IActivityLogger logger;

        public TaskAssistantService(ITaskRepository repository, IActivityLogger logger)
        {
            this.repository = repository;
            this.logger = logger;
        }

        public CyberTask AddTask(string title, string description = null, DateTime? reminder = null)
        {
            var task = new CyberTask
            {
                Title = title,
                Description = description,
                ReminderAt = reminder,
                IsCompleted = false,
                CreatedAt = DateTime.Now
            };

            task.Id = repository.AddTask(task);

            logger.Log("Task Added", $"Task #{task.Id} '{task.Title}' was added.");
            if (task.ReminderAt.HasValue)
                logger.Log("Reminder Set", $"Reminder set for task #{task.Id} at {task.ReminderAt.Value:yyyy-MM-dd HH:mm}.");

            return task;
        }

        public List<CyberTask> GetTasks()
        {
            return repository.GetAllTasks();
        }

        public void CompleteTask(int id)
        {
            repository.MarkCompleted(id);
            logger.Log("Task Completed", $"Task #{id} was marked as completed.");
        }

        public void DeleteTask(int id)
        {
            repository.DeleteTask(id);
            logger.Log("Task Deleted", $"Task #{id} was deleted.");
        }

        public void SetReminder(int id, DateTime? reminderAt)
        {
            repository.UpdateReminder(id, reminderAt);
            if (reminderAt.HasValue)
                logger.Log("Reminder Set", $"Reminder updated for task #{id} to {reminderAt.Value:yyyy-MM-dd HH:mm}.");
            else
                logger.Log("Reminder Cleared", $"Reminder cleared for task #{id}.");
        }

        public string BuildTaskListMessage()
        {
            var tasks = GetTasks();
            if (tasks == null || tasks.Count == 0)
                return "You currently have no tasks saved.";

            var sb = new StringBuilder();
            sb.AppendLine("Here are your cybersecurity tasks:");
            sb.AppendLine();

            foreach (var task in tasks)
            {
                string statusEmoji = task.IsCompleted ? "✅" : "🕓";
                sb.AppendLine($"- #{task.Id}: {task.Title} {statusEmoji} [{task.StatusText}]");
                if (!string.IsNullOrWhiteSpace(task.Description))
                    sb.AppendLine($"  Description: {task.Description}");
                sb.AppendLine($"  Reminder: {task.ReminderText}");
            }

            return sb.ToString().Trim();
        }

        // Parse natural language time expressions into DateTime
        public bool TryParseNaturalLanguageReminder(string input, out DateTime reminder)
        {
            reminder = DateTime.Now;
            if (string.IsNullOrWhiteSpace(input))
                return false;

            string lower = input.ToLowerInvariant().Trim();

            // Explicit decline
            if (lower == "no" || lower == "nope" || lower == "skip" || lower == "none" || lower == "don't")
            {
                return false;
            }

            // "in X days"
            var daysMatch = Regex.Match(lower, @"in\s+(\d+)\s+days?");
            if (daysMatch.Success && int.TryParse(daysMatch.Groups[1].Value, out int days))
            {
                reminder = DateTime.Now.AddDays(days).Date.AddHours(9); // default 9 AM
                return true;
            }

            // "in X weeks"
            var weeksMatch = Regex.Match(lower, @"in\s+(\d+)\s+weeks?");
            if (weeksMatch.Success && int.TryParse(weeksMatch.Groups[1].Value, out int weeks))
            {
                reminder = DateTime.Now.AddDays(weeks * 7).Date.AddHours(9);
                return true;
            }

            // "in X hours"
            var hoursMatch = Regex.Match(lower, @"in\s+(\d+)\s+hours?");
            if (hoursMatch.Success && int.TryParse(hoursMatch.Groups[1].Value, out int hours))
            {
                reminder = DateTime.Now.AddHours(hours);
                return true;
            }

            // "tomorrow" with optional time
            if (lower.Contains("tomorrow"))
            {
                reminder = DateTime.Now.AddDays(1).Date;

                var timeMatch = Regex.Match(lower, @"at\s+(\d{1,2})(?::(\d{2}))?\s*(am|pm)?");
                if (timeMatch.Success)
                {
                    if (int.TryParse(timeMatch.Groups[1].Value, out int hour))
                    {
                        int minute = 0;
                        if (timeMatch.Groups[2].Success)
                            int.TryParse(timeMatch.Groups[2].Value, out minute);

                        string meridiem = timeMatch.Groups[3].Value.ToLowerInvariant();
                        if (meridiem == "pm" && hour != 12) hour += 12;
                        if (meridiem == "am" && hour == 12) hour = 0;

                        reminder = reminder.AddHours(hour).AddMinutes(minute);
                        return true;
                    }
                }
                else
                {
                    reminder = reminder.AddHours(9);
                    return true;
                }
            }

            // "next monday at 9am" etc.
            var dayOfWeekMatch = Regex.Match(lower, @"next\s+(monday|tuesday|wednesday|thursday|friday|saturday|sunday)(?:\s+at\s+(\d{1,2})(?::(\d{2}))?\s*(am|pm)?)?");
            if (dayOfWeekMatch.Success)
            {
                string dayName = dayOfWeekMatch.Groups[1].Value;
                DayOfWeek targetDay = ParseDayOfWeek(dayName);

                int daysUntil = ((int)targetDay - (int)DateTime.Now.DayOfWeek + 7) % 7;
                if (daysUntil == 0) daysUntil = 7;

                reminder = DateTime.Now.AddDays(daysUntil).Date;

                if (dayOfWeekMatch.Groups[2].Success && int.TryParse(dayOfWeekMatch.Groups[2].Value, out int hour))
                {
                    int minute = 0;
                    if (dayOfWeekMatch.Groups[3].Success)
                        int.TryParse(dayOfWeekMatch.Groups[3].Value, out minute);

                    string meridiem = dayOfWeekMatch.Groups[4].Value.ToLowerInvariant();
                    if (meridiem == "pm" && hour != 12) hour += 12;
                    if (meridiem == "am" && hour == 12) hour = 0;

                    reminder = reminder.AddHours(hour).AddMinutes(minute);
                }
                else
                {
                    reminder = reminder.AddHours(9);
                }
                return true;
            }

            // Specific date/time formats and common natural date formats
            if (TryParseStandardReminder(input, out reminder))
                return true;

            // Fallback: try DateTime.Parse
            if (DateTime.TryParse(input, out reminder))
                return true;

            return false;
        }

        // Standard datetime format parsing
        public static bool TryParseStandardReminder(string input, out DateTime reminder)
        {
            string[] formats =
            {
                "yyyy-MM-dd HH:mm",
                "yyyy/MM/dd HH:mm",
                "dd/MM/yyyy HH:mm",
                "dd-MM-yyyy HH:mm",
                "yyyy-MM-ddTHH:mm",
                "MM/dd/yyyy HH:mm",
                "dd MMMM yyyy",
                "dd MMMM yyyy HH:mm"
            };

            return DateTime.TryParseExact(
                input,
                formats,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeLocal,
                out reminder)
                || DateTime.TryParse(input, out reminder);
        }

        // Helper: Convert day name to DayOfWeek
        private static DayOfWeek ParseDayOfWeek(string dayName)
        {
            string lower = dayName.ToLowerInvariant();

            if (lower == "monday") return DayOfWeek.Monday;
            if (lower == "tuesday") return DayOfWeek.Tuesday;
            if (lower == "wednesday") return DayOfWeek.Wednesday;
            if (lower == "thursday") return DayOfWeek.Thursday;
            if (lower == "friday") return DayOfWeek.Friday;
            if (lower == "saturday") return DayOfWeek.Saturday;
            if (lower == "sunday") return DayOfWeek.Sunday;

            return DayOfWeek.Monday;
        }

        // Format a reminder datetime into user-friendly text
        public static string FormatReminderForUser(DateTime reminder)
        {
            TimeSpan timeUntil = reminder - DateTime.Now;

            if (timeUntil.TotalMinutes < 1)
                return "in a moment";
            else if (timeUntil.TotalHours < 1)
                return $"in {(int)timeUntil.TotalMinutes} minutes";
            else if (timeUntil.TotalHours < 24)
                return $"in {(int)timeUntil.TotalHours} hours";
            else if (timeUntil.TotalDays < 7)
                return $"in {(int)timeUntil.TotalDays} days at {reminder:h:mm tt}";
            else
                return $"on {reminder:MMMM dd, yyyy} at {reminder:h:mm tt}";
        }
    }
}
