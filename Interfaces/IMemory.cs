namespace CyberAwarenessBot
{
    /// <summary>
    /// Contract for user memory storage.
    /// Decouples storage mechanism from engine logic.
    /// </summary>
    public interface IMemory
    {
        /// <summary>
        /// Stores a key-value pair in memory.
        /// </summary>
        /// <param name="key">The identifier for the data (e.g., "name", "topic")</param>
        /// <param name="value">The value to store</param>
        void Set(string key, string value);

        /// <summary>
        /// Retrieves a value from memory by its key.
        /// </summary>
        /// <param name="key">The identifier for the data</param>
        /// <returns>The stored value, or null if not found</returns>
        string Get(string key);

        /// <summary>
        /// Checks whether a key exists in memory.
        /// </summary>
        /// <param name="key">The identifier to check</param>
        /// <returns>True if the key exists, false otherwise</returns>
        bool Has(string key);
    }
}