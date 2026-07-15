namespace Backend.Models
{
    /// <summary>
    /// Represents a single answer option for an MCQ question.
    /// Maps to the "mcq_options" table in Supabase.
    /// </summary>
    public class McqOption
    {
        public int Id { get; set; }
        public int QuestionId { get; set; }
        public string OptionLetter { get; set; } = string.Empty; // "A", "B", "C", "D"
        public string OptionText { get; set; } = string.Empty;
        public bool IsCorrect { get; set; } = false;

        // Navigation
        public McqQuestion? Question { get; set; }
    }
}
