// Summary of comments:
// - Each following line is annotated with a short comment describing its purpose.
// - Comments explain fields, methods, event handlers, UI updates, and control wiring.
// - This file contains the main window implementation for rendering chat messages and handling user interaction.

using System; // Fundamental types and exceptions
using System.IO; // File and path operations for resources like greeting.wav
using System.Media; // SoundPlayer and system sounds for audio playback
using System.Windows; // WPF core classes like Window and MessageBox
using System.Windows.Controls; // WPF controls such as StackPanel, Border, TextBlock
using System.Windows.Input; // Input handling types like KeyEventArgs and Keyboard
using System.Windows.Media; // Brushes, Colors, and FontFamily

namespace CyberAwarenessBot // Application namespace grouping related classes
{
    public partial class MainWindow : Window // Main window class, partial because XAML generates part
    {
        private ChatbotEngine _engine; // Backend engine instance for processing input and producing responses
        private SoundPlayer _greetingPlayer; // SoundPlayer used to play greeting.wav when requested

        private const string AsciiArt = @"
╔══════════════════════════════╗
║  ██████╗██╗   ██╗██████╗     ║
║ ██╔════╝╚██╗ ██╔╝██╔══██╗    ║
║ ██║      ╚████╔╝ ██████╔╝    ║
║ ██║       ╚██╔╝  ██╔══██╗    ║
║ ╚██████╗   ██║   ██████╔╝    ║
║  ╚═════╝   ╚═╝   ╚═════╝     ║
╚══════════════════════════════╝"; // ASCII art shown in the UI welcome panel

        public MainWindow() // Constructor for the main window
        {
            InitializeComponent(); // Initialize components defined in XAML
            InitializeChatbot(); // Create and configure the chatbot engine
            LoadAsciiArt(); // Load ascii art string into the UI element
            AddWelcomeMessage(); // Add initial bot welcome message to chat
        }

        private void InitializeChatbot() // Set up the ChatbotEngine with callbacks
        {
            _engine = new ChatbotEngine(
                onSentimentChanged: UpdateSentimentIndicator, // Provide method to update sentiment UI
                onMemoryUpdated: UpdateMemoryPanel // Provide method to refresh memory panel UI
            );
        }

        private void LoadAsciiArt() // Assign ASCII art string to the UI text block
        {
            AsciiArtBlock.Text = AsciiArt; // Set Text property of AsciiArtBlock control
        }

        private void AddWelcomeMessage() // Insert initial chatbot welcome message into the chat
        {
            string welcome = _engine.GetWelcomeMessage(); // Get welcome text from engine
            AddBotMessage(welcome); // Render bot message in the chat panel
        }

        // ── UI Rendering Methods ───────────────────────────────────

        private void AddUserMessage(string message) // Render a user message bubble in the chat
        {
            var container = new StackPanel // Container holding bubble and avatar horizontally
            {
                Orientation = Orientation.Horizontal, // Layout children side-by-side
                HorizontalAlignment = HorizontalAlignment.Right, // Align user messages to the right
                Margin = new Thickness(0, 6, 0, 0) // Top margin between messages
            };

            var bubble = new Border // Visual bubble for user message
            {
                Background = (Brush)FindResource("UserBubbleBrush"), // Use resource for bubble background
                CornerRadius = new CornerRadius(12, 0, 12, 12), // Rounded corners style
                Padding = new Thickness(14, 10, 14, 10), // Inner padding for text
                MaxWidth = 480 // Prevent overly wide bubbles
            };

            bubble.Child = new TextBlock // Text block placed inside the bubble
            {
                Text = message, // Message text content
                Foreground = (Brush)FindResource("AccentGreenBrush"), // Text color resource
                FontFamily = new FontFamily("Consolas"), // Monospace font for chat text
                FontSize = 13, // Text size
                TextWrapping = TextWrapping.Wrap, // Wrap long text into multiple lines
                LineHeight = 20 // Line height for readability
            };

            var avatar = new Border // Circular avatar next to user's message
            {
                Width = 36, // Avatar width
                Height = 36, // Avatar height
                CornerRadius = new CornerRadius(18), // Make avatar circular
                Background = (Brush)FindResource("AccentGreenBrush"), // Avatar background color
                Margin = new Thickness(10, 0, 0, 0), // Space between bubble and avatar
                VerticalAlignment = VerticalAlignment.Top // Align avatar to top of message
            };

            avatar.Child = new TextBlock // Emoji avatar content
            {
                Text = "👤", // User emoji
                FontSize = 18, // Emoji font size
                HorizontalAlignment = HorizontalAlignment.Center, // Center emoji horizontally
                VerticalAlignment = VerticalAlignment.Center // Center emoji vertically
            };

            container.Children.Add(bubble); // Add message bubble to container
            container.Children.Add(avatar); // Add avatar to container
            ChatPanel.Children.Add(container); // Append container to chat panel UI
            ScrollToBottom(); // Ensure chat scrolls to show latest message
        }

