using System.Collections.Concurrent;
using Backend.Models;

namespace Backend.Repositories
{
    public class RoomRepository : IRoomRepository
    {
        private readonly ConcurrentDictionary<string, GameRoom> _rooms = new();

        public GameRoom? GetRoom(string roomCode)
        {
            _rooms.TryGetValue(roomCode, out var room);
            return room;
        }

        public GameRoom CreateRoom(string roomCode)
        {
            var room = new GameRoom { RoomCode = roomCode };
            _rooms[roomCode] = room;
            return room;
        }

        public GameRoom AddOrUpdateRoom(GameRoom room)
        {
            _rooms[room.RoomCode] = room;
            return room;
        }

        public bool RoomExists(string roomCode)
        {
            return _rooms.ContainsKey(roomCode);
        }

        public void RemoveRoom(string roomCode)
        {
            if (_rooms.TryRemove(roomCode, out var room))
            {
                room.QuestionLoadCancellation.Cancel();
                room.GameTimerCancellation?.Cancel();
                room.GameTimerCancellation?.Dispose();
                room.GameTimerCancellation = null;
            }
        }

        public IEnumerable<GameRoom> GetAllRooms()
        {
            return _rooms.Values;
        }

        public GameRoom? FindRoomByConnection(string connectionId)
        {
            return _rooms.Values.FirstOrDefault(room =>
                room.GetPlayersSnapshot().Any(player => player.ConnectionId == connectionId));
        }

        public GameRoom? FindRoomBySession(string sessionToken)
        {
            return _rooms.Values.FirstOrDefault(room =>
                room.GetPlayersSnapshot().Any(player => player.SessionToken == sessionToken));
        }
    }
}
