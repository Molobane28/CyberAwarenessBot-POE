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
    public partial class MainWindow : Window
    {
        // Core services
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Additional startup logic if needed
        }

        private void InitializeServices()
        {
            try
            {
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

        private void InitializeChatbot()
        {
            engine = new ChatbotEngine(
                onSentimentChanged: UpdateSentimentIndicator,
                onMemoryUpdated: UpdateMemoryPanel
            );
        }

        private void LoadAsciiArt() => AsciiArtBlock.Text = AsciiArt;

        private void PlayGreetingAudio()
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                string resourceName = "CyberAwarenessBot.Resources.GreetingAudio.wav";
                using (var stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream != null)
                    {
                        greetingSoundPlayer = new SoundPlayer(stream);
                        greetingSoundPlayer.Play();
                    }
                    else
                    {
                        string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "GreetingAudio.wav");
                        if (File.Exists(filePath))
                        {
                            greetingSoundPlayer = new SoundPlayer(filePath);
                            greetingSoundPlayer.Play();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to play greeting audio: {ex.Message}");
            }
        }

        private void AddWelcomeMessage()
        {
            string welcome = engine.GetWelcomeMessage();
            AddBotMessage(welcome);
        }

        private void InitializeQuizUI()
        {
            QuizQuestionText.Text = "📝 Click 'Start Quiz' to begin the cybersecurity quiz!";
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

        private void AddUserMessage(string message)
        {
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
                MaxWidth = 480
            };

            bubble.Child = new TextBlock
            {
                Text = message,
                Foreground = (Brush)FindResource("AccentGreenBrush"),
                FontFamily = new FontFamily("Consolas"),
                FontSize = 13,
                TextWrapping = TextWrapping.Wrap,
                LineHeight = 20
            };

            var avatar = new Border
            {
                Width = 36,
                Height = 36,
                CornerRadius = new CornerRadius(18),
                Background = (Brush)FindResource("AccentGreenBrush"),
                Margin = new Thickness(10, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Top
            };

            avatar.Child = new TextBlock
            {
                Text = "👤",
                FontSize = 18,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            container.Children.Add(bubble);
            container.Children.Add(avatar);
            ChatPanel.Children.Add(container);
            ScrollToBottom();
        }

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
                VerticalAlignment = VerticalAlignment.Top
            };

            avatar.Child = new TextBlock
            {
                Text = "🤖",
                FontSize = 18,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            var bubble = new Border
            {
                Background = (Brush)FindResource("BotBubbleBrush"),
                CornerRadius = new CornerRadius(0, 12, 12, 12),
                Padding = new Thickness(14, 10, 14, 10),
                MaxWidth = 560
            };

            bubble.Child = new TextBlock
            {
                Text = message,
                Foreground = (Brush)FindResource("TextPrimaryBrush"),
                FontFamily = new FontFamily("Consolas"),
                FontSize = 13,
                TextWrapping = TextWrapping.Wrap,
                LineHeight = 20
            };

            container.Children.Add(avatar);
            container.Children.Add(bubble);
            ChatPanel.Children.Add(container);
            ScrollToBottom();
        }

        private void ScrollToBottom() => ChatScrollViewer.ScrollToEnd();

        // ===================== INPUT HANDLING =====================

        private void SendButtonClick(object sender, RoutedEventArgs e) => ProcessUserInput();

        private void UserInputTextBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) ProcessUserInput();
        }

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

        private string HandleDirectReminder(NlpIntentResult intent, string input)
        {
            string title = intent.ExtractedTitle;
            if (string.IsNullOrWhiteSpace(title))
                title = input.Replace("remind me", "").Replace("remind me to", "").Trim();

            if (string.IsNullOrWhiteSpace(title))
                return "What would you like me to remind you about?";

            DateTime? reminderTime = ParseNaturalLanguageTime(intent.ExtractedTime ?? "tomorrow at 9am");
            if (!reminderTime.HasValue)
                reminderTime = DateTime.Now.AddDays(1).Date.AddHours(9);

            var task = taskAssistant.AddTask(title, null, reminderTime);
            RefreshTasks();
            RefreshActivityLog();

            string formatted = reminderTime.Value.ToString("dddd, MMMM d, yyyy at h:mm tt");
            return $"✅ Reminder set for '{task.Title}' on {formatted}.";
        }

        private string HandleAddTask(NlpIntentResult intent, string input)
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

            return $"✅ Task added: #{newTask.Id} - {taskTitle}\n\n⏰ Would you like to set a reminder?\n(Reply with: yes, no, or a time like 'in 3 days', 'tomorrow at 3pm', 'Monday at 10am')";
        }

        // ===================== REMINDER HANDLING =====================

        private string HandleReminderResponse(string input)
        {
            string lower = input.ToLower().Trim();

            if (lower == "no" || lower == "n" || lower.Contains("no reminder") || lower.Contains("no thanks") || lower.Contains("skip"))
            {
                awaitingReminderResponse = false;
                string taskTitle = pendingTaskWithReminder.Title;
                pendingTaskWithReminder = null;
                Dispatcher.Invoke(() => StatusBar.Text = "");
                activityLogger.Log("Reminder Skipped", $"User declined reminder for '{taskTitle}'");
                return $"Okay, I won't set a reminder for '{taskTitle}'.";
            }

            if (lower == "yes" || lower == "y" || lower == "sure" || lower == "ok" || lower == "yeah")
            {
                Dispatcher.Invoke(() => StatusBar.Text = "⏳ Please provide a time...");
                return "⏰ What time would you like to be reminded?\n\nExamples:\n- '4 days'\n- 'in 3 days'\n- 'tomorrow at 3pm'\n- 'next Monday at 9am'\n- 'Wednesday'\n- 'Friday 4:30pm'\n- '2026-07-15 10:00'";
            }

            DateTime? reminderTime = ParseNaturalLanguageTime(input);
            if (reminderTime.HasValue)
            {
                taskAssistant.SetReminder(pendingTaskWithReminder.Id, reminderTime.Value);
                RefreshTasks();
                RefreshActivityLog();

                string formattedTime = reminderTime.Value.ToString("dddd, MMMM d, yyyy at h:mm tt");
                string taskTitle = pendingTaskWithReminder.Title;

                awaitingReminderResponse = false;
                pendingTaskWithReminder = null;
                Dispatcher.Invoke(() => StatusBar.Text = "");

                activityLogger.Log("Reminder Set", $"Reminder set for task #{pendingTaskWithReminder?.Id ?? 0} at {formattedTime}");

                return $"✅ Got it! I'll remind you about '{taskTitle}' on {formattedTime}.";
            }
            else
            {
                return "❌ I didn't understand that time format. Please try again with:\n\n" +
                       "Examples:\n" +
                       "- '4 days'\n" +
                       "- 'in 3 days'\n" +
                       "- 'tomorrow at 3pm'\n" +
                       "- 'next Monday at 9am'\n" +
                       "- 'Wednesday'\n" +
                       "- 'Friday 4:30pm'\n" +
                       "- '2026-07-15 10:00'\n" +
                       "- or type 'no' to skip the reminder";
            }
        }

        // ===================== NATURAL LANGUAGE TIME PARSING =====================

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

        private bool TryParseTimeFromText(string input, out TimeSpan time)
        {
            time = TimeSpan.Zero;
            string lower = input.ToLower();

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

        private void AddTaskButtonClick(object sender, RoutedEventArgs e)
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

        private void RefreshTasksButtonClick(object sender, RoutedEventArgs e) => RefreshTasks();

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

        private CyberTask GetSelectedTask() => TasksListBox.SelectedItem as CyberTask;

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

        private void StartQuizButtonClick(object sender, RoutedEventArgs e) => StartQuiz();

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

        private int GetSelectedQuizOptionIndex()
        {
            if (Option1Radio.IsChecked == true) return 0;
            if (Option2Radio.IsChecked == true) return 1;
            if (Option3Radio.IsChecked == true) return 2;
            if (Option4Radio.IsChecked == true) return 3;
            return -1;
        }

        private void SubmitQuizAnswerButtonClick(object sender, RoutedEventArgs e)
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

        private void NextQuizQuestionButtonClick(object sender, RoutedEventArgs e)
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

        // ===================== ACTIVITY LOG =====================

        private void RefreshLogButtonClick(object sender, RoutedEventArgs e) => RefreshActivityLog();

        private void ShowMoreLogButtonClick(object sender, RoutedEventArgs e)
        {
            logDisplayCount += 5;
            RefreshActivityLog();
        }

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

        private string BuildActivityLogMessage(int count)
        {
            var logs = activityLogger.GetRecent(count);
            if (logs.Count == 0)
                return "📋 No activity has been logged yet.";

            return "📋 Here are the most recent actions:\n\n" +
                   string.Join("\n", logs.Select(x => "- " + x.DisplayText));
        }

        // ===================== UI UPDATES =====================

        private void UpdateSentimentIndicator(string sentiment, string emoji)
        {
            Dispatcher.Invoke(() => SentimentIndicatorText.Text = $"Sentiment: {emoji} {sentiment}");
        }

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