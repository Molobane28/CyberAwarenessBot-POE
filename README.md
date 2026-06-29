CyberGuard AI - Cybersecurity Awareness Chatbot
Table of Contents
Project Overview

System Architecture

Technology Stack

Project Structure

Detailed Component Breakdown

Part 1: Core Chatbot & Sentiment Analysis

Part 2: Keyword Responses & User Memory

Part 3: Task Management, Quiz & Activity Log

Database Schema

How to Run the Application

Features Walkthrough

Troubleshooting
Project Overview
CyberGuard AI is a comprehensive WPF (Windows Presentation Foundation) desktop application designed to educate users about cybersecurity best practices through interactive conversation. The application combines a chatbot with sentiment analysis, task management, a cybersecurity quiz, and activity logging—all backed by a MySQL database.

The project is structured in three progressive parts, each building upon the previous to create a fully integrated solution:

Part 1: Core chatbot engine with sentiment analysis and dynamic responses

Part 2: Keyword recognition, user memory, and contextual responses

Part 3: Task assistant with reminders, cybersecurity quiz, NLP simulation, and activity logging

Project Overview
CyberGuard AI is a comprehensive WPF (Windows Presentation Foundation) desktop application designed to educate users about cybersecurity best practices through interactive conversation. The application combines a chatbot with sentiment analysis, task management, a cybersecurity quiz, and activity logging—all backed by a MySQL database.

The project is structured in three progressive parts, each building upon the previous to create a fully integrated solution:

Part 1: Core chatbot engine with sentiment analysis and dynamic responses

Part 2: Keyword recognition, user memory, and contextual responses

Part 3: Task assistant with reminders, cybersecurity quiz, NLP simulation, and activity logging

Project Overview
CyberGuard AI is a comprehensive WPF (Windows Presentation Foundation) desktop application designed to educate users about cybersecurity best practices through interactive conversation. The application combines a chatbot with sentiment analysis, task management, a cybersecurity quiz, and activity logging—all backed by a MySQL database.

The project is structured in three progressive parts, each building upon the previous to create a fully integrated solution:

Part 1: Core chatbot engine with sentiment analysis and dynamic responses

Part 2: Keyword recognition, user memory, and contextual responses

Part 3: Task assistant with reminders, cybersecurity quiz, NLP simulation, and activity logging

Technology Stack
Component	Technology
Frontend	WPF (Windows Presentation Foundation) with XAML
Language	C# (.NET Framework 4.7.2 / .NET Core)
Database	MySQL 8.0+
Database Connector	MySql.Data (NuGet package)
Audio	System.Media (SoundPlayer)
Pattern Matching	Regex (System.Text.RegularExpressions)
UI Styles	Custom XAML with dark theme (CyberGuard AI branding)

Project Structure
CyberAwarenessBot/
│
├── App.xaml                          # Application resources and styles
├── App.xaml.cs                       # Global exception handling
├── MainWindow.xaml                   # Main UI with chat, tabs, and controls
├── MainWindow.xaml.cs                # All UI logic and event handlers
├── App.config                        # Assembly binding redirects
│
├── Models/                           # Data models
│   ├── CyberTask.cs                  # Task entity with reminder properties
│   ├── ActivityLogEntry.cs           # Log entry with timestamp
│   ├── NlpIntentResult.cs            # NLP processing result
│   ├── QuizQuestion.cs               # Quiz question with options
│   ├── QuizSession.cs                # Active quiz session state
│   ├── UserMemory.cs                 # User profile storage
│   ├── ConversationState.cs          # Chat session state
│   └── SentimentResult.cs            # Sentiment analysis result
│
├── Interfaces/                       # Service contracts
│   ├── ITaskRepository.cs            # CRUD operations for tasks
│   └── IActivityLogger.cs            # Logging operations
│
├── Services/                         # Business logic services
│   ├── ChatbotEngine.cs              # Main conversation engine
│   ├── KeywordResponseProvider.cs    # Topic-specific responses
│   ├── SentimentAnalyzer.cs          # Sentiment detection
│   ├── NlpProcessor.cs               # Intent detection and extraction
│   ├── TaskAssistantService.cs       # Task management logic
│   ├── MySqlTaskRepository.cs        # MySQL database operations
│   ├── ActivityLogger.cs             # In-memory activity logging
│   └── QuizService.cs                # Quiz management and scoring
│
├── Resources/                        # Embedded resources
│   └── GreetingAudio.wav             # Startup greeting sound
│
└── Scripts/                          # Database scripts
    └── cyberawarenessbot.sql         # Database schema and sample data
 Detailed Component Breakdown
