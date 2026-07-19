using Backend.Models;

namespace Backend.Repositories
{
    public interface IRoomRepository
    {
        GameRoom? GetRoom(string roomCode);
        GameRoom CreateRoom(string roomCode);
        GameRoom AddOrUpdateRoom(GameRoom room);
        bool RoomExists(string roomCode);
        void RemoveRoom(string roomCode);
        IEnumerable<GameRoom> GetAllRooms();
        GameRoom? FindRoomByConnection(string connectionId);
        GameRoom? FindRoomBySession(string sessionToken);
    }
}
