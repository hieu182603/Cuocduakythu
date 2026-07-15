using System.Collections.Generic;

namespace CuocDuaKyThu.Data
{
    /// <summary>
    /// Data transfer object for questions received from the backend API.
    /// Deserialized from JSON responses from /api/questions.
    /// </summary>
    [System.Serializable]
    public class QuestionDTO
    {
        public int id;
        public int questionNumber;
        public string questionText;
        public string correctAnswer;   // "A", "B", "C", "D"
        public string partName;
        public string difficulty;
        public List<OptionDTO> options;

        /// <summary>Get the 0-based index of the correct answer (0=A, 1=B, 2=C, 3=D).</summary>
        public int CorrectIndex
        {
            get
            {
                if (string.IsNullOrEmpty(correctAnswer)) return -1;
                return correctAnswer.ToUpper()[0] - 'A';
            }
        }
    }

    [System.Serializable]
    public class OptionDTO
    {
        public string optionLetter;  // "A", "B", "C", "D"
        public string optionText;
        public bool isCorrect;
    }

    /// <summary>Wrapper for JSON array deserialization.</summary>
    [System.Serializable]
    public class QuestionListWrapper
    {
        public List<QuestionDTO> questions;
    }
}
