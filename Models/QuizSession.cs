using System.Collections.Generic;

namespace CyberAwarenessBot
{
    public class QuizSession
    {
        public List<QuizQuestion> Questions { get; set; } = new List<QuizQuestion>();
        public int CurrentQuestionIndex { get; set; }
        public int Score { get; set; }
        public bool IsActive { get; set; }

        public QuizQuestion CurrentQuestion
        {
            get
            {
                if (Questions == null || Questions.Count == 0) return null;
                if (CurrentQuestionIndex < 0 || CurrentQuestionIndex >= Questions.Count) return null;
                return Questions[CurrentQuestionIndex];
            }
        }

        public bool HasNextQuestion => CurrentQuestionIndex < Questions.Count - 1;
    }
}