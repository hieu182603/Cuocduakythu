namespace Backend.Models
{
    public sealed record PlayerSnapshot(
        int Id,
        string ConnectionId,
        string Name,
        string HorseId,
        int TileIndex,
        int WrongStreak,
        bool Shield,
        int FreezeTimeMs,
        bool DoubleDice,
        bool IsAutoRoll,
        int DiceModifier,
        int LapCount,
        bool IsSpectator,
        bool IsHost)
    {
        public static PlayerSnapshot From(Player player)
        {
            lock (player)
            {
                return new PlayerSnapshot(
                    player.Id,
                    player.ConnectionId,
                    player.Name,
                    player.HorseId,
                    player.TileIndex,
                    player.WrongStreak,
                    player.Shield,
                    player.GetRemainingFreezeTimeMs(),
                    player.DoubleDice,
                    player.IsAutoRoll,
                    player.DiceModifier,
                    player.LapCount,
                    player.IsSpectator,
                    player.IsHost);
            }
        }
    }
}
