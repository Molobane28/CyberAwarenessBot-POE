using System.Collections.Generic;

namespace CyberAwarenessBot
{
    public interface IActivityLogger
    {
        void Log(string actionType, string message);
        List<ActivityLogEntry> GetRecent(int count = 10);
        List<ActivityLogEntry> GetAll();
    }
}