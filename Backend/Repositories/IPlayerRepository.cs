using Backend.Models;

namespace Backend.Repositories
{
    public interface IPlayerRepository
    {
        Player? GetPlayer(GameRoom room, string connectionId);
        Player? GetPlayerById(GameRoom room, int playerId);
        void AddPlayer(GameRoom room, Player player);
        void RemovePlayer(GameRoom room, string connectionId);
        void UpdatePosition(Player player, int newTileIndex);
    }
}
