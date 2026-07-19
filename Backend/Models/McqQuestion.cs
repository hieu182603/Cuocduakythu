using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Backend.Models
{
    /// <summary>
    /// Represents a single MCQ question.
    /// Maps to the "mcq_questions" collection in MongoDB.
    /// </summary>
    public class McqQuestion
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;
        
        public int QuestionNumber { get; set; }
        
        [BsonRepresentation(BsonType.ObjectId)]
        public string PartId { get; set; } = string.Empty;
        
        public string QuestionText { get; set; } = string.Empty;
        public string CorrectAnswer { get; set; } = string.Empty; // e.g. "A", "B", "C", "D"
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        [BsonIgnore]
        public McqPart? Part { get; set; }
        
        public List<McqOption> Options { get; set; } = new();
    }
}
