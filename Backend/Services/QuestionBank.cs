using Backend.Models;
using Backend.Repositories;

namespace Backend.Services
{
    public interface IQuestionBank
    {
        Task EnsureLoadedAsync(CancellationToken cancellationToken = default);
        McqQuestion? GetRandom();
        McqQuestion? GetById(string id);
        int Count { get; }
    }

    public sealed class QuestionBank : IQuestionBank
    {
        private const int BatchSize = 100;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<QuestionBank> _logger;
        private readonly SemaphoreSlim _loadLock = new(1, 1);
        private IReadOnlyList<McqQuestion> _questions = Array.Empty<McqQuestion>();
        private IReadOnlyDictionary<string, McqQuestion> _questionsById = new Dictionary<string, McqQuestion>();

        public QuestionBank(IServiceScopeFactory scopeFactory, ILogger<QuestionBank> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public int Count => _questions.Count;

        public async Task EnsureLoadedAsync(CancellationToken cancellationToken = default)
        {
            if (_questions.Count > 0) return;

            await _loadLock.WaitAsync(cancellationToken);
            try
            {
                if (_questions.Count > 0) return;

                var loaded = new List<McqQuestion>();
                var skip = 0;
                while (true)
                {
                    using var scope = _scopeFactory.CreateScope();
                    var repository = scope.ServiceProvider.GetRequiredService<IQuestionRepository>();
                    var batch = await repository.GetBatchAsync(skip, BatchSize, cancellationToken);
                    loaded.AddRange(batch.Where(IsValidQuestion));
                    if (batch.Count < BatchSize) break;
                    skip += BatchSize;
                }

                if (loaded.Count == 0)
                    throw new InvalidOperationException("Database does not contain any valid questions.");

                _questions = loaded;
                _questionsById = loaded.ToDictionary(question => question.Id);
                _logger.LogInformation("Loaded {Count} questions into the shared question bank.", loaded.Count);
            }
            finally
            {
                _loadLock.Release();
            }
        }

        public McqQuestion? GetRandom()
        {
            var questions = _questions;
            return questions.Count == 0 ? null : questions[Random.Shared.Next(questions.Count)];
        }

        public McqQuestion? GetById(string id)
        {
            return _questionsById.TryGetValue(id, out var question) ? question : null;
        }

        private static bool IsValidQuestion(McqQuestion question)
        {
            return !string.IsNullOrWhiteSpace(question.QuestionText)
                && !string.IsNullOrWhiteSpace(question.CorrectAnswer)
                && question.Options.Count >= 2
                && question.Options.Count(option => option.IsCorrect) == 1;
        }
    }
}
