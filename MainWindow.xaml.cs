using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CyberAwarenessBot
{
    public partial class MainWindow : Window
    {
        private ChatbotEngine engine;
        private ITaskRepository taskRepository;
        private IActivityLogger activityLogger;
        private TaskAssistantService taskAssistant;
        private QuizService quizService;
        private QuizSession quizSession;
        private NlpProcessor nlp;

        private int logDisplayCount = 5;

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
            AddWelcomeMessage();
            RefreshTasks();
            RefreshActivityLog();
        }

        private void InitializeServices()
        {
            string connectionString = "server=localhost;port=3306;database=cyberawarenessbot;uid=root;pwd=Salome@123;";
            taskRepository = new MySqlTaskRepository(connectionString);
            taskRepository.InitializeDatabase();

            activityLogger = new ActivityLogger();
            taskAssistant = new TaskAssistantService(taskRepository, activityLogger);
            quizService = new QuizService();
            nlp = new NlpProcessor();
        }

        private void InitializeChatbot()
        {
            engine = new ChatbotEngine(
                onSentimentChanged: UpdateSentimentIndicator,
                onMemoryUpdated: UpdateMemoryPanel
            );
        }

        private void LoadAsciiArt()
        {
            AsciiArtBlock.Text = AsciiArt;
        }

        private void AddWelcomeMessage()
        {
            string welcome = engine.GetWelcomeMessage();
            AddBotMessage(welcome);

           
        }
        

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

        private void ScrollToBottom()
        {
            ChatScrollViewer.ScrollToEnd();
        }

        private void SendButtonClick(object sender, RoutedEventArgs e)
        {
            ProcessUserInput();
        }

        private void UserInputTextBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                ProcessUserInput();
        }

        private void ProcessUserInput()
        {
            string input = UserInputTextBox.Text?.Trim();
            if (string.IsNullOrWhiteSpace(input))
                return;

            AddUserMessage(input);
            UserInputTextBox.Clear();

            string response = HandleIntegratedInput(input);
            AddBotMessage(response);
        }

        private string HandleIntegratedInput(string input)
        {
            var intent = nlp.Process(input);
            activityLogger.Log("NLP", $"Input processed with intent '{intent.Intent}': {input}");

            switch (intent.Intent)
            {
                case "showactivitylog":
                    RefreshActivityLog();
                    return BuildActivityLogMessage(logDisplayCount);

                case "startquiz":
                    StartQuiz();
                    return "Quiz started. Look at the Quiz tab and answer the current question.";

                case "showtasks":
                    RefreshTasks();
                    return taskAssistant.BuildTaskListMessage();

                case "addtask":
                    if (!string.IsNullOrWhiteSpace(intent.ExtractedTitle))
                    {
                        var task = taskAssistant.AddTask(intent.ExtractedTitle);
                        RefreshTasks();
                        RefreshActivityLog();
                        return $"Task added successfully: #{task.Id} - {task.Title}\nIf you want, enter a reminder in the Tasks tab using format yyyy-MM-dd HH:mm.";
                    }
                    return "I can add that task. Please provide a title, for example: Add task - Review privacy settings";

                default:
                    break;
            }

            string botReply = engine.ProcessInput(input);
            RefreshActivityLog();
            return botReply;
        }

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
            if (!string.IsNullOrWhiteSpace(reminderInput))
            {
                DateTime parsedReminder;
                if (!TaskAssistantService.TryParseReminder(reminderInput, out parsedReminder))
                {
                    MessageBox.Show("Reminder format not recognized. Use e.g. 2026-06-30 14:00");
                    return;
                }
                reminder = parsedReminder;
            }

            var task = taskAssistant.AddTask(title, description, reminder);

            AddBotMessage($"I've added your task: #{task.Id} - {task.Title}" +
                          (task.ReminderAt.HasValue ? $"\nReminder set for {task.ReminderAt.Value:yyyy-MM-dd HH:mm}." : ""));

            TaskTitleTextBox.Clear();
            TaskDescriptionTextBox.Clear();
            TaskReminderTextBox.Clear();

            RefreshTasks();
            RefreshActivityLog();
        }

        private void RefreshTasksButtonClick(object sender, RoutedEventArgs e)
        {
            RefreshTasks();
        }

        private void RefreshTasks()
        {
            TasksListBox.ItemsSource = null;
            TasksListBox.ItemsSource = taskAssistant.GetTasks();
        }

        private CyberTask GetSelectedTask()
        {
            return TasksListBox.SelectedItem as CyberTask;
        }

        private void CompleteTaskButtonClick(object sender, RoutedEventArgs e)
        {
            var task = GetSelectedTask();
            if (task == null)
            {
                MessageBox.Show("Select a task first.");
                return;
            }

            taskAssistant.CompleteTask(task.Id);
            AddBotMessage($"Task #{task.Id} marked as completed.");
            RefreshTasks();
            RefreshActivityLog();
        }

        private void DeleteTaskButtonClick(object sender, RoutedEventArgs e)
        {
            var task = GetSelectedTask();
            if (task == null)
            {
                MessageBox.Show("Select a task first.");
                return;
            }

            taskAssistant.DeleteTask(task.Id);
            AddBotMessage($"Task #{task.Id} deleted.");
            RefreshTasks();
            RefreshActivityLog();
        }

        private void StartQuizButtonClick(object sender, RoutedEventArgs e)
        {
            StartQuiz();
            AddBotMessage("Quiz started. Answer the question in the Quiz tab.");
        }

        private void StartQuiz()
        {
            quizSession = quizService.CreateDefaultQuiz();
            activityLogger.Log("Quiz Started", "Cybersecurity quiz started.");
            DisplayCurrentQuizQuestion();
            RefreshActivityLog();
        }

        private void DisplayCurrentQuizQuestion()
        {
            if (quizSession == null || quizSession.CurrentQuestion == null)
            {
                QuizQuestionText.Text = "No active quiz.";
                ClearQuizOptions();
                return;
            }

            var q = quizSession.CurrentQuestion;
            QuizQuestionText.Text = $"Question {quizSession.CurrentQuestionIndex + 1}/{quizSession.Questions.Count}: {q.QuestionText}";
            QuizFeedbackText.Text = string.Empty;

            SetOption(Option1Radio, q.Options.Count > 0 ? q.Options[0] : null);
            SetOption(Option2Radio, q.Options.Count > 1 ? q.Options[1] : null);
            SetOption(Option3Radio, q.Options.Count > 2 ? q.Options[2] : null);
            SetOption(Option4Radio, q.Options.Count > 3 ? q.Options[3] : null);
        }

        private void SetOption(RadioButton radio, string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                radio.Visibility = Visibility.Collapsed;
                radio.Content = string.Empty;
                radio.IsChecked = false;
            }
            else
            {
                radio.Visibility = Visibility.Visible;
                radio.Content = text;
                radio.IsChecked = false;
            }
        }

        private void ClearQuizOptions()
        {
            SetOption(Option1Radio, null);
            SetOption(Option2Radio, null);
            SetOption(Option3Radio, null);
            SetOption(Option4Radio, null);
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

            int selectedIndex = GetSelectedQuizOptionIndex();
            if (selectedIndex < 0)
            {
                MessageBox.Show("Select an answer first.");
                return;
            }

            string feedback;
            bool correct = quizService.SubmitAnswer(quizSession, selectedIndex, out feedback);
            QuizFeedbackText.Text = feedback;
            activityLogger.Log("Quiz Answered", $"Question {quizSession.CurrentQuestionIndex + 1} answered. Correct = {correct}.");
            RefreshActivityLog();
        }

        private void NextQuizQuestionButtonClick(object sender, RoutedEventArgs e)
        {
            if (quizSession == null)
            {
                MessageBox.Show("Start the quiz first.");
                return;
            }

            bool moved = quizService.MoveNext(quizSession);

            if (moved)
            {
                DisplayCurrentQuizQuestion();
            }
            else
            {
                QuizQuestionText.Text = "Quiz complete!";
                QuizFeedbackText.Text = quizService.BuildFinalFeedback(quizSession);
                ClearQuizOptions();
                activityLogger.Log("Quiz Completed", $"Quiz completed with score {quizSession.Score}/{quizSession.Questions.Count}.");
                AddBotMessage(quizService.BuildFinalFeedback(quizSession));
                RefreshActivityLog();
            }
        }

        private void RefreshLogButtonClick(object sender, RoutedEventArgs e)
        {
            RefreshActivityLog();
        }

        private void ShowMoreLogButtonClick(object sender, RoutedEventArgs e)
        {
            logDisplayCount += 5;
            RefreshActivityLog();
        }

        private void RefreshActivityLog()
        {
            ActivityLogListBox.ItemsSource = null;
            ActivityLogListBox.ItemsSource = activityLogger.GetRecent(logDisplayCount);
        }

        private string BuildActivityLogMessage(int count)
        {
            var logs = activityLogger.GetRecent(count);
            if (logs.Count == 0)
                return "No activity has been logged yet.";

            return "Here are the most recent actions:\n\n" +
                   string.Join("\n", logs.Select(x => "- " + x.DisplayText));
        }

        private void UpdateSentimentIndicator(string sentiment, string emoji)
        {
            Dispatcher.Invoke(() =>
            {
                SentimentIndicatorText.Text = $"Sentiment: {emoji} {sentiment}";
            });
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