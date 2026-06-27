using System;
using System.Text.RegularExpressions;

namespace CyberAwarenessBot
{
    public class NlpProcessor
    {
        public NlpIntentResult Process(string input)
        {
            string normalized = Normalize(input);

            var result = new NlpIntentResult
            {
                RawInput = input,
                Intent = "unknown",
                WantsReminder = normalized.Contains("reminder")
            };

            // Check for explicit "no" to reminder
            if (ContainsAny(normalized, "no", "nope", "skip", "no reminder", "no thanks", "don't need", "cancel reminder"))
            {
                result.Intent = "noreminder";
                return result;
            }

            // Check for reminder time expressions
            if (IsReminderTimeExpression(normalized))
            {
                result.Intent = "remindertime";
                result.ExtractedTopic = input; // Store raw input for time parsing
                return result;
            }

            // Check for yes/affirmative to reminder prompt
            if (ContainsAny(normalized, "yes", "yeah", "sure", "ok", "okay", "yep", "please", "set reminder", "remind me"))
            {
                result.Intent = "yesreminder";
                return result;
            }

            if (ContainsAny(normalized, "show activity log", "activity log", "what have you done for me", "show log", "recent actions"))
            {
                result.Intent = "showactivitylog";
                return result;
            }

            if (ContainsAny(normalized, "start quiz", "quiz", "play quiz", "start game", "play game"))
            {
                result.Intent = "startquiz";
                return result;
            }

            if (ContainsAny(normalized, "show tasks", "view tasks", "list tasks", "my tasks"))
            {
                result.Intent = "showtasks";
                return result;
            }

            if (ContainsAny(normalized, "add task", "create task", "new task", "task to", "set a task", "add a task"))
            {
                result.Intent = "addtask";
                result.ExtractedTitle = ExtractTaskTitle(input);
                result.ExtractedTopic = ExtractTopic(normalized);
                return result;
            }

            if (ContainsAny(normalized, "delete task", "remove task"))
            {
                result.Intent = "deletetask";
                return result;
            }

            if (ContainsAny(normalized, "complete task", "mark completed", "mark task complete", "done task", "complete"))
            {
                result.Intent = "completetask";
                return result;
            }

            result.ExtractedTopic = ExtractTopic(normalized);
            return result;
        }

        // Check if input contains time expressions like "in 3 days", "tomorrow", "next Monday", etc.
        private static bool IsReminderTimeExpression(string normalized)
        {
            var timePatterns = new[]
            {
                @"in\s+\d+\s+(days?|weeks?|hours?)",
                @"tomorrow",
                @"next\s+(monday|tuesday|wednesday|thursday|friday|saturday|sunday)",
                @"at\s+\d{1,2}(:\d{2})?\s*(am|pm)?",
                @"\d{4}-\d{2}-\d{2}",
                @"\d{1,2}/\d{1,2}/\d{4}",
                @"\d{1,2}\s+[a-zA-Z]+\s+\d{4}"
            };

            foreach (var pattern in timePatterns)
            {
                if (Regex.IsMatch(normalized, pattern, RegexOptions.IgnoreCase))
                    return true;
            }

            return false;
        }

        private static bool ContainsAny(string input, params string[] patterns)
        {
            foreach (var pattern in patterns)
            {
                if (input.Contains(pattern))
                    return true;
            }
            return false;
        }

        private static string Normalize(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            return input.ToLowerInvariant().Trim();
        }

        private static string ExtractTaskTitle(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return null;

            string lower = input.ToLowerInvariant();
            
            var patterns = new[] { "add task", "create task", "new task", "add a task" };
            foreach (var pattern in patterns)
            {
                int idx = lower.IndexOf(pattern);
                if (idx >= 0)
                {
                    string afterPattern = input.Substring(idx + pattern.Length).Trim();
                    if (afterPattern.StartsWith("-"))
                        afterPattern = afterPattern.Substring(1).Trim();
                    
                    return afterPattern.Length > 0 ? afterPattern : null;
                }
            }

            // Fallback: if input contains '-' use part after
            int dash = input.IndexOf('-');
            if (dash >= 0 && dash < input.Length - 1)
            {
                string after = input.Substring(dash + 1).Trim();
                return after.Length > 0 ? after : null;
            }

            return null;
        }

        private static string ExtractTopic(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return null;

            string[] topics = new[] 
            { 
                "password", "phishing", "privacy", "malware", "2fa", "two factor", 
                "data breach", "scam", "ransomware", "vpn", "encryption", 
                "firewall", "antivirus", "backup"
            };

            foreach (var topic in topics)
            {
                if (input.Contains(topic))
                    return topic;
            }
            return null;
        }
    }
}