Part 1: Core Chatbot & Sentiment Analysis
1.1 SentimentAnalyzer.cs
Purpose: Detects user sentiment based on keyword matching.

How it works:

Maintains a dictionary of keywords mapped to sentiment categories (Worried, Curious, Frustrated, Neutral)

Uses case-insensitive string.Contains() to scan user input

When sentiment changes, raises an event that updates the UI indicator

Returns a SentimentResult with label, emoji, and tone prefix   

1.2 ChatbotEngine.cs
Purpose: Manages conversation flow, onboarding, and response generation.

How it works:

Onboarding Flow: Asks for name → favourite topic → experience level

Response Generation:

Checks for exit phrases (bye, goodbye, etc.)

Analyzes sentiment

Checks for follow-up requests ("tell me more", "another tip")

Looks up keyword-based responses

Falls back to general cybersecurity tips

Personalization: Uses user's name, favourite topic, and experience level to tailor responses
Part 2: Keyword Responses & User Memory
2.1 KeywordResponseProvider.cs
Purpose: Provides topic-specific cybersecurity responses.

How it works:

Maintains a dictionary of topics → array of responses

Each topic has multiple response variations for variety

Provides follow-up responses when user asks "tell me more"

Supported Topics:

Topic	Sample Response
password	"🔑 Strong passwords are essential. Use at least 12 characters..."
phishing	"🎣 Phishing attacks try to steal your information..."
2fa	"🔐 Two-Factor Authentication adds a second verification step..."
malware	"🦠 Malware includes viruses, ransomware, and spyware..."
privacy	"🛡️ Review your privacy settings on social media regularly..."
scam	"⚠️ Scammers often impersonate banks, government agencies..."
ransomware	"💰 Ransomware encrypts your files and demands payment..."
2.2 UserMemory.cs & ConversationState.cs
Purpose: Store user preferences and conversation context.

UserMemory Properties:

UserName - User's name

FavouriteTopic - Preferred cybersecurity topic

ExperienceLevel - beginner / intermediate / expert

ConversationState Properties:

Phase - AskName, AskTopic, AskLevel, Chatting

LastTopic - Last discussed topic (for follow-ups)

Part 3: Task Management, Quiz & Activity Log
3.1 TaskAssistantService.cs & MySqlTaskRepository.cs
Purpose: Full CRUD operations for cybersecurity tasks with reminders.

How it works:

Add Task: Creates task in database with title, description, optional reminder

Set Reminder: Updates reminderat column with DateTime

Complete Task: Marks iscompleted as true

Delete Task: Removes from database

List Tasks: Retrieves all tasks ordered by creation date
3.2 QuizService.cs
Purpose: Manages cybersecurity quiz with 12+ questions.

Question Types:

Multiple Choice (e.g., "Which is the strongest password?")

True/False (e.g., "True or False: It is safe to click a link...")

Features:

Tracks score through quiz session

Provides immediate feedback with explanations

Builds motivational final message based on score

Scoring Feedback:

Score Range	Message
100%	"Outstanding - you scored 12/12!"
75-99%	"Well done - you have strong cybersecurity knowledge."
50-74%	"Nice effort - you're building good cybersecurity awareness."
<50%	"That's a good start - keep practicing!"
3.3 NlpProcessor.cs
Purpose: Simulates natural language processing using keyword detection.

