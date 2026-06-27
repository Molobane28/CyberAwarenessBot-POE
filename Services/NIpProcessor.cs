using System;
using System.Text;

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

            if (ContainsAny(normalized, "complete task", "mark completed", "mark task complete", "done task"))
            {
                result.Intent = "completetask";
                return result;
            }

            if (ContainsAny(normalized, "remind me", "set reminder", "add reminder"))
            {
                result.Intent = "setreminder";
                result.ExtractedTopic = ExtractTopic(normalized);
                return result;
            }

            result.ExtractedTopic = ExtractTopic(normalized);
            return result;
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

            input = input.ToLowerInvariant();

            var sb = new StringBuilder();
            foreach (char c in input)
            {
                if (char.IsLetterOrDigit(c) || char.IsWhiteSpace(c))
                    sb.Append(c);
                else
                    sb.Append(' ');
            }

            string normalized = sb.ToString();
            while (normalized.Contains("  "))
                normalized = normalized.Replace("  ", " ");

            normalized = normalized.Replace("two factor", "2fa");
            normalized = normalized.Replace("2 factor", "2fa");
            normalized = normalized.Replace("multi factor", "2fa");
            normalized = normalized.Replace("mfa", "2fa");

            return normalized.Trim();
        }

        private static string ExtractTaskTitle(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return null;

            string[] prefixes =
            {
                "add task",
                "add a task",
                "create task",
                "new task",
                "task to",
                "set a task"
            };

            string lowered = input.ToLowerInvariant();

            foreach (string prefix in prefixes)
            {
                int idx = lowered.IndexOf(prefix, StringComparison.OrdinalIgnoreCase);
                if (idx >= 0)
                {
                    string title = input.Substring(idx + prefix.Length).Trim(' ', '-', ':');
                    if (!string.IsNullOrWhiteSpace(title))
                        return title;
                }
            }

            return null;
        }

        private static string ExtractTopic(string lower)
        {
            string[] known =
            {
                "password", "phishing", "privacy", "malware", "2fa",
                "scam", "ransomware", "data breach", "breach", "vpn", "encryption"
            };

            foreach (var t in known)
            {
                if (lower.Contains(t))
                    return t;
            }

            return null;
        }
    }
}