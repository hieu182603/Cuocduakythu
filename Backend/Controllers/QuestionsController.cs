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
        public async Task<IActionResult> GetAll()
        {
            var questions = await _questionRepo.GetAllAsync();
            var result = questions.Select(q => new
            {
                q.Id,
                q.QuestionNumber,
                q.QuestionText,
                q.CorrectAnswer,
                PartName = q.Part?.PartName ?? "",
                Difficulty = q.Part?.Difficulty ?? "medium",
                Options = q.Options.OrderBy(o => o.OptionLetter).Select(o => new
                {
                    o.OptionLetter,
                    o.OptionText,
                    o.IsCorrect
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
                q.CorrectAnswer,
                PartName = q.Part?.PartName ?? "",
                Difficulty = q.Part?.Difficulty ?? "medium",
                Options = q.Options.OrderBy(o => o.OptionLetter).Select(o => new
                {
                    o.OptionLetter,
                    o.OptionText,
                    o.IsCorrect
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
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var question = await _questionRepo.GetByIdAsync(id);
            if (question == null) return NotFound();
            return Ok(new
            {
                question.Id,
                question.QuestionNumber,
                question.QuestionText,
                question.CorrectAnswer,
                PartName = question.Part?.PartName ?? "",
                Difficulty = question.Part?.Difficulty ?? "medium",
                Options = question.Options.OrderBy(o => o.OptionLetter).Select(o => new
                {
                    o.OptionLetter,
                    o.OptionText,
                    o.IsCorrect
                })
            });
        }
    }
}
