using Backend.Models;

namespace Backend.Repositories
{
    public interface IRoomRepository
    {
        GameRoom? GetRoom(string roomCode);
        GameRoom CreateRoom(string roomCode);
        bool RoomExists(string roomCode);
        void RemoveRoom(string roomCode);
        IEnumerable<GameRoom> GetAllRooms();
    }
}
