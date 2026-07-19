using MongoDB.Bson.Serialization.Attributes;

namespace Backend.Models
{
    /// <summary>
    /// Represents a single answer option for an MCQ question.
    /// Embedded inside McqQuestion in MongoDB.
    /// </summary>
    public class McqOption
    {
        public string OptionLetter { get; set; } = string.Empty; // "A", "B", "C", "D"
        public string OptionText { get; set; } = string.Empty;
        public bool IsCorrect { get; set; } = false;
    }
}
