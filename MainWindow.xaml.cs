using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Media;
using System.IO;
using System.Reflection;

namespace CyberAwarenessBot
{
    /// <summary>
    /// Main window for the CyberGuard AI application
    /// </summary>
    public partial class MainWindow : Window
    {
        // Core services - removed 'readonly' from engine since it's assigned in InitializeChatbot()
        private ChatbotEngine engine;
        private ITaskRepository taskRepository;
        private IActivityLogger activityLogger;
        private TaskAssistantService taskAssistant;
        private QuizService quizService;
        private QuizSession quizSession;
        private NlpProcessor nlp;

        // UI state
        private int logDisplayCount = 5;
        private CyberTask pendingTaskWithReminder = null;
        private bool awaitingReminderResponse = false;
        private SoundPlayer greetingSoundPlayer;

        // ASCII Art
        private const string AsciiArt = @"
╔══════════════════════════════╗
║  ██████╗██╗   ██╗██████╗     ║
║ ██╔════╝╚██╗ ██╔╝██╔══██╗    ║
║ ██║      ╚████╔╝ ██████╔╝    ║
║ ██║       ╚██╔╝  ██╔══██╗    ║
║ ╚██████╗   ██║   ██████╔╝    ║
║  ╚═════╝   ╚═╝   ╚═════╝     ║
╚══════════════════════════════╝";

        /// <summary>
        /// Constructor - initializes the window and all services
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            InitializeServices();
            InitializeChatbot();
            LoadAsciiArt();
            PlayGreetingAudio();
            AddWelcomeMessage();
            RefreshTasks();
            RefreshActivityLog();
            InitializeQuizUI();
        }

        /// <summary>
        /// Initializes all core services and database connection
        /// </summary>
        private void InitializeServices()
        {
            try
            {
                // UPDATE THIS WITH YOUR MYSQL PASSWORD
                string connectionString = "server=localhost;port=3306;database=cyberawarenessbot;uid=root;pwd=Salome@123;";

                taskRepository = new MySqlTaskRepository(connectionString);
                taskRepository.InitializeDatabase();

                activityLogger = new ActivityLogger();
                taskAssistant = new TaskAssistantService(taskRepository, activityLogger);
                quizService = new QuizService();
                nlp = new NlpProcessor();

                activityLogger.Log("System", "Application started successfully");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Database Error: {ex.Message}\n\nMake sure MySQL is running and connection string is correct.",
                    "Database Connection Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Initializes the chatbot engine
        /// </summary>
        private void InitializeChatbot()
        {
            engine = new ChatbotEngine(
                onSentimentChanged: UpdateSentimentIndicator,
                onMemoryUpdated: UpdateMemoryPanel
            );
        }

        /// <summary>
        /// Loads the ASCII art into the UI
        /// </summary>
        private void LoadAsciiArt() => AsciiArtBlock.Text = AsciiArt;

        /// <summary>
        /// Plays greeting audio from embedded resource with fallback to file system
        /// </summary>
        private void PlayGreetingAudio()
        {
            try
            {
                // Method 1: Try embedded resource
                var assembly = Assembly.GetExecutingAssembly();
                string resourceName = "CyberAwarenessBot.Resources.GreetingAudio.wav";

                using (var stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream != null)
                    {
                        greetingSoundPlayer = new SoundPlayer(stream);
                        greetingSoundPlayer.Play();
                        activityLogger.Log("Audio", "Greeting audio played from embedded resource");
                        return;
                    }
                }

                // Method 2: Try file system (Resources folder)
                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "GreetingAudio.wav");
                if (File.Exists(filePath))
                {
                    greetingSoundPlayer = new SoundPlayer(filePath);
                    greetingSoundPlayer.Play();
                    activityLogger.Log("Audio", "Greeting audio played from file system");
                    return;
                }

                // Method 3: Try relative path
                string relativePath = Path.Combine("..", "..", "Resources", "GreetingAudio.wav");
                if (File.Exists(relativePath))
                {
                    greetingSoundPlayer = new SoundPlayer(relativePath);
                    greetingSoundPlayer.Play();
                    activityLogger.Log("Audio", "Greeting audio played from relative path");
                    return;
                }

                activityLogger.Log("Audio", "GreetingAudio.wav not found - continuing without audio");
            }
            catch (Exception ex)
            {
                activityLogger.Log("Audio Error", $"Failed to play greeting audio: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Failed to play greeting audio: {ex.Message}");
            }
        }

