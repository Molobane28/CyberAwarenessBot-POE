using System;
using System.Collections.Generic;
using System.Linq;

namespace CyberAwarenessBot
{
    public class ActivityLogger : IActivityLogger
    {
        private readonly List<ActivityLogEntry> entries = new List<ActivityLogEntry>();

        public void Log(string actionType, string message)
        {
            entries.Add(new ActivityLogEntry
            {
                Timestamp = DateTime.Now,
                ActionType = actionType,
                Message = message
            });
        }

        public List<ActivityLogEntry> GetRecent(int count = 10)
        {
            return entries
                .OrderByDescending(x => x.Timestamp)
                .Take(count)
                .ToList();
        }

        public List<ActivityLogEntry> GetAll()
        {
            return entries
                .OrderByDescending(x => x.Timestamp)
                .ToList();
        }
    }
}