using Backend.Models;

namespace Backend.Repositories
{
    public interface IQuestionRepository
    {
        Task<List<McqQuestion>> GetBatchAsync(int skip, int take, CancellationToken cancellationToken = default);

        /// <summary>Get questions filtered by part.</summary>
        Task<List<McqQuestion>> GetByPartAsync(string partId);

        /// <summary>Get N random questions with options loaded.</summary>
        Task<List<McqQuestion>> GetRandomAsync(int count);

        /// <summary>Get a single question by ID with options.</summary>
        Task<McqQuestion?> GetByIdAsync(string id);

        /// <summary>Get all available parts.</summary>
        Task<List<McqPart>> GetAllPartsAsync();
    }
}
