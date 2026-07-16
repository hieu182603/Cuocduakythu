using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories
{
    public class QuestionRepository : IQuestionRepository
    {
        private readonly AppDbContext _db;

        public QuestionRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<McqQuestion>> GetByPartAsync(int partId)
        {
            return await _db.McqQuestions
                .Include(q => q.Options)
                .Include(q => q.Part)
                .Where(q => q.PartId == partId)
                .OrderBy(q => q.QuestionNumber)
                .ToListAsync();
        }

        public Task<List<McqQuestion>> GetBatchAsync(int skip, int take, CancellationToken cancellationToken = default)
        {
            skip = Math.Max(0, skip);
            take = Math.Clamp(take, 1, 100);
            return _db.McqQuestions
                .AsNoTracking()
                .Include(q => q.Options)
                .Include(q => q.Part)
                .OrderBy(q => q.QuestionNumber)
                .Skip(skip)
                .Take(take)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<McqQuestion>> GetRandomAsync(int count)
        {
            // Use EF Core random ordering for PostgreSQL
            return await _db.McqQuestions
                .Include(q => q.Options)
                .Include(q => q.Part)
                .OrderBy(q => EF.Functions.Random())
                .Take(count)
                .ToListAsync();
        }

        public async Task<McqQuestion?> GetByIdAsync(int id)
        {
            return await _db.McqQuestions
                .Include(q => q.Options)
                .Include(q => q.Part)
                .FirstOrDefaultAsync(q => q.Id == id);
        }

        public async Task<List<McqPart>> GetAllPartsAsync()
        {
            return await _db.McqParts
                .OrderBy(p => p.Id)
                .ToListAsync();
        }
    }
}
