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

            // =============================================
            // ACTIVITY LOG
            // =============================================
            if (ContainsAny(normalized,
                "show activity log", "activity log", "what have you done for me",
                "show log", "recent actions", "log", "what have you been doing",
                "show activity", "show history", "history", "actions"))
            {
                result.Intent = "showactivitylog";
                return result;
            }

            // =============================================
            // QUIZ
            // =============================================
            if (ContainsAny(normalized,
                "start quiz", "quiz", "play quiz", "start game", "play game",
                "take quiz", "do quiz", "begin quiz", "let's play", "test me",
                "cybersecurity quiz", "security quiz", "question time"))
            {
                result.Intent = "startquiz";
                return result;
            }

            // =============================================
            // SHOW TASKS
            // =============================================
            if (ContainsAny(normalized,
                "show tasks", "view tasks", "list tasks", "my tasks",
                "show all tasks", "display tasks", "tasks", "what tasks",
                "get tasks", "task list", "show my tasks"))
            {
                result.Intent = "showtasks";
                return result;
            }

            // =============================================
            // DELETE TASK
            // =============================================
            if (ContainsAny(normalized,
                "delete task", "remove task", "delete the task", "remove the task",
                "erase task", "delete a task", "remove a task", "task delete"))
            {
                result.Intent = "deletetask";
                return result;
            }

            // =============================================
            // COMPLETE TASK
            // =============================================
            if (ContainsAny(normalized,
                "complete task", "mark completed", "mark task complete", "done task",
                "finish task", "task done", "mark as done", "complete", "finish"))
            {
                result.Intent = "completetask";
                return result;
            }

            // =============================================
            // REMIND ME
            // =============================================
            if (normalized.Contains("remind me") ||
                normalized.Contains("remind me to") ||
                normalized.Contains("remind me about") ||
                normalized.Contains("set reminder for") ||
                normalized.Contains("remind"))
            {
                result.Intent = "remindme";
                result.ExtractedTitle = ExtractReminderTitle(input);
                result.ExtractedTime = ExtractTimeFromInput(input);
                return result;
            }

            // =============================================
            // ADD TASK – with many variations
            // =============================================
            if (ContainsAny(normalized,
                "add task", "create task", "new task", "task to", "set a task", "add a task",
                "add a new task", "create a task", "set up a task", "setup a task",
                "make a task", "make task", "i want to add", "i need to add",
                "add", "create", "set up", "setup", "set", "new", "new task for",
                "add task to", "create task for", "task create", "task add"))
            {
                result.Intent = "addtask";
                result.ExtractedTitle = ExtractTaskTitle(input);
                result.ExtractedTopic = ExtractTopic(normalized);
                return result;
            }

            // =============================================
            // HELP
            // =============================================
            if (ContainsAny(normalized,
                "help", "what can you do", "commands", "what do you do",
                "how to use", "help me", "help with", "what options"))
            {
                result.Intent = "help";
                return result;
            }

            // =============================================
            // FALLBACK
            // =============================================
            result.ExtractedTopic = ExtractTopic(normalized);
            return result;
        }

        // ===================== HELPER METHODS =====================

        private static bool ContainsAny(string input, params string[] patterns)
        {
            foreach (var pattern in patterns)
                if (input.Contains(pattern)) return true;
            return false;
        }

        private static string Normalize(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;

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
            while (normalized.Contains("  ")) normalized = normalized.Replace("  ", " ");

            normalized = normalized.Replace("two factor", "2fa");
            normalized = normalized.Replace("2 factor", "2fa");
            normalized = normalized.Replace("multi factor", "2fa");
            normalized = normalized.Replace("mfa", "2fa");
            normalized = normalized.Replace("two-factor", "2fa");
            normalized = normalized.Replace("set up", "setup");

            return normalized.Trim();
        }

        private static string ExtractTaskTitle(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return null;

            string[] prefixes = {
                "add task", "add a task", "create task", "new task", "task to",
                "set a task", "add a new task", "create a task", "set up a task",
                "setup a task", "make a task", "i want to add", "i need to add",
                "set up", "setup", "add", "create", "set", "new",
                "add a", "create a", "make a", "task create", "task add"
            };

            string lowered = input.ToLowerInvariant();

            foreach (string prefix in prefixes)
            {
                int idx = lowered.IndexOf(prefix, StringComparison.OrdinalIgnoreCase);
                if (idx >= 0)
                {
                    string title = CleanTitle(input.Substring(idx + prefix.Length));
                    if (!string.IsNullOrWhiteSpace(title)) return title;
                }
            }

            return CleanTitle(input);
        }

        /// <summary>
        /// Trims surrounding punctuation and strips leading connector words
        /// (e.g. "to review my privacy settings" -> "review my privacy settings")
        /// so extracted task/reminder titles read naturally.
        /// </summary>
        private static string CleanTitle(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return s;

            s = s.Trim(' ', '-', ':', '.', '?', '!', ',');

            string[] leading = { "to ", "a ", "an ", "the ", "for ", "about ", "that " };
            bool changed = true;
            while (changed)
            {
                changed = false;
                foreach (var w in leading)
                {
                    if (s.StartsWith(w, StringComparison.OrdinalIgnoreCase))
                    {
                        s = s.Substring(w.Length).Trim();
                        changed = true;
                    }
                }
            }

            s = s.Trim(' ', '-', ':', '.', '?', '!', ',');

            // Capitalise the first letter so titles read like "Review my privacy settings".
            if (s.Length > 0)
                s = char.ToUpperInvariant(s[0]) + s.Substring(1);

            return s;
        }

        private static string ExtractReminderTitle(string input)
        {
            string lowered = input.ToLowerInvariant();

            string[] markers = {
                "remind me to", "remind me about", "remind me",
                "set a reminder to", "set a reminder for", "set a reminder about",
                "set reminder to", "set reminder for", "set reminder about",
                "create a reminder to", "create a reminder for", "create a reminder about",
                "new reminder to", "new reminder for", "new reminder about",
                "reminder to", "reminder for", "reminder about"
            };

            foreach (var marker in markers)
            {
                int idx = lowered.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
                if (idx >= 0)
                {
                    string rest = input.Substring(idx + marker.Length).Trim();
                    return CleanTitle(StripTrailingTimePhrase(rest));
                }
            }

            return CleanTitle(input);
        }

        // Time words that, when they appear as a standalone word, mark the start
        // of a time expression (e.g. "update my password in 3 days").
        private static readonly string[] TimeWords =
            { "tomorrow", "today", "tonight", "next", "in", "on", "at", "by", "this" };

        /// <summary>
        /// Removes a trailing time expression from a title using whole-word matching,
        /// so "enable authentication" is not truncated at the "at" inside the word.
        /// </summary>
        private static string StripTrailingTimePhrase(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return text;

            string[] words = text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < words.Length; i++)
            {
                string w = words[i].Trim(',', '.', '?', '!').ToLowerInvariant();
                if (Array.IndexOf(TimeWords, w) >= 0)
                    return string.Join(" ", words, 0, i);
            }
            return text;
        }

        private static string ExtractTimeFromInput(string input)
        {
            string[] words = input.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < words.Length; i++)
            {
                string w = words[i].Trim(',', '.', '?', '!').ToLowerInvariant();
                if (Array.IndexOf(TimeWords, w) >= 0)
                {
                    string timePart = string.Join(" ", words, i, words.Length - i).Trim();
                    if (timePart.Length < 50)
                        return timePart;
                }
            }

            return null;
        }

        private static string ExtractTopic(string lower)
        {
            string[] known = {
                "password", "phishing", "privacy", "malware", "2fa",
                "scam", "ransomware", "data breach", "breach",
                "vpn", "encryption", "security", "authentication",
                "backup", "software", "update", "browsing", "wifi",
                "social engineering", "engineering", "firewall",
                "antivirus", "spyware", "trojan", "virus"
            };

            foreach (var t in known)
                if (lower.Contains(t)) return t;

            return null;
        }
    }
}