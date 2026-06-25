 # CyberAwarenessBot

CyberAwarenessBot is a simple WPF desktop chatbot prototype that provides cybersecurity tips and guidance. It demonstrates a small, modular architecture with a UI, a lightweight rule-based sentiment analyzer, a keyword-driven response provider, and an in-memory user memory store.

---

## Table of Contents

- [Project overview](#project-overview)
- [Prerequisites](#prerequisites)
- [Setup and run](#setup-and-run)
- [Project structure (file-by-file)](#project-structure-file-by-file)
- [How the main components work](#how-the-main-components-work)
- [Adding content (resources, tips)](#adding-content-resources-tips)
- [Troubleshooting](#troubleshooting)
- [Contributing](#contributing)
- [License](#license)

---

## Project overview

`CyberAwarenessBot` is a Windows Presentation Foundation (WPF) application that accepts user input in a chat UI and replies with cybersecurity tips. It includes:

- A UI layer (`MainWindow`) that renders user/bot message bubbles, a sentiment indicator and a small user memory panel.
- A small engine layer (`ChatbotEngine`) that manages conversation state, onboarding (name/topic/level), sentiment integration and delegates to the response provider.
- A `KeywordResponseProvider` that stores topic tips and returns a random tip for a matched keyword.
- A rule-based `SentimentAnalyzer` used to form tone prefixes and to update a visible sentiment indicator.
- An in-memory `UserMemory` used to persist small user attributes during a session.

---

## Prerequisites

- Microsoft Visual Studio 2022/2025/2026 (Community or higher) with .NET desktop development workload installed.
- .NET Framework 4.7.2 (project targets `v4.7.2`).
- Optional: `Resources/greeting.wav` if you want voice greeting playback.

> Note: The project folder is currently inside OneDrive in this workspace. OneDrive (or other sync/antivirus software) may lock files in `bin/` or `obj/` during builds. Consider excluding the project folder or `bin`/`obj` from sync if you see file-lock errors.

---

## Setup and run

1. Open `CyberAwarenessBot.sln` (or the folder containing `CyberAwarenessBot.csproj`) in Visual Studio.
2. Restore any missing workloads or SDKs if Visual Studio prompts.
3. Build the solution (`Build` → `Build Solution`).
4. If Visual Studio reports that the EXE is locked (MSB3026/MSB3027):
   - Stop debugging if the app is currently running (Debug → Stop Debugging).
   - Kill the process using Task Manager or PowerShell (example):
     - `Get-Process -Id 7320` (inspect)
     - `Stop-Process -Id 7320 -Force`
     - Or by name: `Stop-Process -Name CyberAwarenessBot -Force`
   - Clean (`Build` → `Clean Solution`) and rebuild.
5. Run the app (`Debug` → `Start Debugging` or `Start Without Debugging`).

When running, the application will ask for your name as part of onboarding, then ask for your favourite cybersecurity topic and experience level.

---

## Project structure (file-by-file)

Top-level project files

- `CyberAwarenessBot.csproj` - MSBuild project file with references and build settings.
- `App.xaml`, `App.xaml.cs` - WPF application definition and global startup logic. `App.xaml.cs` registers global exception handlers.
- `MainWindow.xaml`, `MainWindow.xaml.cs` - Main UI and code-behind. Handles rendering messages, user input, buttons and visual feedback (ASCII art, sentiment, memory panel).

Source files (core logic)

- `Services/ChatbotEngine.cs` - Central engine. Handles onboarding flow, records conversation state (`ConversationState`), invokes `SentimentAnalyzer`, and asks `KeywordResponseProvider` for replies. Builds help/farewell messages.
- `Services/KeywordResponseProvider.cs` - A dictionary-based knowledge base and alias map. Exposes `GetResponse` and `GetFollowUpFor`.
- `Services/SentimentAnalyzer.cs` - Small rule-based sentiment detector. Produces `SentimentResult` (label, emoji, tone prefix) and raises sentiment changed events.

Models

- `Models/ConversationState.cs` - Tracks onboarding phase, last topic, whether a follow-up is expected, and recent user inputs (FIFO up to `MaxHistory`).
- `Models/SentimentResult.cs` - DTO with `Label`, `Emoji`, and `TonePrefix` used to format replies and update UI.
- `Models/UserMemory.cs` - In-memory key/value store implementing `IMemory` and convenience properties (`UserName`, `FavouriteTopic`, `ExperienceLevel`).

Interfaces

- `Interfaces/IResponseProvider.cs` - Defines `string GetResponse(string input, ConversationState state);` for pluggable response providers.
- `Interfaces/IMemory.cs` - Interface implemented by `UserMemory` (simple key/value API).

Properties (auto-generated)

- `Properties/Resources.Designer.cs`, `Properties/Settings.Designer.cs` - Auto-generated resource and settings wrappers. Update `.resx` / settings then rebuild.

Resources

- `Resources/` - Folder for static assets. Add `greeting.wav` here to enable the voice greeting feature used by the UI.

---

## How the main components work (high level)

- `MainWindow.xaml.cs` handles UI events (send button, Enter key, quick-topic buttons, voice greeting button) and uses `ChatbotEngine` to process user input. It renders user and bot messages as styled bubbles and updates sentiment and memory UI panels via callbacks the engine provides.

- `ChatbotEngine` maintains a `ConversationState` which records onboarding phase and recent inputs. On first runs it walks the user through an onboarding flow: ask name, ask topic, ask level. After onboarding it processes messages:
  - Check for exit phrases (returns farewell)
  - Run `SentimentAnalyzer.Analyse` to get `SentimentResult` and possibly update UI via event/callback
  - If the user requests a follow-up ("tell me more"), attempt to return a follow-up tip for the last topic
  - Otherwise ask `KeywordResponseProvider.GetResponse` for a topic tip
  - If nothing matched, return a fallback message or the help message

- `KeywordResponseProvider` uses a canonical `Dictionary<string,string[]>` mapping to topic tips and a small alias dictionary to normalize user terms (e.g., "two factor" → `2fa`). It randomly selects one of the tips to vary responses.

- `SentimentAnalyzer` is rule-based and keyword-driven. It maps indicative words/phrases to sentiment labels (`Worried`, `Curious`, `Frustrated`). When it detects a change in sentiment it raises `OnSentimentChanged` to let the UI update an emoji + color scheme. It also returns a tone prefix that the engine prepends to replies.

---

## Adding or modifying tips and resources

- To add topics or tips, edit `Services/KeywordResponseProvider.cs` and add new entries to `_knowledgeBase` or update `_aliases`.
- To modify sentiment rules or tone prefixes, edit `Services/SentimentAnalyzer.cs`.
- To add voice greeting audio, place `greeting.wav` into the `Resources` folder in the project root.

---

## Troubleshooting

- Build fails with "file is being used by another process" (MSB3026 / MSB3027): stop the running app, kill the `CyberAwarenessBot` process (Task Manager or PowerShell), clean and rebuild.
- If UI resources (brushes, controls referenced by `FindResource`) are missing, ensure `MainWindow.xaml` contains the resource keys: `UserBubbleBrush`, `BotBubbleBrush`, `AccentGreenBrush`, `AccentBlueBrush`, `TextPrimaryBrush`, `TextMutedBrush`, `BorderBrush2`.
- If audio playback fails, verify `Resources/greeting.wav` exists and is a valid WAV file.

---

## Screenshots

Paste three screenshots showing the app running. Use the Markdown image format and relative paths (or full URLs) to include them here:

- Screenshot 1 - onboarding / welcome screen

  ![Screenshot 1](path/to/screenshot1.png)

- Screenshot 2 - conversation with a topic/tip shown

  ![Screenshot 2](path/to/screenshot2.png)

- Screenshot 3 - sentiment indicator / memory panel

  ![Screenshot 3](path/to/screenshot3.png)

Replace `path/to/screenshotX.png` with your image filenames (add them to the repo or use external URLs).

---

## Demo video

Paste your unlisted YouTube demo link here so markers can view the presentation. Example:

YouTube (unlisted): https://youtu.be/your_video_id_here

Replace the URL above with your actual video link.

---

## Contributing

- Fork, make changes, test locally and open a pull request. Keep changes focused and add unit tests if you add logic changes.

---

## License

Include your project's license here (MIT, Apache-2.0, etc.).


If you want, I can also:
- Add inline badges (build status, license)
- Generate a sample `greeting.wav` placeholder
- Create a short CONTRIBUTING.md or issue templates