using Backend.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuestionsController : ControllerBase
    {
        private readonly IQuestionRepository _questionRepo;

        public QuestionsController(IQuestionRepository questionRepo)
        {
            _questionRepo = questionRepo;
        }

        /// <summary>GET /api/questions — All questions with options.</summary>
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int skip = 0, [FromQuery] int take = 40)
        {
            skip = Math.Max(0, skip);
            take = Math.Clamp(take, 1, 100);
            var questions = await _questionRepo.GetBatchAsync(skip, take, HttpContext.RequestAborted);
            var result = questions.Select(q => new
            {
                q.Id,
                q.QuestionNumber,
                q.QuestionText,
                PartName = q.Part?.PartName ?? "",
                Difficulty = q.Part?.Difficulty ?? "medium",
                Options = q.Options.OrderBy(o => o.OptionLetter).Select(o => new
                {
                    o.OptionLetter,
                    o.OptionText
                })
            });
            return Ok(result);
        }

        /// <summary>GET /api/questions/random?count=10 — Random questions.</summary>
        [HttpGet("random")]
        public async Task<IActionResult> GetRandom([FromQuery] int count = 10)
        {
            count = Math.Clamp(count, 1, 200);
            var questions = await _questionRepo.GetRandomAsync(count);
            var result = questions.Select(q => new
            {
                q.Id,
                q.QuestionNumber,
                q.QuestionText,
                PartName = q.Part?.PartName ?? "",
                Difficulty = q.Part?.Difficulty ?? "medium",
                Options = q.Options.OrderBy(o => o.OptionLetter).Select(o => new
                {
                    o.OptionLetter,
                    o.OptionText
                })
            });
            return Ok(result);
        }

        /// <summary>GET /api/questions/parts — All parts with difficulty.</summary>
        [HttpGet("parts")]
        public async Task<IActionResult> GetParts()
        {
            var parts = await _questionRepo.GetAllPartsAsync();
            return Ok(parts.Select(p => new
            {
                p.Id,
                p.PartName,
                p.Difficulty
            }));
        }

        /// <summary>GET /api/questions/{id} — Single question by ID.</summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var question = await _questionRepo.GetByIdAsync(id);
            if (question == null) return NotFound();
            return Ok(new
            {
                question.Id,
                question.QuestionNumber,
                question.QuestionText,
                PartName = question.Part?.PartName ?? "",
                Difficulty = question.Part?.Difficulty ?? "medium",
                Options = question.Options.OrderBy(o => o.OptionLetter).Select(o => new
                {
                    o.OptionLetter,
                    o.OptionText
                })
            });
        }

        public sealed record AnswerSubmission(int AnswerIndex);

        /// <summary>Validate one answer without exposing the answer key.</summary>
        [HttpPost("{id}/answer")]
        public async Task<IActionResult> CheckAnswer(string id, [FromBody] AnswerSubmission submission)
        {
            var question = await _questionRepo.GetByIdAsync(id);
            if (question == null) return NotFound();

            var orderedOptions = question.Options.OrderBy(o => o.OptionLetter).ToList();
            var correctIndex = orderedOptions.FindIndex(o =>
                string.Equals(o.OptionLetter, question.CorrectAnswer, StringComparison.OrdinalIgnoreCase));
            if (correctIndex < 0) return Problem("Dữ liệu đáp án của câu hỏi không hợp lệ.");

            return Ok(new
            {
                IsCorrect = submission.AnswerIndex == correctIndex,
                CorrectIndex = correctIndex
            });
        }
    }
}
