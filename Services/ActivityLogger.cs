using System;
using System.Collections.Generic;

namespace CyberAwarenessBot
{
    /// <summary>
    /// Service for logging application activities with timestamps
    /// </summary>
    public class ActivityLogger : IActivityLogger
    {
        private readonly List<ActivityLogEntry> _logs = new List<ActivityLogEntry>();
        private readonly int _maxLogSize = 100; // Prevent memory issues

        /// <summary>
        /// Log an action with timestamp
        /// </summary>
        public void Log(string actionType, string message)
        {
            try
            {
                var entry = new ActivityLogEntry
                {
                    Timestamp = DateTime.Now,
                    ActionType = actionType,
                    Message = message
                };

                lock (_logs) // Thread-safe
                {
                    _logs.Add(entry);

                    // Trim if too many entries
                    if (_logs.Count > _maxLogSize)
                    {
                        _logs.RemoveRange(0, _logs.Count - _maxLogSize);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to log activity: {ex.Message}");
            }
        }

        /// <summary>
        /// Get recent log entries (most recent first)
        /// </summary>
        public List<ActivityLogEntry> GetRecent(int count = 10)
        {
            try
            {
                lock (_logs)
                {
                    if (count <= 0) count = 5;
                    if (count > _logs.Count) count = _logs.Count;

                    // Return most recent entries in descending order (newest first)
                    var result = new List<ActivityLogEntry>();
                    for (int i = _logs.Count - 1; i >= 0 && result.Count < count; i--)
                    {
                        result.Add(_logs[i]);
                    }
                    return result;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to retrieve logs: {ex.Message}");
                return new List<ActivityLogEntry>();
            }
        }

        /// <summary>
        /// Get all log entries
        /// </summary>
        public List<ActivityLogEntry> GetAll()
        {
            try
            {
                lock (_logs)
                {
                    return new List<ActivityLogEntry>(_logs);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to retrieve all logs: {ex.Message}");
                return new List<ActivityLogEntry>();
            }
        }

        /// <summary>
        /// Clear all logs (for debugging)
        /// </summary>
        public void Clear()
        {
            lock (_logs)
            {
                _logs.Clear();
            }
        }
    }
}