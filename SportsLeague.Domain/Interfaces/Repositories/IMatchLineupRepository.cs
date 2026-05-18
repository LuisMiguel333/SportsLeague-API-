using SportsLeague.Domain.Entities;

namespace SportsLeague.Domain.Interfaces.Repositories;

public interface IMatchLineupRepository
{
    Task<MatchLineup?> GetByIdAsync(int id);
    Task<IEnumerable<MatchLineup>> GetByMatchAsync(int matchId);
    Task<IEnumerable<MatchLineup>> GetByMatchAndTeamAsync(int matchId, int teamId);
    Task<bool> ExistsByMatchAndPlayerAsync(int matchId, int playerId);
    Task<MatchLineup> CreateAsync(MatchLineup lineup);
    Task DeleteAsync(MatchLineup lineup);
}