        /// <summary>
        /// Adds the welcome message to the chat
        /// </summary>
        private void AddWelcomeMessage()
        {
            string welcome = engine.GetWelcomeMessage();
            AddBotMessage(welcome);
        }

        /// <summary>
        /// Initializes the quiz UI with default values
        /// </summary>
        private void InitializeQuizUI()
        {
            QuizQuestionText.Text = "📝 Click 'Start Quiz' to begin the cybersecurity quiz!";

            // Simplified object initialization
            Option1Radio.Content = "A. Ready to start?";
            Option1Radio.Visibility = Visibility.Visible;
            Option1Radio.IsChecked = false;

            Option2Radio.Content = "B. Click Start Quiz";
            Option2Radio.Visibility = Visibility.Visible;
            Option2Radio.IsChecked = false;

            Option3Radio.Content = "C. Test your knowledge";
            Option3Radio.Visibility = Visibility.Visible;
            Option3Radio.IsChecked = false;

            Option4Radio.Content = "D. Learn cybersecurity";
            Option4Radio.Visibility = Visibility.Visible;
            Option4Radio.IsChecked = false;

            QuizFeedbackText.Text = string.Empty;
        }

        // ===================== CHAT UI HELPERS =====================

        /// <summary>
        /// Adds a user message bubble to the chat
        /// </summary>
        /// <param name="message">The user's message</param>
        private void AddUserMessage(string message)
        {
            // Simplified object initialization
            var container = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 6, 0, 0)
            };

            var bubble = new Border
            {
                Background = (Brush)FindResource("UserBubbleBrush"),
                CornerRadius = new CornerRadius(12, 0, 12, 12),
                Padding = new Thickness(14, 10, 14, 10),
                MaxWidth = 480,
                Child = new TextBlock
                {
                    Text = message,
                    Foreground = (Brush)FindResource("AccentGreenBrush"),
                    FontFamily = new FontFamily("Consolas"),
                    FontSize = 13,
                    TextWrapping = TextWrapping.Wrap,
                    LineHeight = 20
                }
            };

            var avatar = new Border
            {
                Width = 36,
                Height = 36,
                CornerRadius = new CornerRadius(18),
                Background = (Brush)FindResource("AccentGreenBrush"),
                Margin = new Thickness(10, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Top,
                Child = new TextBlock
                {
                    Text = "👤",
                    FontSize = 18,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                }
            };

            container.Children.Add(bubble);
            container.Children.Add(avatar);
            ChatPanel.Children.Add(container);
            ScrollToBottom();
        }

        /// <summary>
        /// Adds a bot message bubble to the chat
        /// </summary>
        /// <param name="message">The bot's message</param>
        private void AddBotMessage(string message)
        {
            var container = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 6, 0, 0)
            };

            var avatar = new Border
            {
                Width = 36,
                Height = 36,
                CornerRadius = new CornerRadius(18),
                Background = new SolidColorBrush(Color.FromRgb(0, 191, 255)),
                Margin = new Thickness(0, 0, 10, 0),
                VerticalAlignment = VerticalAlignment.Top,
                Child = new TextBlock
                {
                    Text = "🤖",
                    FontSize = 18,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                }
            };

            var bubble = new Border
            {
                Background = (Brush)FindResource("BotBubbleBrush"),
                CornerRadius = new CornerRadius(0, 12, 12, 12),
                Padding = new Thickness(14, 10, 14, 10),
                MaxWidth = 560,
                Child = new TextBlock
                {
                    Text = message,
                    Foreground = (Brush)FindResource("TextPrimaryBrush"),
                    FontFamily = new FontFamily("Consolas"),
                    FontSize = 13,
                    TextWrapping = TextWrapping.Wrap,
                    LineHeight = 20
                }
            };

            container.Children.Add(avatar);
            container.Children.Add(bubble);
            ChatPanel.Children.Add(container);
            ScrollToBottom();
        }

        /// <summary>
        /// Scrolls the chat view to the bottom
        /// </summary>
        private void ScrollToBottom() => ChatScrollViewer.ScrollToEnd();

        // ===================== INPUT HANDLING =====================

        /// <summary>
        /// Handles the Send button click
        /// </summary>
        private void SendButtonClick(object sender, RoutedEventArgs e) => ProcessUserInput();

        /// <summary>
        /// Handles the Enter key press in the input box
        /// </summary>
        private void UserInputTextBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) ProcessUserInput();
        }

        /// <summary>
        /// Processes the user's input and generates a response
        /// </summary>
        private void ProcessUserInput()
        {
            string input = UserInputTextBox.Text?.Trim();
            if (string.IsNullOrWhiteSpace(input)) return;

            AddUserMessage(input);
            UserInputTextBox.Clear();

            string response = HandleIntegratedInput(input);
            AddBotMessage(response);
        }

        // ===================== MAIN INTEGRATION =====================

        /// <summary>
        /// Handles the integrated input processing with intent recognition
        /// </summary>
        /// <param name="input">The user's input</param>
        /// <returns>The bot's response</returns>
        private string HandleIntegratedInput(string input)
        {
            // Check if we're awaiting a reminder response
            if (awaitingReminderResponse && pendingTaskWithReminder != null)
            {
                return HandleReminderResponse(input);
            }

            var intent = nlp.Process(input);
            activityLogger.Log("NLP", $"Intent '{intent.Intent}' from input: '{input}'");

            switch (intent.Intent)
            {
                case "showactivitylog":
                    RefreshActivityLog();
                    return BuildActivityLogMessage(logDisplayCount);

                case "startquiz":
                    StartQuiz();
                    return "🎯 Quiz started! Look at the Quiz tab and answer the current question.";

                case "showtasks":
                    RefreshTasks();
                    return taskAssistant.BuildTaskListMessage();

                case "remindme":
                    return HandleDirectReminder(intent, input);

                case "addtask":
                    return HandleAddTask(intent, input);

                default:
                    string botReply = engine.ProcessInput(input);
                    RefreshActivityLog();
                    return botReply;
            }
        }

        /// <summary>
        /// Handles direct reminder requests
        /// </summary>
        private string HandleDirectReminder(NlpIntentResult intent, string input)
        {
            try
            {
                string title = intent.ExtractedTitle;
                if (string.IsNullOrWhiteSpace(title))
                    return "What would you like me to remind you about?";

                // Only parse a time if the user actually supplied one in the same message.
                DateTime? reminderTime = null;
                if (!string.IsNullOrWhiteSpace(intent.ExtractedTime))
                    reminderTime = ParseNaturalLanguageTime(intent.ExtractedTime);

                var task = taskAssistant.AddTask(title, null, reminderTime);
                RefreshTasks();
                RefreshActivityLog();

                if (reminderTime.HasValue)
                {
                    return $"✅ Reminder set for {DescribeRelative(reminderTime.Value)}. " +
                           $"I'll remind you to {LowerFirst(task.Title)}.";
                }

                // No time given yet — keep the conversation going and ask for one.
                pendingTaskWithReminder = task;
                awaitingReminderResponse = true;
                activityLogger.Log("Reminder Prompt", $"Asked user for a time for task #{task.Id}");
                Dispatcher.Invoke(() => StatusBar.Text = "⏳ Awaiting reminder time...");

                return $"✅ Reminder set for '{task.Title}'. When would you like to be reminded?";
            }
            catch (Exception ex)
            {
                activityLogger.Log("Error", $"Failed to set reminder: {ex.Message}");
                return $"❌ Error setting reminder: {ex.Message}";
            }
        }

        /// <summary>
        /// Handles adding a new task
        /// </summary>
        private string HandleAddTask(NlpIntentResult intent, string input)
        {
            try
            {
                string taskTitle = intent.ExtractedTitle;
                if (string.IsNullOrWhiteSpace(taskTitle))
                    taskTitle = input.Trim();

                if (string.IsNullOrWhiteSpace(taskTitle))
                    return "What task would you like to add?";

                var newTask = taskAssistant.AddTask(taskTitle);
                RefreshTasks();
                RefreshActivityLog();

                pendingTaskWithReminder = newTask;
                awaitingReminderResponse = true;

                activityLogger.Log("Reminder Prompt", $"Asked user for reminder on task #{newTask.Id}");
                Dispatcher.Invoke(() => StatusBar.Text = "⏳ Awaiting reminder reply...");

                return $"✅ Task added: '{newTask.Title}'. Would you like to set a reminder?\n" +
                       "(Reply 'yes', 'no', or a time like 'in 7 days' or 'tomorrow at 3pm'.)";
            }
            catch (Exception ex)
            {
                activityLogger.Log("Error", $"Failed to add task: {ex.Message}");
                return $"❌ Error adding task: {ex.Message}";
            }
        }

        // ===================== REMINDER HANDLING =====================

        /// <summary>
        /// Handles the user's response to a reminder prompt
        /// </summary>
        private string HandleReminderResponse(string input)
        {
            try
            {
                string lower = input.ToLower().Trim();

                // Handle "no" responses
                if (lower == "no" || lower == "n" || lower.Contains("no reminder") ||
                    lower.Contains("no thanks") || lower.Contains("skip") || lower.Contains("don't"))
                {
                    awaitingReminderResponse = false;
                    string taskTitle = pendingTaskWithReminder.Title;
                    pendingTaskWithReminder = null;
                    Dispatcher.Invoke(() => StatusBar.Text = "");
                    activityLogger.Log("Reminder Skipped", $"User declined reminder for '{taskTitle}'");
                    return $"Okay, I won't set a reminder for '{taskTitle}'.";
                }

                // Try to parse a time from the input first, so combined replies like
                // "Yes, remind me in 7 days" are handled in one step.
                DateTime? reminderTime = ParseNaturalLanguageTime(input);
                if (reminderTime.HasValue)
                {
                    // Set the reminder in the database
                    taskAssistant.SetReminder(pendingTaskWithReminder.Id, reminderTime.Value);
                    RefreshTasks();
                    RefreshActivityLog();

                    string taskTitle = pendingTaskWithReminder.Title;

                    // Clear state
                    awaitingReminderResponse = false;
                    pendingTaskWithReminder = null;
                    Dispatcher.Invoke(() => StatusBar.Text = "");

                    activityLogger.Log("Reminder Set", $"Reminder set for task '{taskTitle}' at {reminderTime.Value:yyyy-MM-dd HH:mm}");

                    return $"✅ Reminder set for {DescribeRelative(reminderTime.Value)}. " +
                           $"I'll remind you to {LowerFirst(taskTitle)}.";
                }

                // Affirmative without a time – ask for the time.
                if (lower == "yes" || lower == "y" || lower == "sure" || lower == "ok" ||
                    lower == "okay" || lower == "yeah" || lower == "yep" || lower.StartsWith("yes"))
                {
                    Dispatcher.Invoke(() => StatusBar.Text = "⏳ Please provide a time...");
                    return "⏰ When would you like to be reminded?\n\nExamples:\n- 'in 7 days'\n- 'tomorrow at 3pm'\n- 'next Monday at 9am'\n- 'Friday 4:30pm'\n- '2026-07-15 10:00'";
                }

                return "❌ I didn't understand that time. Please try again with:\n\n" +
                       "Examples:\n" +
                       "- 'in 7 days'\n" +
                       "- 'tomorrow at 3pm'\n" +
                       "- 'next Monday at 9am'\n" +
                       "- 'Friday 4:30pm'\n" +
                       "- '2026-07-15 10:00'\n" +
                       "- or type 'no' to skip the reminder";
            }
            catch (Exception ex)
            {
                activityLogger.Log("Error", $"Failed to handle reminder response: {ex.Message}");
                awaitingReminderResponse = false;
                pendingTaskWithReminder = null;
                Dispatcher.Invoke(() => StatusBar.Text = "");
                return $"❌ Error setting reminder: {ex.Message}";
            }
        }

        // ===================== PHRASING HELPERS =====================

        /// <summary>
        /// Describes a future time relative to now in friendly language
        /// </summary>
        /// <param name="when">The future date/time</param>
        /// <returns>A human-readable description</returns>
        private static string DescribeRelative(DateTime when)
        {
            TimeSpan diff = when - DateTime.Now;

            if (diff.TotalSeconds <= 0)
                return when.ToString("dddd, MMMM d 'at' h:mm tt");

            int minutes = (int)Math.Round(diff.TotalMinutes);
            if (minutes < 60)
                return $"{minutes} minute{(minutes == 1 ? "" : "s")} from now";

            int hours = (int)Math.Round(diff.TotalHours);
            if (hours < 24)
                return $"{hours} hour{(hours == 1 ? "" : "s")} from now";

            int days = (int)Math.Round(diff.TotalDays);
            if (days <= 14)
                return $"{days} day{(days == 1 ? "" : "s")} from now ({when:dddd, MMMM d 'at' h:mm tt})";

            return when.ToString("dddd, MMMM d, yyyy 'at' h:mm tt");
        }

        /// <summary>
        /// Lowercases the first character of a phrase so it reads naturally mid-sentence
        /// </summary>
        private static string LowerFirst(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            return char.ToLowerInvariant(s[0]) + s.Substring(1);
        }

        // ===================== NATURAL LANGUAGE TIME PARSING =====================

        /// <summary>
        /// Parses natural language time expressions into a DateTime
        /// </summary>
        /// <param name="input">The natural language time string</param>
        /// <returns>A DateTime or null if parsing fails</returns>
        private DateTime? ParseNaturalLanguageTime(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return null;

            string lower = input.ToLower().Trim();
            DateTime now = DateTime.Now;

            // 1. "in X days/hours/weeks/minutes"
            if (lower.Contains("in "))
            {
                string[] parts = lower.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < parts.Length; i++)
                {
                    if (int.TryParse(parts[i], out int number))
                    {
                        if (i + 1 < parts.Length)
                        {
                            string unit = parts[i + 1];
                            if (unit.Contains("day")) return now.AddDays(number);
                            if (unit.Contains("hour")) return now.AddHours(number);
                            if (unit.Contains("week")) return now.AddDays(number * 7);
                            if (unit.Contains("minute")) return now.AddMinutes(number);
                            if (unit.Contains("month")) return now.AddMonths(number);
                        }
                    }
                }
            }

            // 2. "tomorrow" or "tomorrow at 3pm"
            if (lower.Contains("tomorrow"))
            {
                DateTime tomorrow = now.AddDays(1);
                if (TryParseTimeFromText(input, out TimeSpan time))
                    return tomorrow.Date + time;
                return tomorrow.Date.AddHours(9);
            }

            // 3. Day of week: "monday", "next tuesday", "wednesday at 4:30pm"
            string[] dayNames = { "monday", "tuesday", "wednesday", "thursday", "friday", "saturday", "sunday" };
            foreach (string dayName in dayNames)
            {
                if (lower.Contains(dayName))
                {
                    DayOfWeek targetDay = (DayOfWeek)Array.IndexOf(dayNames, dayName);
                    int daysUntil = ((int)targetDay - (int)now.DayOfWeek + 7) % 7;

                    // If "next" qualifier, force next week
                    if (lower.Contains("next " + dayName) || lower.Contains("next " + dayName.Substring(0, 3)))
                    {
                        if (daysUntil == 0) daysUntil = 7;
                    }

                    DateTime targetDate = now.AddDays(daysUntil).Date;

                    if (TryParseTimeFromText(input, out TimeSpan time))
                    {
                        DateTime result = targetDate + time;
                        // If today and time passed, push to next week
                        if (daysUntil == 0 && result <= now && !lower.Contains("next"))
                            result = result.AddDays(7);
                        return result;
                    }
                    else
                    {
                        // No time given, default 9am
                        DateTime result = targetDate.AddHours(9);
                        if (daysUntil == 0 && result <= now && !lower.Contains("next"))
                            result = result.AddDays(7);
                        return result;
                    }
                }
            }

            // 4. "today at 5pm"
            if (lower.Contains("today"))
            {
                if (TryParseTimeFromText(input, out TimeSpan time))
                {
                    DateTime result = now.Date + time;
                    if (result <= now) result = result.AddDays(1);
                    return result;
                }
                return now.Date.AddHours(9);
            }

            // 5. "4 days" (without "in")
            var relativeMatch = Regex.Match(lower, @"(\d+)\s+days?");
            if (relativeMatch.Success && int.TryParse(relativeMatch.Groups[1].Value, out int dayCount))
                return now.AddDays(dayCount);

            // 6. Absolute date/time
            if (DateTime.TryParse(input, out DateTime parsedDate))
                return parsedDate;

            return null;
        }

        /// <summary>
        /// Tries to parse a time from text (e.g., "3pm", "3:30pm", "14:00")
        /// </summary>
        private bool TryParseTimeFromText(string input, out TimeSpan time)
        {
            time = TimeSpan.Zero;
            string lower = input.ToLower();

            // Pattern: "3pm", "3:30pm", "15:30"
            var match = Regex.Match(lower, @"(\d{1,2})(?::(\d{2}))?\s*(am|pm)");
            if (match.Success)
            {
                int hour = int.Parse(match.Groups[1].Value);
                int minute = match.Groups[2].Success ? int.Parse(match.Groups[2].Value) : 0;
                string ampm = match.Groups[3].Value;

                if (ampm == "pm" && hour < 12) hour += 12;
                if (ampm == "am" && hour == 12) hour = 0;

                time = new TimeSpan(hour, minute, 0);
                return true;
            }

            // 24-hour format: "14:00"
            match = Regex.Match(lower, @"(\d{1,2}):(\d{2})");
            if (match.Success)
            {
                int hour = int.Parse(match.Groups[1].Value);
                int minute = int.Parse(match.Groups[2].Value);
                if (hour >= 0 && hour <= 23 && minute >= 0 && minute <= 59)
                {
                    time = new TimeSpan(hour, minute, 0);
                    return true;
                }
            }

            return false;
        }

        // ===================== TASK MANAGEMENT (GUI) =====================

        /// <summary>
        /// Handles the Add Task button click
        /// </summary>
        private void AddTaskButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                string title = TaskTitleTextBox.Text?.Trim();
                string description = TaskDescriptionTextBox.Text?.Trim();
                string reminderInput = TaskReminderTextBox.Text?.Trim();

                if (string.IsNullOrWhiteSpace(title))
                {
                    MessageBox.Show("Please enter a task title.");
                    return;
                }

                DateTime? reminder = null;
                if (!string.IsNullOrWhiteSpace(reminderInput) && DateTime.TryParse(reminderInput, out DateTime parsed))
                    reminder = parsed;

                var task = taskAssistant.AddTask(title, description, reminder);
                AddBotMessage($"✅ Task added: #{task.Id} - {task.Title}" +
                              (task.ReminderAt.HasValue ? $"\n⏰ Reminder set for {task.ReminderAt.Value:yyyy-MM-dd HH:mm}" : ""));
                TaskTitleTextBox.Clear();
                TaskDescriptionTextBox.Clear();
                TaskReminderTextBox.Clear();
                RefreshTasks();
                RefreshActivityLog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding task: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Handles the Refresh Tasks button click
        /// </summary>
        private void RefreshTasksButtonClick(object sender, RoutedEventArgs e) => RefreshTasks();

        /// <summary>
        /// Refreshes the tasks list from the database
        /// </summary>
        private void RefreshTasks()
        {
            try
            {
                TasksListBox.ItemsSource = null;
                TasksListBox.ItemsSource = taskAssistant.GetTasks();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading tasks: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Gets the currently selected task
        /// </summary>
        private CyberTask GetSelectedTask() => TasksListBox.SelectedItem as CyberTask;

        /// <summary>
        /// Handles the Complete Task button click
        /// </summary>
        private void CompleteTaskButtonClick(object sender, RoutedEventArgs e)
        {
            var task = GetSelectedTask();
            if (task == null) { MessageBox.Show("Select a task first."); return; }

            try
            {
                taskAssistant.CompleteTask(task.Id);
                AddBotMessage($"✅ Task '{task.Title}' marked as completed!");
                RefreshTasks();
                RefreshActivityLog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error completing task: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Handles the Delete Task button click
        /// </summary>
        private void DeleteTaskButtonClick(object sender, RoutedEventArgs e)
        {
            var task = GetSelectedTask();
            if (task == null) { MessageBox.Show("Select a task first."); return; }

            try
            {
                taskAssistant.DeleteTask(task.Id);
                AddBotMessage($"🗑️ Task '{task.Title}' deleted.");
                RefreshTasks();
                RefreshActivityLog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting task: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ===================== QUIZ MANAGEMENT =====================

        /// <summary>
        /// Handles the Start Quiz button click
        /// </summary>
        private void StartQuizButtonClick(object sender, RoutedEventArgs e) => StartQuiz();

        /// <summary>
        /// Starts a new quiz session
        /// </summary>
        private void StartQuiz()
        {
            try
            {
                quizSession = quizService.CreateDefaultQuiz();
                if (quizSession == null || quizSession.Questions == null || quizSession.Questions.Count == 0)
                {
                    MessageBox.Show("Failed to load quiz questions.");
                    return;
                }
                activityLogger.Log("Quiz Started", $"Cybersecurity quiz started with {quizSession.Questions.Count} questions.");
                DisplayCurrentQuizQuestion();
                RefreshActivityLog();
                AddBotMessage($"🎯 Quiz started! {quizSession.Questions.Count} questions loaded. Answer in the Quiz tab.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error starting quiz: {ex.Message}", "Quiz Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Displays the current quiz question
        /// </summary>
        private void DisplayCurrentQuizQuestion()
        {
            if (quizSession == null || quizSession.CurrentQuestion == null)
            {
                QuizQuestionText.Text = "No active quiz. Click 'Start Quiz' to begin.";
                ClearQuizOptions();
                QuizFeedbackText.Text = string.Empty;
                return;
            }

            var q = quizSession.CurrentQuestion;
            QuizQuestionText.Text = $"📝 Question {quizSession.CurrentQuestionIndex + 1}/{quizSession.Questions.Count}: {q.QuestionText}";
            QuizFeedbackText.Text = string.Empty;

            ClearQuizOptions();

            if (q.Options.Count >= 1) { Option1Radio.Visibility = Visibility.Visible; Option1Radio.Content = q.Options[0]; Option1Radio.IsChecked = false; }
            if (q.Options.Count >= 2) { Option2Radio.Visibility = Visibility.Visible; Option2Radio.Content = q.Options[1]; Option2Radio.IsChecked = false; }
            if (q.Options.Count >= 3) { Option3Radio.Visibility = Visibility.Visible; Option3Radio.Content = q.Options[2]; Option3Radio.IsChecked = false; }
            if (q.Options.Count >= 4) { Option4Radio.Visibility = Visibility.Visible; Option4Radio.Content = q.Options[3]; Option4Radio.IsChecked = false; }
        }

        /// <summary>
        /// Clears the quiz options
        /// </summary>
        private void ClearQuizOptions()
        {
            Option1Radio.Content = "A. Select an answer";
            Option1Radio.Visibility = Visibility.Visible;
            Option1Radio.IsChecked = false;
            Option2Radio.Content = "B. Select an answer";
            Option2Radio.Visibility = Visibility.Visible;
            Option2Radio.IsChecked = false;
            Option3Radio.Content = "C. Select an answer";
            Option3Radio.Visibility = Visibility.Visible;
            Option3Radio.IsChecked = false;
            Option4Radio.Content = "D. Select an answer";
            Option4Radio.Visibility = Visibility.Visible;
            Option4Radio.IsChecked = false;
        }

        /// <summary>
        /// Gets the selected quiz option index
        /// </summary>
        private int GetSelectedQuizOptionIndex()
        {
            if (Option1Radio.IsChecked == true) return 0;
            if (Option2Radio.IsChecked == true) return 1;
            if (Option3Radio.IsChecked == true) return 2;
            if (Option4Radio.IsChecked == true) return 3;
            return -1;
        }

        /// <summary>
        /// Handles the Submit Quiz Answer button click
        /// </summary>
        private void SubmitQuizAnswerButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (quizSession == null || !quizSession.IsActive)
                {
                    MessageBox.Show("Start the quiz first.");
                    return;
                }

                int selected = GetSelectedQuizOptionIndex();
                if (selected < 0) { MessageBox.Show("Select an answer first."); return; }

                string feedback;
                bool correct = quizService.SubmitAnswer(quizSession, selected, out feedback);
                QuizFeedbackText.Text = feedback;
                activityLogger.Log("Quiz Answered", $"Question {quizSession.CurrentQuestionIndex + 1} answered. Correct = {correct}.");
                RefreshActivityLog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error submitting answer: {ex.Message}", "Quiz Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Handles the Next Quiz Question button click
        /// </summary>
        private void NextQuizQuestionButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (quizSession == null) { MessageBox.Show("Start the quiz first."); return; }

                if (quizService.MoveNext(quizSession))
                    DisplayCurrentQuizQuestion();
                else
                {
                    QuizQuestionText.Text = "🎉 Quiz Complete!";
                    QuizFeedbackText.Text = quizService.BuildFinalFeedback(quizSession);
                    ClearQuizOptions();
                    activityLogger.Log("Quiz Completed", $"Quiz completed with score {quizSession.Score}/{quizSession.Questions.Count}.");
                    AddBotMessage(quizService.BuildFinalFeedback(quizSession));
                    RefreshActivityLog();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error moving to next question: {ex.Message}", "Quiz Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ===================== ACTIVITY LOG =====================

        /// <summary>
        /// Handles the Refresh Log button click
        /// </summary>
        private void RefreshLogButtonClick(object sender, RoutedEventArgs e) => RefreshActivityLog();

        /// <summary>
        /// Handles the Show More Log button click
        /// </summary>
        private void ShowMoreLogButtonClick(object sender, RoutedEventArgs e)
        {
            logDisplayCount += 5;
            RefreshActivityLog();
        }

        /// <summary>
        /// Refreshes the activity log from the database
        /// </summary>
        private void RefreshActivityLog()
        {
            try
            {
                ActivityLogListBox.ItemsSource = null;
                ActivityLogListBox.ItemsSource = activityLogger.GetRecent(logDisplayCount);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading activity log: {ex.Message}");
            }
        }

        /// <summary>
        /// Builds a formatted activity log message
        /// </summary>
        private string BuildActivityLogMessage(int count)
        {
            var logs = activityLogger.GetRecent(count);
            if (logs.Count == 0)
                return "📋 No activity has been logged yet.";

            return "📋 Here are the most recent actions:\n\n" +
                   string.Join("\n", logs.Select(x => "- " + x.DisplayText));
        }

        // ===================== UI UPDATES =====================

        /// <summary>
        /// Updates the sentiment indicator in the UI
        /// </summary>
        private void UpdateSentimentIndicator(string sentiment, string emoji)
        {
            Dispatcher.Invoke(() => SentimentIndicatorText.Text = $"Sentiment: {emoji} {sentiment}");
        }

        /// <summary>
        /// Updates the memory panel in the UI
        /// </summary>
        private void UpdateMemoryPanel(UserMemory memory)
        {
            Dispatcher.Invoke(() =>
            {
                MemoryNameText.Text = $"Name: {memory.UserName ?? "(not set)"}";
                MemoryTopicText.Text = $"Favourite topic: {memory.FavouriteTopic ?? "(not set)"}";
                MemoryLevelText.Text = $"Experience level: {memory.ExperienceLevel ?? "(not set)"}";
            });
        }
    }
}