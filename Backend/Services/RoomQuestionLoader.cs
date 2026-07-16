using Backend.Models;
using Backend.Repositories;

namespace Backend.Services
{
    public interface IRoomQuestionLoader
    {
        void Start(GameRoom room);
        Task WaitForFirstBatchAsync(GameRoom room, CancellationToken cancellationToken);
    }

    public sealed class RoomQuestionLoader : IRoomQuestionLoader
    {
        private const int BatchSize = 40;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<RoomQuestionLoader> _logger;

        public RoomQuestionLoader(IServiceScopeFactory scopeFactory, ILogger<RoomQuestionLoader> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public void Start(GameRoom room)
        {
            lock (room.SyncRoot)
            {
                if (room.QuestionsLoadCompleted) return;
                if (room.QuestionLoadTask != null && !room.QuestionLoadTask.IsCompleted) return;
                if (room.QuestionLoadTask?.IsFaulted == true || room.QuestionLoadTask?.IsCanceled == true || room.QuestionLoadError != null)
                {
                    room.QuestionLoadCancellation.Dispose();
                    room.QuestionLoadCancellation = new CancellationTokenSource();
                    room.FirstQuestionBatchReady = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                    room.QuestionLoadError = null;
                    room.CachedQuestions.Clear();
                }
                room.QuestionLoadTask = LoadBatchesAsync(room, room.QuestionLoadCancellation.Token);
            }
        }

        public async Task WaitForFirstBatchAsync(GameRoom room, CancellationToken cancellationToken)
        {
            Start(room);
            await room.FirstQuestionBatchReady.Task.WaitAsync(cancellationToken);
        }

        private async Task LoadBatchesAsync(GameRoom room, CancellationToken cancellationToken)
        {
            var skip = 0;
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    using var scope = _scopeFactory.CreateScope();
                    var repository = scope.ServiceProvider.GetRequiredService<IQuestionRepository>();
                    var batch = await repository.GetBatchAsync(skip, BatchSize, cancellationToken);
                    var validBatch = batch.Where(IsValidQuestion).ToList();

                    lock (room.SyncRoot)
                    {
                        room.CachedQuestions.AddRange(validBatch);
                    }

                    if (skip == 0)
                    {
                        if (validBatch.Count == 0)
                            throw new InvalidOperationException("Database chưa có câu hỏi hợp lệ.");
                        room.FirstQuestionBatchReady.TrySetResult(true);
                    }

                    _logger.LogInformation(
                        "Loaded question batch for room {RoomCode}: skip {Skip}, received {Count}, valid {ValidCount}.",
                        room.RoomCode, skip, batch.Count, validBatch.Count);

                    if (batch.Count < BatchSize) break;
                    skip += BatchSize;
                    await Task.Delay(150, cancellationToken);
                }

                room.QuestionsLoadCompleted = true;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                room.FirstQuestionBatchReady.TrySetCanceled(cancellationToken);
            }
            catch (Exception ex)
            {
                room.QuestionLoadError = ex.Message;
                room.FirstQuestionBatchReady.TrySetException(ex);
                _logger.LogError(ex, "Failed loading questions for room {RoomCode}.", room.RoomCode);
            }
        }

        private static bool IsValidQuestion(McqQuestion question)
        {
            return !string.IsNullOrWhiteSpace(question.QuestionText)
                && !string.IsNullOrWhiteSpace(question.CorrectAnswer)
                && question.Options.Count >= 2
                && question.Options.Count(option => option.IsCorrect) == 1
                && question.Options.Any(option => option.IsCorrect
                    && string.Equals(option.OptionLetter, question.CorrectAnswer, StringComparison.OrdinalIgnoreCase));
        }
    }
}
