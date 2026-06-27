using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

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
            logger.Log("Reminder Set", $"Reminder updated for task #{id} to {reminderAt:yyyy-MM-dd HH:mm}.");
        }

        public string BuildTaskListMessage()
        {
            var tasks = GetTasks();
            if (tasks.Count == 0)
                return "You currently have no tasks saved.";

            var sb = new StringBuilder();
            sb.AppendLine("Here are your cybersecurity tasks:");
            sb.AppendLine();

            foreach (var task in tasks)
            {
                sb.AppendLine($"- #{task.Id}: {task.Title} [{task.StatusText}]");
                if (!string.IsNullOrWhiteSpace(task.Description))
                    sb.AppendLine($"  Description: {task.Description}");
                sb.AppendLine($"  Reminder: {task.ReminderText}");
            }

            return sb.ToString().Trim();
        }

        public static bool TryParseReminder(string input, out DateTime reminder)
        {
            string[] formats =
            {
                "yyyy-MM-dd HH:mm",
                "yyyy/MM/dd HH:mm",
                "dd/MM/yyyy HH:mm",
                "dd-MM-yyyy HH:mm",
                "yyyy-MM-ddTHH:mm"
            };

            return DateTime.TryParseExact(
                input,
                formats,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out reminder)
                || DateTime.TryParse(input, out reminder);
        }
    }
}