Intents Detected:

Intent	Example Variations	Extracted Data
addtask	"add task X", "create task X", "new task X", "I want to add X", "make task X"	Title
remindme	"remind me to X", "remind me about X", "set reminder for X"	Title, Time
showtasks	"show tasks", "view tasks", "list tasks", "my tasks"	-
startquiz	"start quiz", "play quiz", "take quiz", "test me"	-
showactivitylog	"show activity log", "what have you done for me", "history"	-
3.4 ActivityLogger.cs
Purpose: In-memory logging of all user actions with timestamps.

Logged Actions:

System startup
Task added / completed / deleted
Reminder set / skipped
Quiz started / answered / completed
NLP interpretation
Audio playback status
Database errors

How to Run the Application
Prerequisites
Visual Studio (2019 or 2022) with .NET desktop development workload

MySQL Server (8.0 or higher) running locally

MySQL Workbench (optional, for running scripts)

Features Walkthrough
1. Chat Interface
Purpose: Main interaction area where users talk to CyberGuard AI.

How to use:

Type a message in the text box at the bottom

Press Enter or click "Send"

The bot responds with cybersecurity information

Example Interactions:

text
User: "Hello"
Bot: "👋 Hello! I'm CyberGuard AI - your personal cybersecurity awareness assistant..."

User: "I'm worried about phishing"
Bot: [Analyzes sentiment → Worried]
    "It's completely understandable to feel that way...
    🎣 Phishing attacks try to steal your information..."
2. Sentiment Indicator
Location: Top of chat panel

Purpose: Shows detected sentiment in real-time.

Sentiment	Emoji	When Triggered
Worried	😟	"worried", "concerned", "scared", "hacked"
Curious	🤔	"curious", "learn", "want to know"
Frustrated	😤	"frustrated", "confusing", "overwhelming"
Neutral	😐	Default
3. Memory Tab
Location: Right panel, first tab

Purpose: Displays user profile information collected during onboarding.

Information displayed:

Name

Favourite Topic

Experience Level

4. Tasks Tab
Location: Right panel, second tab

Purpose: Full task management interface.

Features:

Add Task: Enter Title, Description (optional), Reminder Date (optional)

Refresh: Update task list

Mark Completed: Select a task and click to mark as done

Delete Task: Select a task and remove from database

Chat Integration:

text
User: "add task enable two-factor authentication"
Bot: "✅ Task added: #12 - enable two-factor authentication
     ⏰ Would you like to set a reminder?"
     
User: "yes, in 7 days"
Bot: "✅ Got it! I'll remind you about 'enable two-factor authentication' on Monday, July 6, 2026 at 9:00 AM."
5. Quiz Tab
Location: Right panel, third tab

Purpose: Cybersecurity knowledge assessment.

How to play:

Click "Start Quiz" (12 questions loaded)

Read the question

Select one of four options

Click "Submit Answer" for immediate feedback

Click "Next Question" to continue

View final score with motivational message

Activity Log Integration:

"Quiz Started" entry when you begin

"Quiz Answered" entry for each question (with correctness)

"Quiz Completed" entry with final score

6. Activity Log Tab
Location: Right panel, fourth tab

Purpose: Shows chronological history of all actions.

Features:

Displays last 5 entries by default

"Show More" button increases display count by 5

Entries include timestamps and descriptions

Chat Integration:

text
User: "show activity log"
Bot: "📋 Here are the most recent actions:
     - [2026-06-29 14:30:25] Task Added: Task #12 'Enable Two-Factor Authentication'
     - [2026-06-29 14:31:10] Reminder Set: Reminder for task #12 at 2026-07-06 14:30
     - [2026-06-29 14:35:45] Quiz Started: Cybersecurity quiz started with 12 questions"
