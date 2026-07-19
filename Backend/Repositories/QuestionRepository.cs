using Backend.Models;
using MongoDB.Driver;

namespace Backend.Repositories
{
    public class QuestionRepository : IQuestionRepository
    {
        private readonly IMongoCollection<McqQuestion> _questions;
        private readonly IMongoCollection<McqPart> _parts;

        public QuestionRepository(IMongoDatabase db)
        {
            _questions = db.GetCollection<McqQuestion>("mcq_questions");
            _parts = db.GetCollection<McqPart>("mcq_parts");
        }

        public async Task<List<McqQuestion>> GetByPartAsync(string partId)
        {
            return await _questions.Find(q => q.PartId == partId)
                                   .SortBy(q => q.QuestionNumber)
                                   .ToListAsync();
        }

        public async Task<List<McqQuestion>> GetBatchAsync(int skip, int take, CancellationToken cancellationToken = default)
        {
            skip = Math.Max(0, skip);
            take = Math.Clamp(take, 1, 100);
            return await _questions.Find(_ => true)
                                   .SortBy(q => q.QuestionNumber)
                                   .Skip(skip)
                                   .Limit(take)
                                   .ToListAsync(cancellationToken);
        }

        public async Task<List<McqQuestion>> GetRandomAsync(int count)
        {
            var pipeline = new EmptyPipelineDefinition<McqQuestion>()
                .Sample(count);
            
            return await _questions.Aggregate(pipeline).ToListAsync();
        }

        public async Task<McqQuestion?> GetByIdAsync(string id)
        {
            return await _questions.Find(q => q.Id == id).FirstOrDefaultAsync();
        }

        public async Task<List<McqPart>> GetAllPartsAsync()
        {
            return await _parts.Find(_ => true)
                               .SortBy(p => p.Id)
                               .ToListAsync();
        }
    }
}
