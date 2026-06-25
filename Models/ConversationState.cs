// Summary of comments:
// - This file defines ConversationState which tracks lightweight per-session information used by the engine.
// - Each line is annotated with a short comment describing its role: fields, properties, constants and methods.
// - The `OnboardingPhase` enum represents the current step during initial setup of a user conversation.

using System.Collections.Generic; // Provides List<T> used to store recent user inputs

namespace CyberAwarenessBot // Application namespace grouping related models and services
{
    public class ConversationState // Holds transient state for a single conversation/session
    {
        public OnboardingPhase Phase { get; set; } = OnboardingPhase.AskName; // Current onboarding phase; defaults to asking the user's name
        public string LastTopic { get; set; } // Last topic discussed in the conversation (may be null)
        public bool AwaitingFollowUp { get; set; } // True when the bot expects a follow-up from the user
        public List<string> RecentUserInputs { get; } = new List<string>(); // FIFO list of recent user messages for context
        private const int MaxHistory = 10; // Maximum number of entries to retain in RecentUserInputs

        public void RecordInput(string input) // Record a new user input and trim history to MaxHistory
        {
            RecentUserInputs.Add(input); // Append newest input to the end of the list
            if (RecentUserInputs.Count > MaxHistory) // If history exceeds limit
                RecentUserInputs.RemoveAt(0); // Remove the oldest entry to maintain size
        }
    }

    public enum OnboardingPhase // Enum describing steps in initial onboarding flow
    {
        AskName, // Phase where the bot asks for the user's name
        AskTopic, // Phase where the bot asks for a preferred topic
        AskLevel, // Phase where the bot asks about the user's experience level
        Chatting // Normal chatting phase after onboarding is complete
    }
}