Command Reference
Chat Commands
Command	Variations	Description
add task [title]	"create task", "new task", "make task"	Adds a task and prompts for reminder
show tasks	"view tasks", "list tasks", "my tasks"	Displays all tasks
start quiz	"play quiz", "take quiz", "test me"	Starts cybersecurity quiz
show activity log	"what have you done for me", "history", "show log"	Shows recent actions
remind me to [task]	"set reminder for", "remind me about"	Direct reminder creation
help	"what can you do", "commands"	Shows available topics and commands
Task Management Commands
Complete a task: Select in Tasks tab → Click "Mark Completed"

Delete a task: Select in Tasks tab → Click "Delete Task"

Refresh tasks: Click "Refresh"

Quiz Commands
Start Quiz: Click button

Submit Answer: Click button after selecting option

Next Question: Click button after submitting answer

Troubleshooting
Common Issues and Solutions
1. Database Connection Error
text
Error: "Unable to connect to any of the specified MySQL hosts"
Solution:

Ensure MySQL Server is running

Check connection string in InitializeServices()

Verify MySQL port (default: 3306)

Test connection in MySQL Workbench

2. Missing MySql.Data Assembly
text
Error: "The type or namespace name 'MySql' could not be found"
Solution:

Open NuGet Package Manager

Install MySql.Data package

Rebuild the solution

3. Audio Not Playing
text
No error, but audio doesn't play on startup
Solution:

Ensure GreetingAudio.wav is in Resources folder

Set Build Action to "Embedded Resource"

Check if file is corrupted (try playing manually)

The app will continue without audio (graceful fallback)

4. Reminder Not Setting
text
Task added but no reminder prompt appears
Solution:

Check NlpProcessor detects "add task" phrase

Ensure awaitingReminderResponse is set to true

Verify pendingTaskWithReminder stores the task

Check Activity Log for "Reminder Prompt" entry

5. Quiz Questions Not Loading
text
Error: "Failed to load quiz questions"
Solution:

Check QuizService.CreateDefaultQuiz() has questions

Ensure 12+ questions are defined

Verify each question has options and correct answer

6. Activity Log Not Showing
text
Activity Log tab is empty
Solution:

Perform some actions first (add task, start quiz)

Click "Refresh Log" button

Check ActivityLogger.Log() is being called

Verify ActivityLogListBox.ItemsSource is updated

7. NLP Not Detecting Intents
text
Bot responds with "I'm not sure I understand"
Solution:

Check Output window for [NLP] debug messages

Use exactly one of the supported command variations

Ensure Normalize() method handles input correctly

Add more variations in ContainsAny() patterns

8. Build Errors (Duplicate Classes)
text
Error: "The namespace already contains a definition for 'TaskAssistantService'"
Solution:

Check for duplicate files in Solution Explorer

Ensure only one TaskAssistantService.cs exists

Delete any duplicate files from the project

Performance Considerations
Database Operations
All queries use parameterized commands to prevent SQL injection

Connections are opened and closed for each operation

Try-catch blocks prevent crashes on database errors

Memory Management
Activity Log limits to 100 entries (prevents memory issues)

Tasks are loaded from database on-demand (no caching issues)

SoundPlayer resources are disposed automatically

UI Responsiveness
All database operations run on UI thread (small data sets)

Chat panel auto-scrolls to latest message

Status bar provides visual feedback during operations

Future Enhancements
Potential Improvements
Cloud Sync: Store tasks in cloud database for multi-device access

Push Notifications: Reminders via email or mobile notifications

Speech Recognition: Voice input for hands-free interaction

More Quiz Questions: Add 50+ questions with difficulty levels

Dark/Light Theme: Toggle between themes

Export Data: Export tasks and activity log to CSV/PDF

User Profiles: Multiple users with separate task lists

Achievement System: Badges for completing tasks and quizzes

Credits & Resources
Technologies Used
WPF: Microsoft UI framework

MySQL: Database management system

MySql.Data: Official MySQL connector for .NET

Regex: Natural language pattern matching

Documentation References
Microsoft WPF Documentation

MySQL Connector/NET Documentation
