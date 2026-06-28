using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace CyberAwarenessBot
{
    /// <summary>
    /// Service for managing tasks with reminder functionality
    /// </summary>
    public class TaskAssistantService
    {
        private readonly ITaskRepository repository;
        private readonly IActivityLogger logger;

        public TaskAssistantService(ITaskRepository repository, IActivityLogger logger)
        {
            this.repository = repository;
            this.logger = logger;
        }

        /// <summary>
        /// Add a new task with optional description and reminder
        /// </summary>
        public CyberTask AddTask(string title, string description = null, DateTime? reminder = null)
        {
            try
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
            catch (Exception ex)
            {
                logger.Log("Error", $"Failed to add task: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get all tasks from repository
        /// </summary>
        public List<CyberTask> GetTasks()
        {
            try
            {
                return repository.GetAllTasks();
            }
            catch (Exception ex)
            {
                logger.Log("Error", $"Failed to retrieve tasks: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get a specific task by ID
        /// </summary>
        public CyberTask GetTaskById(int id)
        {
            try
            {
                return repository.GetTaskById(id);
            }
            catch (Exception ex)
            {
                logger.Log("Error", $"Failed to retrieve task #{id}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Mark task as completed
        /// </summary>
        public void CompleteTask(int id)
        {
            try
            {
                repository.MarkCompleted(id);
                logger.Log("Task Completed", $"Task #{id} was marked as completed.");
            }
            catch (Exception ex)
            {
                logger.Log("Error", $"Failed to complete task #{id}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Delete task
        /// </summary>
        public void DeleteTask(int id)
        {
            try
            {
                repository.DeleteTask(id);
                logger.Log("Task Deleted", $"Task #{id} was deleted.");
            }
            catch (Exception ex)
            {
                logger.Log("Error", $"Failed to delete task #{id}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Set or update reminder for task
        /// </summary>
        public void SetReminder(int id, DateTime? reminderAt)
        {
            try
            {
                repository.UpdateReminder(id, reminderAt);
                if (reminderAt.HasValue)
                    logger.Log("Reminder Set", $"Reminder updated for task #{id} to {reminderAt.Value:yyyy-MM-dd HH:mm}.");
                else
                    logger.Log("Reminder Cleared", $"Reminder cleared for task #{id}.");
            }
            catch (Exception ex)
            {
                logger.Log("Error", $"Failed to set reminder for task #{id}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Build a formatted task list message
        /// </summary>
        public string BuildTaskListMessage()
        {
            try
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
            catch (Exception ex)
            {
                logger.Log("Error", $"Failed to build task list: {ex.Message}");
                return "Error loading tasks. Please try again.";
            }
        }

        /// <summary>
        /// Parse natural language time expressions into DateTime
        /// </summary>
        public bool TryParseNaturalLanguageReminder(string input, out DateTime reminder)
        {
            reminder = DateTime.Now;

            if (string.IsNullOrWhiteSpace(input))
                return false;

            string lower = input.ToLowerInvariant().Trim();

            // Explicit decline
            if (lower == "no" || lower == "nope" || lower == "skip" || lower == "none" || lower == "don't")
                return false;

            // "in X days" or "X days"
            var daysMatch = Regex.Match(lower, @"(?:in\s+)?(\d+)\s+days?");
            if (daysMatch.Success && int.TryParse(daysMatch.Groups[1].Value, out int days))
            {
                reminder = DateTime.Now.AddDays(days);
                return true;
            }

            // "in X weeks" or "X weeks"
            var weeksMatch = Regex.Match(lower, @"(?:in\s+)?(\d+)\s+weeks?");
            if (weeksMatch.Success && int.TryParse(weeksMatch.Groups[1].Value, out int weeks))
            {
                reminder = DateTime.Now.AddDays(weeks * 7);
                return true;
            }

            // "in X hours" or "X hours"
            var hoursMatch = Regex.Match(lower, @"(?:in\s+)?(\d+)\s+hours?");
            if (hoursMatch.Success && int.TryParse(hoursMatch.Groups[1].Value, out int hours))
            {
                reminder = DateTime.Now.AddHours(hours);
                return true;
            }

            // Time-only inputs
            var timeOnlyMatch = Regex.Match(lower, @"\b(\d{1,2})(?::(\d{2}))?\s*(am|pm)?\b");
            if (timeOnlyMatch.Success && (timeOnlyMatch.Groups[3].Success || timeOnlyMatch.Groups[2].Success))
            {
                int hour = int.Parse(timeOnlyMatch.Groups[1].Value);
                int minute = 0;
                if (timeOnlyMatch.Groups[2].Success)
                    int.TryParse(timeOnlyMatch.Groups[2].Value, out minute);

                string meridiem = timeOnlyMatch.Groups[3].Value;
                if (!string.IsNullOrWhiteSpace(meridiem))
                {
                    meridiem = meridiem.ToLowerInvariant();
                    if (meridiem == "pm" && hour != 12) hour += 12;
                    if (meridiem == "am" && hour == 12) hour = 0;
                }

                var todayTarget = DateTime.Today.AddHours(hour).AddMinutes(minute);
                if (todayTarget <= DateTime.Now)
                    todayTarget = todayTarget.AddDays(1);

                reminder = todayTarget;
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

            // Day of week: "next monday at 9am"
            var dayOfWeekMatch = Regex.Match(lower, @"(?:(next|this)\s+)?(monday|tuesday|wednesday|thursday|friday|saturday|sunday)(?:\s+at\s+(\d{1,2})(?::(\d{2}))?\s*(am|pm)?)?");
            if (dayOfWeekMatch.Success)
            {
                string qualifier = dayOfWeekMatch.Groups[1].Value;
                string dayName = dayOfWeekMatch.Groups[2].Value;
                DayOfWeek targetDay = ParseDayOfWeek(dayName);

                int today = (int)DateTime.Now.DayOfWeek;
                int target = (int)targetDay;
                int daysUntil = (target - today + 7) % 7;

                if (!string.IsNullOrWhiteSpace(qualifier) && qualifier.ToLowerInvariant() == "next")
                {
                    if (daysUntil == 0) daysUntil = 7;
                }

                reminder = DateTime.Now.AddDays(daysUntil).Date;

                if (dayOfWeekMatch.Groups[3].Success && int.TryParse(dayOfWeekMatch.Groups[3].Value, out int hour))
                {
                    int minute = 0;
                    if (dayOfWeekMatch.Groups[4].Success)
                        int.TryParse(dayOfWeekMatch.Groups[4].Value, out minute);

                    string meridiem = dayOfWeekMatch.Groups[5].Value.ToLowerInvariant();
                    if (meridiem == "pm" && hour != 12) hour += 12;
                    if (meridiem == "am" && hour == 12) hour = 0;

                    reminder = reminder.AddHours(hour).AddMinutes(minute);

                    if (daysUntil == 0 && reminder <= DateTime.Now)
                        reminder = reminder.AddDays(7);
                }
                else
                {
                    reminder = reminder.AddHours(9);
                }
                return true;
            }

            // Try standard formats
            if (TryParseStandardReminder(input, out reminder))
                return true;

            // Fallback to DateTime.TryParse
            if (DateTime.TryParse(input, out reminder))
                return true;

            return false;
        }

        /// <summary>
        /// Try to parse standard date/time formats
        /// </summary>
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
                "dd MMMM yyyy HH:mm",
                "yyyy-MM-dd",
                "dd/MM/yyyy",
                "dd-MM-yyyy"
            };

            return DateTime.TryParseExact(
                input,
                formats,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeLocal,
                out reminder);
        }

        /// <summary>
        /// Convert day name to DayOfWeek
        /// </summary>
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

        /// <summary>
        /// Format reminder for user display
        /// </summary>
        public static string FormatReminderForUser(DateTime reminder)
        {
            TimeSpan timeUntil = reminder - DateTime.Now;

            if (timeUntil.TotalMinutes < 1)
                return "in a moment";
            else if (timeUntil.TotalHours < 1)
                return $"in {Math.Ceiling(timeUntil.TotalMinutes)} minutes";
            else if (timeUntil.TotalHours < 24)
                return $"in {Math.Ceiling(timeUntil.TotalHours)} hours";
            else if (timeUntil.TotalDays < 7)
                return $"in {Math.Ceiling(timeUntil.TotalDays)} days at {reminder:h:mm tt}";
            else
                return $"on {reminder:MMMM dd, yyyy} at {reminder:h:mm tt}";
        }
    }
}