        private void AddBotMessage(string message) // Render a bot message bubble in the chat
        {
            var container = new StackPanel // Container for bot avatar and bubble
            {
                Orientation = Orientation.Horizontal, // Place avatar and bubble side-by-side
                Margin = new Thickness(0, 6, 0, 0) // Top margin between messages
            };

            var avatar = new Border // Bot avatar circle
            {
                Width = 36, // Avatar width
                Height = 36, // Avatar height
                CornerRadius = new CornerRadius(18), // Circular avatar
                Background = new SolidColorBrush(Color.FromRgb(0, 191, 255)), // Fixed cyan-like background for bot
                Margin = new Thickness(0, 0, 10, 0), // Space between avatar and bubble
                VerticalAlignment = VerticalAlignment.Top // Align avatar to top
            };

            avatar.Child = new TextBlock // Emoji representing bot
            {
                Text = "🤖", // Robot emoji
                FontSize = 18, // Emoji size
                HorizontalAlignment = HorizontalAlignment.Center, // Center horizontally
                VerticalAlignment = VerticalAlignment.Center // Center vertically
            };

            var bubble = new Border // Visual bubble for bot message
            {
                Background = (Brush)FindResource("BotBubbleBrush"), // Bot bubble resource
                CornerRadius = new CornerRadius(0, 12, 12, 12), // Rounded corners style for bot
                Padding = new Thickness(14, 10, 14, 10), // Inner padding
                MaxWidth = 560 // Max width for bot bubble
            };

            bubble.Child = new TextBlock // Text content inside bot bubble
            {
                Text = message, // Bot message text
                Foreground = (Brush)FindResource("TextPrimaryBrush"), // Primary text color resource
                FontFamily = new FontFamily("Consolas"), // Monospace font for consistency
                FontSize = 13, // Text size
                TextWrapping = TextWrapping.Wrap, // Wrap long text
                LineHeight = 20 // Readable line height
            };

            container.Children.Add(avatar); // Add avatar to container first for bot
            container.Children.Add(bubble); // Add bubble next to avatar
            ChatPanel.Children.Add(container); // Append to chat panel
            ScrollToBottom(); // Scroll to newest message
        }

        private void ScrollToBottom() // Ensure chat scroll viewer scrolls to bottom
        {
            ChatScrollViewer.Dispatcher.BeginInvoke(new Action(() => // Invoke on UI dispatcher thread
            {
                ChatScrollViewer.ScrollToBottom(); // Scroll action
            })); // End dispatcher invocation
        }

        // ── Event Handlers ────────────────────────────────────────

        private void SendButton_Click(object sender, RoutedEventArgs e) // Handler for send button click
        {
            SendMessage(); // Delegate to shared send method
        }

        private void UserInputBox_KeyDown(object sender, KeyEventArgs e) // Handler for key press in input box
        {
            if (e.Key == Key.Enter && Keyboard.Modifiers != ModifierKeys.Shift) // Check Enter without Shift
            {
                e.Handled = true; // Prevent newline insertion
                SendMessage(); // Send message
            }
        }

        private void SendMessage() // Collect input, validate, send to engine and display response
        {
            try // Protect send flow from exceptions
            {
                string rawInput = UserInputBox.Text; // Read raw text from input box
                if (string.IsNullOrWhiteSpace(rawInput)) // Ignore empty or whitespace-only input
                {
                    UserInputBox.Focus(); // Return focus to input box
                    return; // Do not proceed
                }

                string userText = rawInput.Length > 500 ? rawInput.Substring(0, 500) + "…" : rawInput.Trim(); // Truncate and trim input
                UserInputBox.Clear(); // Clear input box after reading

                AddUserMessage(userText); // Render user's message in UI
                string response = _engine.ProcessInput(userText); // Get response from chatbot engine
                AddBotMessage(response); // Render bot response in UI
            }
            catch (Exception ex) // On error, show a friendly bot message instead of crashing
            {
                AddBotMessage($"⚠️ An error occurred: {ex.Message}"); // Display error text in chat
            }
        }

        private void QuickTopic_Click(object sender, RoutedEventArgs e) // Handler for quick topic buttons
        {
            if (sender is Button btn && btn.Tag is string topic) // Ensure sender is Button and Tag is a topic string
            {
                UserInputBox.Text = $"Tell me about {topic}"; // Pre-fill input with chosen topic
                SendMessage(); // Immediately send the pre-filled message
            }
        }

