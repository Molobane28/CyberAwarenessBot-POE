using System;
using System.Collections.Generic;

namespace CyberAwarenessBot
{
    /// <summary>
    /// Provides keyword-based responses for cybersecurity topics
    /// </summary>
    public class KeywordResponseProvider
    {
        private readonly Dictionary<string, string[]> _topicResponses;
        private readonly Dictionary<string, string> _topicFollowUps;
        private readonly Random _random = new Random();

        public KeywordResponseProvider()
        {
            // Initialize topic-specific responses
            _topicResponses = new Dictionary<string, string[]>
            {
                ["password"] = new[]
                {
                    "🔑 Strong passwords are essential. Use at least 12 characters with a mix of uppercase, lowercase, numbers, and symbols. Consider using a password manager to generate and store unique passwords.",
                    "🔑 A good password is like a toothbrush - change it regularly, and never share it. Use a passphrase of 4-5 random words for better security.",
                    "🔑 Enable two-factor authentication (2FA) on all accounts that support it. This adds an extra layer of protection even if your password is compromised."
                },
                ["phishing"] = new[]
                {
                    "🎣 Phishing attacks try to steal your information through fake emails or websites. Always check the sender's email address and hover over links before clicking.",
                    "🎣 If an email creates urgency or asks for personal information, it's likely phishing. Contact the organization directly using a known phone number to verify.",
                    "🎣 Never click links or download attachments from unknown senders. When in doubt, delete the email and report it as phishing."
                },
                ["privacy"] = new[]
                {
                    "🛡️ Review your privacy settings on social media regularly. Limit what you share publicly and be careful about posting personal information.",
                    "🛡️ Use a VPN when on public Wi-Fi to encrypt your traffic. Consider using privacy-focused browsers like Firefox or Brave.",
                    "🛡️ Regularly clear your browser cookies and browsing history. Use search engines like DuckDuckGo that don't track your searches."
                },
                ["malware"] = new[]
                {
                    "🦠 Malware includes viruses, ransomware, and spyware. Keep your antivirus software updated and run regular scans.",
                    "🦠 Only download software from official sources. Be wary of free software from unknown websites - they often bundle malware.",
                    "🦠 If your computer shows unusual behavior (pop-ups, slowdowns, strange messages), run a full malware scan immediately."
                },
                ["2fa"] = new[]
                {
                    "🔐 Two-Factor Authentication adds a second verification step. Use authenticator apps like Google Authenticator or Microsoft Authenticator instead of SMS when possible.",
                    "🔐 Even if someone gets your password, 2FA blocks them from accessing your account. Enable it on email, banking, and social media accounts.",
                    "🔐 Keep backup codes for your 2FA in a safe place. If you lose your phone, you'll need these codes to access your accounts."
                },
                ["scam"] = new[]
                {
                    "⚠️ Scammers often impersonate banks, government agencies, or tech support. They create urgency and ask for personal information or money.",
                    "⚠️ If someone calls claiming to be from your bank, hang up and call the official number from your bank's website. Never give out personal information over the phone.",
                    "⚠️ Be suspicious of 'too good to be true' offers, lottery winnings, or inheritance scams. If it sounds too good, it probably is."
                },
                ["ransomware"] = new[]
                {
                    "💰 Ransomware encrypts your files and demands payment. Regular backups are your best defense - keep them offline.",
                    "💰 If infected, don't pay the ransom - there's no guarantee you'll get your files back. Report to law enforcement and restore from backups.",
                    "💰 Prevent ransomware by keeping software updated, using antivirus, and being careful with email attachments and downloads."
                },
                ["breach"] = new[]
                {
                    "💥 Data breaches happen when hackers steal information. Use unique passwords for each site so one breach doesn't compromise all your accounts.",
                    "💥 Check if your accounts have been breached using sites like HaveIBeenPwned.com. If your email appears, change passwords immediately.",
                    "💥 Enable 2FA and consider using a password manager to maintain unique passwords across all your accounts."
                },
                ["encryption"] = new[]
                {
                    "🔐 Encryption scrambles data so only authorized parties can read it. Use end-to-end encrypted messaging apps like Signal or WhatsApp.",
                    "🔐 Encrypt sensitive files on your computer using tools like BitLocker (Windows) or FileVault (Mac).",
                    "🔐 Look for HTTPS in your browser address bar - it means your connection is encrypted."
                },
                ["vpn"] = new[]
                {
                    "🔒 A VPN encrypts your internet connection and hides your IP address. Use a reputable VPN service, especially on public Wi-Fi.",
                    "🔒 Free VPNs often log your data and sell it. Choose a paid VPN with a no-logs policy for better privacy.",
                    "🔒 Always enable your VPN when using public Wi-Fi networks like airports, cafes, or hotels."
                },
                ["wifi"] = new[]
                {
                    "📶 Secure your home Wi-Fi with WPA3 (or WPA2) encryption and a strong password. Change the default router login credentials.",
                    "📶 Don't use public Wi-Fi for banking or sensitive transactions. Use a VPN or your mobile data instead.",
                    "📶 Keep your router's firmware updated to patch security vulnerabilities."
                },
                ["social engineering"] = new[]
                {
                    "🧠 Social engineering manipulates people into revealing information. Always verify requests through a trusted channel.",
                    "🧠 Be wary of urgent requests, authority claims, or emotional manipulation. Scammers use these tactics to bypass your better judgment.",
                    "🧠 Train your team or family about social engineering tactics. Awareness is the best defense."
                },
                ["backup"] = new[]
                {
                    "💾 Follow the 3-2-1 backup rule: 3 copies, 2 different media types, 1 offsite copy. This protects against hardware failure and ransomware.",
                    "💾 Test your backups regularly to ensure they work. A backup is useless if it can't be restored.",
                    "💾 Consider cloud backups as part of your strategy. Services like Backblaze or iCloud provide automatic, offsite backup."
                },
                ["update"] = new[]
                {
                    "🔄 Software updates patch security vulnerabilities. Enable automatic updates for your operating system and applications.",
                    "🔄 Don't delay updates - attackers actively exploit known vulnerabilities. Update as soon as possible.",
                    "🔄 Check for firmware updates for your router and IoT devices too. These are often overlooked but critical for security."
                }
            };

            // Follow-up tips for each topic
            _topicFollowUps = new Dictionary<string, string>
            {
                ["password"] = "For even better security, consider using a password manager like Bitwarden or LastPass. They generate and store strong passwords for you.",
                ["phishing"] = "Always check the URL before entering any credentials. Phishing sites often have slight variations of real domains.",
                ["privacy"] = "Review app permissions on your phone too. Many apps request access they don't need.",
                ["malware"] = "Consider using Malwarebytes or Windows Defender for daily protection. Both are effective and free.",
                ["2fa"] = "Some services offer hardware keys like YubiKey for the strongest 2FA protection.",
                ["scam"] = "Report scams to your local authorities or to organizations like the FTC's ReportFraud.ftc.gov.",
                ["ransomware"] = "Cloud backup services like Backblaze or iCloud provide automatic, offsite backup.",
                ["breach"] = "Set up a breach alert service that notifies you if your credentials appear in a data dump.",
                ["encryption"] = "For email, use PGP encryption or services like ProtonMail that offer built-in encryption.",
                ["vpn"] = "Choose a VPN that doesn't keep logs and is based in a privacy-friendly jurisdiction.",
                ["wifi"] = "Consider setting up a guest network for visitors to keep your main network secure.",
                ["social engineering"] = "Implement a verification protocol for any sensitive requests (e.g., call back on a known number).",
                ["backup"] = "Consider using a NAS (Network Attached Storage) with RAID for local backup redundancy.",
                ["update"] = "Set a weekly reminder to check for updates if you don't have automatic updates enabled."
            };
        }

        /// <summary>
        /// Get response for a detected keyword/topic
        /// </summary>
        public string GetResponse(string input, ConversationState state)
        {
            string lower = input.ToLowerInvariant();

            // Check all topics
            foreach (var topic in _topicResponses.Keys)
            {
                if (lower.Contains(topic))
                {
                    state.LastTopic = topic;
                    var responses = _topicResponses[topic];
                    return responses[_random.Next(responses.Length)];
                }
            }

            return null;
        }

        /// <summary>
        /// Get follow-up response for a topic
        /// </summary>
        public string GetFollowUpFor(string topic)
        {
            if (topic == null) return null;
            topic = topic.ToLowerInvariant();

            if (_topicFollowUps.TryGetValue(topic, out string followUp))
                return followUp;

            return null;
        }

        /// <summary>
        /// Check if a topic is supported
        /// </summary>
        public bool HasTopic(string topic)
        {
            if (topic == null) return false;
            return _topicResponses.ContainsKey(topic.ToLowerInvariant());
        }
    }
}