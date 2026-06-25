// Summary of comments:
// - This provider maps topic keywords (and aliases) to helpful tips and selects a random tip per query.
// - Comments on each line explain fields, data structures and helper methods for clarity and maintainability.

using System; // Core system types (Random)
using System.Collections.Generic; // Collection types like Dictionary and IEnumerable
using System.Linq; // LINQ helpers (not heavily used but available)

namespace CyberAwarenessBot // Application namespace
{
    public class KeywordResponseProvider : IResponseProvider // Provides keyword-based responses for the chatbot
    {
        // Use a Random seeded from a GUID hashcode for improved randomness across runs
        private readonly Random _rng = new Random(Guid.NewGuid().GetHashCode()); // RNG for selecting tips
        // Track last returned tip index per topic to avoid returning the same tip twice in a row
        private readonly Dictionary<string, int> _lastTipIndex = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        private int _lastGreetingIndex = -1; // Remember last greeting index
        private int _lastFunFactIndex = -1; // Remember last fun fact index

        // Knowledge base mapping canonical topic keys to arrays of tip strings
        private static readonly Dictionary<string, string[]> _knowledgeBase = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["password"] = new[]
            {
                "🔒 Use a passphrase of 4+ random words (e.g. 'BlueMango$Rain7'). It's easier to remember and much harder to crack.",
                "🔒 Never reuse passwords across sites. Use a password manager like Bitwarden or 1Password.",
                "🔒 Check haveibeenpwned.com to see if your email has appeared in a known data breach.",
                "🔒 A strong password has at least 12 characters with uppercase, lowercase, numbers, and symbols.",
                "🔒 Turn on auto-fill from your password manager instead of typing passwords in public places.",
                "🔒 Use unique, site-specific passwords to limit exposure if one account is breached.",
                "🔒 Consider passphrases that include unrelated words and a punctuation mark for strength.",
                "🔒 Regularly review saved passwords in your manager and rotate credentials for critical accounts."
            },
            ["phishing"] = new[]
            {
                "🎣 Always verify the sender's full email address — not just the display name.",
                "🎣 Hover over links before clicking. If the URL doesn't match the legitimate domain — don't click.",
                "🎣 Phishing emails create urgency: 'Act NOW or your account is suspended!' Slow down.",
                "🎣 When in doubt, navigate directly to the website yourself by typing the URL.",
                "🎣 Beware of attachments from unknown senders — scan them before opening.",
                "🎣 Use a separate browser profile for email and web browsing to limit cross-site risk.",
                "🎣 Report suspected phishing to your email provider so they can improve detection.",
                "🎣 Enable link protection in your email client or organization for extra safety." 
            },
            ["privacy"] = new[]
            {
                "🛡️ Review app permissions regularly. Revoke anything unnecessary.",
                "🛡️ Use a VPN on public Wi-Fi to encrypt your traffic.",
                "🛡️ Limit what you share on social media. Birthdates and employers are used in identity theft.",
                "🛡️ Use a privacy-focused browser setup: DuckDuckGo, block third-party cookies.",
                "🛡️ Enable privacy settings in social apps to restrict data sharing.",
                "🛡️ Periodically delete old accounts you no longer use to reduce your footprint.",
                "🛡️ Consider using separate email aliases for different services to limit tracking.",
                "🛡️ Review browser extensions; remove ones that request excessive permissions." 
            },
            ["malware"] = new[]
            {
                "🦠 Keep your OS and all software updated. Most malware exploits known vulnerabilities.",
                "🦠 Only download software from official sources or trusted stores.",
                "🦠 Run reputable antivirus software and schedule weekly full scans.",
                "🦠 Never plug in unknown USB drives — 'USB drop' attacks are real.",
                "🦠 Use application whitelisting in high-risk environments to prevent unknown programs.",
                "🦠 Keep backups offline to recover if ransomware encrypts your files.",
                "🦠 Be cautious with browser pop-ups asking you to install codecs or tools.",
                "🦠 Use least-privilege accounts; don't operate as an administrator for daily tasks." 
            },
            ["2fa"] = new[]
            {
                "🔑 Enable Two-Factor Authentication on every account that supports it.",
                "🔑 Prefer app-based 2FA (Google Authenticator, Authy) over SMS.",
                "🔑 Save your backup codes securely when enabling 2FA.",
                "🔑 Hardware security keys (YubiKey) provide the strongest 2FA available.",
                "🔑 Do not store 2FA backup codes in plain text or in your email.",
                "🔑 Consider using a dedicated 2FA app on a separate device for critical accounts.",
                "🔑 Periodically verify that your 2FA methods still work after phone changes.",
                "🔑 Register multiple 2FA methods when supported to avoid lockout." 
            },
            ["data breach"] = new[]
            {
                "💥 If notified of a breach: change your password immediately, enable 2FA.",
                "💥 Consider placing a credit freeze at the major bureaus if financial data was exposed.",
                "💥 Check haveibeenpwned.com regularly to monitor your email address.",
                "💥 Breached credential databases are sold on the dark web within hours.",
                "💥 Review which other services used the same credentials and rotate them.",
                "💥 Notify affected contacts if their data may have been exposed through you.",
                "💥 Monitor financial statements closely for suspicious activity after a breach.",
                "💥 Use a password manager to simplify rotating many compromised passwords." 
            },
            ["scam"] = new[]
            {
                "⚠️ If it sounds too good to be true — it is. Prize scams, investment schemes all rely on excitement.",
                "⚠️ Never pay someone who contacts you unsolicited via gift cards or cryptocurrency.",
                "⚠️ Tech-support scams: Microsoft will NEVER call you unprompted about a virus.",
                "⚠️ If a 'friend' messages asking for money, call them directly to confirm.",
                "⚠️ Be wary of impersonation on social media — verify profile details before trusting messages.",
                "⚠️ Scammers often pressure you to act quickly; pause and verify independently.",
                "⚠️ Never share remote access with unknown callers asking to 'fix' your PC.",
                "⚠️ Use trusted marketplaces and check seller reviews before making transactions." 
            },
            ["ransomware"] = new[]
            {
                "💰 Follow the 3-2-1 backup rule: 3 copies, 2 different media types, 1 offsite.",
                "💰 Never pay the ransom — there's no guarantee you'll recover your data.",
                "💰 Disconnect an infected machine from the network immediately.",
                "💰 Keep critical systems patched and isolate backups from the primary network.",
                "💰 Test your backups regularly to ensure recoverability before a crisis.",
                "💰 Use network segmentation to limit the blast radius of ransomware.",
                "💰 Educate employees about suspicious attachments and links that can deliver ransomware." 
            },
            ["vpn"] = new[]
            {
                "🌐 A VPN encrypts your internet traffic — essential on public Wi-Fi.",
                "🌐 Choose a reputable no-logs VPN (Mullvad, ProtonVPN). Free VPNs often sell your data.",
                "🌐 Remember: a VPN doesn't make you anonymous. Websites can still track you via cookies.",
                "🌐 Avoid free VPNs that inject ads or track your traffic; prefer audited providers.",
                "🌐 Use a VPN on devices you control; avoid installing untrusted VPN clients on shared devices.",
                "🌐 Check VPN jurisdiction and logging policies before trusting them with sensitive info.",
                "🌐 For business use, prefer a company-managed VPN with split-tunneling configured appropriately." 
            },
            ["encryption"] = new[]
            {
                "🔐 Always look for HTTPS (padlock icon) before entering sensitive information.",
                "🔐 Enable full-disk encryption on your laptop — BitLocker on Windows, FileVault on macOS.",
                "🔐 For sensitive messages, use end-to-end encrypted apps like Signal.",
                "🔐 Use strong, unique passphrases when protecting encrypted containers.",
                "🔐 Consider using PGP or S/MIME for email when exchanging highly sensitive data.",
                "🔐 Verify public keys/fingerprints when adding new contacts to avoid man-in-the-middle attacks.",
                "🔐 Regularly backup your encryption keys in a secure offline location."
            }
        };

