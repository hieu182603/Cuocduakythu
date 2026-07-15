using System.Collections.Concurrent;
using Backend.Models;

namespace Backend.Repositories
{
    public class RoomRepository : IRoomRepository
    {
        private static readonly ConcurrentDictionary<string, GameRoom> Rooms = new();

        public GameRoom? GetRoom(string roomCode)
        {
            Rooms.TryGetValue(roomCode, out var room);
            return room;
        }

        public GameRoom CreateRoom(string roomCode)
        {
            var room = new GameRoom { RoomCode = roomCode };
            Rooms[roomCode] = room;
            return room;
        }

        public bool RoomExists(string roomCode)
        {
            return Rooms.ContainsKey(roomCode);
        }

        public void RemoveRoom(string roomCode)
        {
            Rooms.TryRemove(roomCode, out _);
        }

        public IEnumerable<GameRoom> GetAllRooms()
        {
            return Rooms.Values;
        }
    }
}
