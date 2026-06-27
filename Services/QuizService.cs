using System.Collections.Generic;

namespace CyberAwarenessBot
{
    public class QuizService
    {
        public QuizSession CreateDefaultQuiz()
        {
            return new QuizSession
            {
                IsActive = true,
                CurrentQuestionIndex = 0,
                Score = 0,
                Questions = new List<QuizQuestion>
                {
                    new QuizQuestion
                    {
                        QuestionText = "Which is the strongest password?",
                        Options = new List<string> { "password123", "Summer2024", "BlueMango$Rain7", "qwerty" },
                        CorrectOptionIndex = 2,
                        Explanation = "A long, unique passphrase with mixed character types is much stronger.",
                        IsTrueFalse = false
                    },
                    new QuizQuestion
                    {
                        QuestionText = "True or False: It is safe to click a link if the email uses your name.",
                        Options = new List<string> { "True", "False" },
                        CorrectOptionIndex = 1,
                        Explanation = "False. Attackers often personalize phishing messages.",
                        IsTrueFalse = true
                    },
                    new QuizQuestion
                    {
                        QuestionText = "What should you do before clicking a link in an email?",
                        Options = new List<string> { "Forward it", "Hover over it", "Reply first", "Ignore the sender" },
                        CorrectOptionIndex = 1,
                        Explanation = "Hovering lets you inspect the real destination URL.",
                        IsTrueFalse = false
                    },
                    new QuizQuestion
                    {
                        QuestionText = "True or False: Reusing the same password across websites is a good practice.",
                        Options = new List<string> { "True", "False" },
                        CorrectOptionIndex = 1,
                        Explanation = "False. Reuse means one breach can expose multiple accounts.",
                        IsTrueFalse = true
                    },
                    new QuizQuestion
                    {
                        QuestionText = "Which of these is a sign of phishing?",
                        Options = new List<string> { "Urgent threats", "Correct domain", "Expected invoice", "Known contact style" },
                        CorrectOptionIndex = 0,
                        Explanation = "Urgency is a classic phishing technique.",
                        IsTrueFalse = false
                    },
                    new QuizQuestion
                    {
                        QuestionText = "True or False: Public Wi-Fi is always safe without protection.",
                        Options = new List<string> { "True", "False" },
                        CorrectOptionIndex = 1,
                        Explanation = "False. Public Wi-Fi can expose your traffic unless protected.",
                        IsTrueFalse = true
                    },
                    new QuizQuestion
                    {
                        QuestionText = "What is the best second factor for account protection?",
                        Options = new List<string> { "SMS only", "App-based 2FA", "Birth date", "Username" },
                        CorrectOptionIndex = 1,
                        Explanation = "App-based 2FA is generally safer than SMS.",
                        IsTrueFalse = false
                    },
                    new QuizQuestion
                    {
                        QuestionText = "True or False: Social engineering attacks manipulate people, not just systems.",
                        Options = new List<string> { "True", "False" },
                        CorrectOptionIndex = 0,
                        Explanation = "True. Social engineering targets human trust and behavior.",
                        IsTrueFalse = true
                    },
                    new QuizQuestion
                    {
                        QuestionText = "Which action is safest if a 'friend' urgently asks for money online?",
                        Options = new List<string> { "Send immediately", "Reply with bank details", "Call them directly", "Share your password" },
                        CorrectOptionIndex = 2,
                        Explanation = "Always verify using a trusted channel like a direct phone call.",
                        IsTrueFalse = false
                    },
                    new QuizQuestion
                    {
                        QuestionText = "True or False: Software updates can improve security.",
                        Options = new List<string> { "True", "False" },
                        CorrectOptionIndex = 0,
                        Explanation = "True. Updates often patch known vulnerabilities.",
                        IsTrueFalse = true
                    },
                    new QuizQuestion
                    {
                        QuestionText = "Which is safest for storing many strong passwords?",
                        Options = new List<string> { "Notebook on desk", "Password manager", "Same password everywhere", "Browser tabs" },
                        CorrectOptionIndex = 1,
                        Explanation = "A password manager helps generate and store unique passwords securely.",
                        IsTrueFalse = false
                    },
                    new QuizQuestion
                    {
                        QuestionText = "True or False: If a site has HTTPS, it is automatically trustworthy.",
                        Options = new List<string> { "True", "False" },
                        CorrectOptionIndex = 1,
                        Explanation = "False. HTTPS encrypts traffic, but malicious sites can also use it.",
                        IsTrueFalse = true
                    }
                }
            };
        }

        public bool SubmitAnswer(QuizSession session, int selectedIndex, out string feedback)
        {
            feedback = string.Empty;

            if (session == null || !session.IsActive || session.CurrentQuestion == null)
            {
                feedback = "No active quiz session.";
                return false;
            }

            var question = session.CurrentQuestion;
            bool correct = selectedIndex == question.CorrectOptionIndex;

            if (correct)
            {
                session.Score++;
                feedback = "Correct! " + question.Explanation;
            }
            else
            {
                string correctAnswer = question.Options[question.CorrectOptionIndex];
                feedback = $"Not quite. Correct answer: {correctAnswer}. {question.Explanation}";
            }

            return correct;
        }

        public bool MoveNext(QuizSession session)
        {
            if (session == null) return false;

            if (session.HasNextQuestion)
            {
                session.CurrentQuestionIndex++;
                return true;
            }

            session.IsActive = false;
            return false;
        }

        public string BuildFinalFeedback(QuizSession session)
        {
            if (session == null) return "Quiz complete.";

            int total = session.Questions.Count;
            int score = session.Score;

            if (score == total)
                return $"Outstanding - you scored {score}/{total}! Your cybersecurity awareness is excellent.";
            if (score >= total * 0.75)
                return $"Well done - you scored {score}/{total}. You have strong cybersecurity knowledge.";
            if (score >= total * 0.5)
                return $"Nice effort - you scored {score}/{total}. You're building good cybersecurity awareness.";
            return $"You scored {score}/{total}. That's a good start - keep practicing and you'll improve quickly.";
        }
    }
}