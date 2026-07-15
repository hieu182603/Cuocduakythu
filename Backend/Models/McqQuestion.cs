namespace Backend.Models
{
    /// <summary>
    /// Represents a single MCQ question.
    /// Maps to the "mcq_questions" table in Supabase.
    /// </summary>
    public class McqQuestion
    {
        public int Id { get; set; }
        public int QuestionNumber { get; set; }
        public int PartId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string CorrectAnswer { get; set; } = string.Empty; // e.g. "A", "B", "C", "D"
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public McqPart? Part { get; set; }
        public List<McqOption> Options { get; set; } = new();
    }
}
