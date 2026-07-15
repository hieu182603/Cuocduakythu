namespace Backend.Models
{
    /// <summary>
    /// Represents a part/section of the MCQ question bank.
    /// Maps to the "mcq_parts" table in Supabase.
    /// </summary>
    public class McqPart
    {
        public int Id { get; set; }
        public string PartName { get; set; } = string.Empty;
        public string Difficulty { get; set; } = "medium";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public List<McqQuestion> Questions { get; set; } = new();
    }
}
