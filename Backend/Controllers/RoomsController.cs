using Backend.Repositories;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoomsController : ControllerBase
    {
        private readonly IRoomRepository _roomRepo;
        private readonly IGameService _gameService;

        public RoomsController(IRoomRepository roomRepo, IGameService gameService)
        {
            _roomRepo = roomRepo;
            _gameService = gameService;
        }

        /// <summary>POST /api/rooms — Create a new room, returns room code.</summary>
        [HttpPost]
        public IActionResult CreateRoom()
        {
            string code = _gameService.GenerateRoomCode();

            // Ensure uniqueness
            while (_roomRepo.RoomExists(code))
            {
                code = _gameService.GenerateRoomCode();
            }

            var room = _roomRepo.CreateRoom(code);
            return Ok(new { RoomCode = room.RoomCode });
        }

        /// <summary>GET /api/rooms/{code} — Get room info.</summary>
        [HttpGet("{code}")]
        public IActionResult GetRoom(string code)
        {
            var room = _roomRepo.GetRoom(code.ToUpper());
            if (room == null) return NotFound(new { Message = "Phòng không tồn tại." });

            return Ok(new
            {
                room.RoomCode,
                room.IsStarted,
                PlayerCount = room.Players.Count,
                Players = room.Players.Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.HorseId,
                    p.TileIndex
                }),
                ActivePlayerIndex = room.ActivePlayerIndex
            });
        }

        /// <summary>GET /api/rooms/{code}/leaderboard — Get leaderboard sorted by position.</summary>
        [HttpGet("{code}/leaderboard")]
        public IActionResult GetLeaderboard(string code)
        {
            var room = _roomRepo.GetRoom(code.ToUpper());
            if (room == null) return NotFound(new { Message = "Phòng không tồn tại." });

            var leaderboard = room.Players
                .OrderByDescending(p => p.TileIndex)
                .ThenBy(p => p.Id)
                .Select((p, idx) => new
                {
                    Rank = idx + 1,
                    p.Id,
                    p.Name,
                    p.HorseId,
                    TilePosition = p.TileIndex + 1,
                    p.LapCount
                });

            return Ok(leaderboard);
        }

    }
}
