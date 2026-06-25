// Summary of comments:
// - This file implements a simple in-memory key/value store for persisting lightweight user attributes.
// - Each line is annotated with a concise comment describing its purpose and behavior.
// - The class implements `IMemory` and exposes convenience properties and constants for known keys.

using System; // Core system types used for StringComparer
using System.Collections.Generic; // Provides Dictionary<TKey, TValue> used for storage

namespace CyberAwarenessBot // Application namespace grouping models and services
{
    public class UserMemory : IMemory // In-memory implementation of IMemory for storing simple user data
    {
        // Internal dictionary storing string keys and values with case-insensitive keys
        private readonly Dictionary<string, string> _store = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public const string KeyName = "name"; // Key used to store the user's name
        public const string KeyTopic = "favourite_topic"; // Key used to store the user's favourite topic
        public const string KeyLevel = "experience_level"; // Key used to store the user's experience level

        public void Set(string key, string value) => _store[key] = value; // Store or overwrite a value for a key

        public string Get(string key) // Retrieve a stored value by key, or null if missing
        {
            string v; // Temporary variable to hold attempted retrieved value
            return _store.TryGetValue(key, out v) ? v : null; // Return value if found; otherwise null
        }

        public bool Has(string key) => _store.ContainsKey(key); // Check presence of a key in the store

        // Convenience properties that expose commonly used memory entries
        public string UserName => Get(KeyName); // Get stored user name or null
        public string FavouriteTopic => Get(KeyTopic); // Get stored favourite topic or null
        public string ExperienceLevel => Get(KeyLevel); // Get stored experience level or null

        // Convenience boolean properties to quickly check presence of common entries
        public bool HasName => Has(KeyName); // True when user name is stored
        public bool HasTopic => Has(KeyTopic); // True when favourite topic is stored
        public bool HasLevel => Has(KeyLevel); // True when experience level is stored
    } // End UserMemory class
} // End namespace