        private async void VoiceGreetingButton_Click(object sender, RoutedEventArgs e) // Play voice greeting when clicked
        {
            try // Protect audio playback from exceptions
            {
                string wavPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "greeting.wav"); // Path to greeting audio

                if (File.Exists(wavPath)) // If audio file exists, play it
                {
                    _greetingPlayer = new SoundPlayer(wavPath); // Create SoundPlayer for file
                    _greetingPlayer.Play(); // Start playback (non-blocking)
                    AddBotMessage("🔊 Voice greeting played!"); // Inform user via chat

                    // Visual feedback
                    VoiceGreetingButton.Background = new SolidColorBrush(Color.FromRgb(0, 200, 100)); // Temporarily change button color
                    await System.Threading.Tasks.Task.Delay(200); // Short delay to show feedback
                    VoiceGreetingButton.Background = (Brush)FindResource("AccentBlueBrush"); // Restore original brush
                }
                else // If file not found, play system sound and fallback to text message
                {
                    System.Media.SystemSounds.Asterisk.Play(); // Play system asterisk sound
                    AddBotMessage("🔊 Voice greeting file not found. Add 'greeting.wav' to the Resources folder."); // Tell user about missing file
                    AddBotMessage("Text greeting: Welcome to CyberGuard AI! 🛡️"); // Provide textual fallback greeting
                }
            }
            catch (Exception ex) // Report audio playback errors to chat
            {
                AddBotMessage($"⚠️ Could not play audio: {ex.Message}"); // Display error message
            }
        }

        private void ClearChatButton_Click(object sender, RoutedEventArgs e) // Handler to clear conversation
        {
            ChatPanel.Children.Clear(); // Remove all children messages from chat panel
            AddWelcomeMessage(); // Re-add initial welcome message after clearing
            AddBotMessage("Conversation cleared. Type 'help' to see available topics."); // Inform user that chat was cleared
        }

        // ── Delegate Callbacks ────────────────────────────────────

        private void UpdateSentimentIndicator(string sentiment, string emoji) // Update sentiment display in UI
        {
            Dispatcher.Invoke(() => // Ensure UI updates occur on the dispatcher thread
            {
                SentimentEmoji.Text = emoji; // Show sentiment emoji
                SentimentBlock.Text = $"Sentiment: {sentiment}"; // Update sentiment label

                // Update border color based on sentiment
                switch (sentiment.ToLower()) // Normalize sentiment to lower-case for comparisons
                {
                    case "worried": // Worried sentiment styling
                        SentimentBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(0, 100, 255));
                        SentimentBorder.Background = new SolidColorBrush(Color.FromRgb(50, 20, 20));
                        break; // End worried
                    case "curious": // Curious sentiment styling
                        SentimentBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(0, 255, 136));
                        SentimentBorder.Background = new SolidColorBrush(Color.FromRgb(20, 40, 60));
                        break; // End curious
                    case "frustrated": // Frustrated sentiment styling
                        SentimentBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(255, 100, 100));
                        SentimentBorder.Background = new SolidColorBrush(Color.FromRgb(50, 35, 10));
                        break; // End frustrated
                    default: // Default neutral styling when sentiment is unknown
                        SentimentBorder.BorderBrush = (Brush)FindResource("BorderBrush2");
                        SentimentBorder.Background = new SolidColorBrush(Color.FromRgb(22, 27, 34));
                        break; // End default
                } // End switch
            }); // End dispatcher invoke
        }

        private void UpdateMemoryPanel(UserMemory memory) // Refresh user memory information panel in UI
        {
            Dispatcher.Invoke(() => // Ensure memory UI updates on UI thread
            {
                if (memory.HasName) // If name is present, show it
                {
                    MemoryNameBlock.Text = $"👤 {memory.UserName}"; // Display stored user name
                    MemoryNameBlock.Foreground = (Brush)FindResource("AccentGreenBrush"); // Highlight color for set value
                }
                else // No name stored
                {
                    MemoryNameBlock.Text = "👤 Not set"; // Placeholder text
                    MemoryNameBlock.Foreground = (Brush)FindResource("TextMutedBrush"); // Muted text color
                }

                if (memory.HasTopic) // If favorite topic is stored, show it
                {
                    MemoryTopicBlock.Text = $"📚 Topic: {memory.FavouriteTopic}"; // Display favorite topic
                    MemoryTopicBlock.Foreground = (Brush)FindResource("AccentBlueBrush"); // Accent color
                }
                else // No topic stored
                {
                    MemoryTopicBlock.Text = "📚 No topic selected"; // Placeholder text
                    MemoryTopicBlock.Foreground = (Brush)FindResource("TextMutedBrush"); // Muted color
                }

                if (memory.HasLevel) // If experience level is available, show it
                {
                    MemoryLevelBlock.Text = $"📊 Level: {memory.ExperienceLevel}"; // Display experience level
                    MemoryLevelBlock.Foreground = (Brush)FindResource("AccentGreenBrush"); // Accent color for set value
                }
                else // No level stored
                {
                    MemoryLevelBlock.Text = "📊 Level: Not set"; // Placeholder text
                    MemoryLevelBlock.Foreground = (Brush)FindResource("TextMutedBrush"); // Muted color
                }
            }); // End dispatcher invoke
        } // End UpdateMemoryPanel
    } // End MainWindow class
} // End namespace