        // Aliases map common alternate phrases to canonical knowledge base keys
        private static readonly Dictionary<string, string> _aliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["2 factor"] = "2fa",
            ["two factor"] = "2fa",
            ["mfa"] = "2fa",
            ["databreach"] = "data breach",
            ["breach"] = "data breach"
        };

        // Random greetings the bot can use when greeting the user
        private static readonly string[] _greetings = new[]
        {
            "Hi there! 👋 Ready to learn some security tips?",
            "Hello! 🛡️ I'm here to help with cybersecurity questions.",
            "Hey! 😄 Ask me anything about staying safe online.",
            "Greetings! 🔐 Want a tip to improve your security?",
            "Welcome back! 💡 Curious about a specific topic?",
            "Good to see you! 🚀 Let's talk security."
        };

        // Random fun facts to sprinkle into conversation
        private static readonly string[] _funFacts = new[]
        {
            "Fun fact: The first computer virus was created in 1971 and was called the Creeper.",
            "Fun fact: 'Phishing' as a term emerged in the mid-1990s with AOL account thefts.",
            "Fun fact: Using a passphrase of four words is often stronger and easier to remember than a complex password.",
            "Fun fact: Two-factor authentication can block over 99% of automated attacks against accounts.",
            "Fun fact: The 3-2-1 backup rule helps protect you from ransomware and data loss.",
            "Fun fact: Many breaches are caused by compromised third-party vendors, not direct hacking of the target." 
        };

        // Try to find a response for the input; update conversation state when a topic is matched
        public string GetResponse(string input, ConversationState state)
        {
            foreach (var kv in _aliases) // Replace any alias occurrences with canonical values
            {
                var alias = kv.Key; // Alias text to search for
                var canonical = kv.Value; // Canonical topic key to replace with
                if (input.IndexOf(alias, StringComparison.OrdinalIgnoreCase) >= 0)
                    input = ReplaceIgnoreCase(input, alias, canonical); // Replace alias with canonical term
            }
            foreach (var kv in _knowledgeBase) // Search the knowledge base for a matching topic keyword
            {
                var keyword = kv.Key; // Topic keyword
                var tips = kv.Value; // Associated tips array
                if (input.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    state.LastTopic = keyword; // Remember last matched topic for potential follow-ups
                    state.AwaitingFollowUp = true; // Mark that follow-ups are allowed
                    // Choose a random tip avoiding immediate repeats if possible
                    int idx = _rng.Next(tips.Length);
                    int attempts = 0;
                    int lastIdx;
                    if (_lastTipIndex.TryGetValue(keyword, out lastIdx) && tips.Length > 1)
                    {
                        while (idx == lastIdx && attempts < 6)
                        {
                            idx = _rng.Next(tips.Length);
                            attempts++;
                        }
                    }
                    _lastTipIndex[keyword] = idx; // Record chosen index for next time
                    return tips[idx]; // Return selected tip
                }
            }
            return null; // No topic matched
        }

        // Return a random follow-up tip for the specified topic, or null if topic unknown
        public string GetFollowUpFor(string topic)
        {
            string[] tips; // Temporary variable to receive tips array
            if (!_knowledgeBase.TryGetValue(topic, out tips)) return null;
            int idx = _rng.Next(tips.Length);
            int attempts = 0;
            int lastIdx;
            if (_lastTipIndex.TryGetValue(topic, out lastIdx) && tips.Length > 1)
            {
                while (idx == lastIdx && attempts < 6)
                {
                    idx = _rng.Next(tips.Length);
                    attempts++;
                }
            }
            _lastTipIndex[topic] = idx;
            return tips[idx]; // Pick random tip if available, avoid immediate repeat
        }

        // Expose the known topic keys to callers (read-only)
        public IEnumerable<string> KnownTopics { get { return _knowledgeBase.Keys; } }

        // Return a random greeting string, avoiding the last one when possible
        public string GetRandomGreeting()
        {
            int idx = _rng.Next(_greetings.Length);
            int attempts = 0;
            while (idx == _lastGreetingIndex && _greetings.Length > 1 && attempts < 6)
            {
                idx = _rng.Next(_greetings.Length);
                attempts++;
            }
            _lastGreetingIndex = idx;
            return _greetings[idx];
        }

        // Return a random fun fact, avoiding the last one when possible
        public string GetRandomFunFact()
        {
            int idx = _rng.Next(_funFacts.Length);
            int attempts = 0;
            while (idx == _lastFunFactIndex && _funFacts.Length > 1 && attempts < 6)
            {
                idx = _rng.Next(_funFacts.Length);
                attempts++;
            }
            _lastFunFactIndex = idx;
            return _funFacts[idx];
        }

        // Replace occurrences of oldValue with newValue using a case-insensitive regex
        private static string ReplaceIgnoreCase(string input, string oldValue, string newValue)
        {
            return System.Text.RegularExpressions.Regex.Replace(input, System.Text.RegularExpressions.Regex.Escape(oldValue), newValue, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }
    }
}
