using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Backend.Models
{
    /// <summary>
    /// Represents a part/section of the MCQ question bank.
    /// Maps to the "mcq_parts" collection in MongoDB.
    /// </summary>
    public class McqPart
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;
        
        public string PartName { get; set; } = string.Empty;
        public string Difficulty { get; set; } = "medium";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        [BsonIgnore]
        public List<McqQuestion> Questions { get; set; } = new();
    }
}
