using Backend.Models;

namespace Backend.Repositories
{
    public class PlayerRepository : IPlayerRepository
    {
        public Player? GetPlayer(GameRoom room, string connectionId)
        {
            return room.Players.FirstOrDefault(p => p.ConnectionId == connectionId);
        }

        public Player? GetPlayerById(GameRoom room, int playerId)
        {
            return room.Players.FirstOrDefault(p => p.Id == playerId);
        }

        public void AddPlayer(GameRoom room, Player player)
        {
            room.Players.Add(player);
        }

        public void RemovePlayer(GameRoom room, string connectionId)
        {
            var player = room.Players.FirstOrDefault(p => p.ConnectionId == connectionId);
            if (player != null)
            {
                room.Players.Remove(player);
            }
        }

        public void UpdatePosition(Player player, int newTileIndex)
        {
            player.TileIndex = newTileIndex;
        }
    }
}
