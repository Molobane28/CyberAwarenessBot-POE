using System.Collections.Generic;

namespace CyberAwarenessBot
{
    public interface ITaskRepository
    {
        void InitializeDatabase();
        int AddTask(CyberTask task);
        List<CyberTask> GetAllTasks();
        CyberTask GetTaskById(int id);
        void MarkCompleted(int id);
        void DeleteTask(int id);
        void UpdateReminder(int id, System.DateTime? reminderAt);
    }
}