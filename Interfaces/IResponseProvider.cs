// Summary of comments:
// - This file declares the `IResponseProvider` interface used by the chatbot engine to obtain responses.
// - Each line is annotated with a short comment describing its purpose and the contract it defines.

namespace CyberAwarenessBot // Root namespace for application types
{
    public interface IResponseProvider // Interface contract for components that provide bot responses
    {
        // Given the user input and the current conversation state, return an appropriate response string
        string GetResponse(string input, ConversationState state);
    } // End IResponseProvider interface
} // End namespace
