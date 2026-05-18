using Microsoft.EntityFrameworkCore;
using SportsLeague.DataAccess.Context;
using SportsLeague.Domain.Entities;
using SportsLeague.Domain.Interfaces.Repositories;

namespace SportsLeague.DataAccess.Repositories;

public class MatchLineupRepository : IMatchLineupRepository
{
    private readonly LeagueDbContext _context;

    public MatchLineupRepository(LeagueDbContext context)
    {
        _context = context;
    }

    public async Task<MatchLineup?> GetByIdAsync(int id)
        => await _context.MatchLineups
            .Include(ml => ml.Player).ThenInclude(p => p.Team)
            .Include(ml => ml.Match)
            .FirstOrDefaultAsync(ml => ml.Id == id);

    public async Task<IEnumerable<MatchLineup>> GetByMatchAsync(int matchId)
        => await _context.MatchLineups
            .Include(ml => ml.Player).ThenInclude(p => p.Team)
            .Where(ml => ml.MatchId == matchId)
            .ToListAsync();

    public async Task<IEnumerable<MatchLineup>> GetByMatchAndTeamAsync(int matchId, int teamId)
        => await _context.MatchLineups
            .Include(ml => ml.Player).ThenInclude(p => p.Team)
            .Where(ml => ml.MatchId == matchId && ml.Player.TeamId == teamId)
            .ToListAsync();

    public async Task<bool> ExistsByMatchAndPlayerAsync(int matchId, int playerId)
        => await _context.MatchLineups
            .AnyAsync(ml => ml.MatchId == matchId && ml.PlayerId == playerId);

    public async Task<MatchLineup> CreateAsync(MatchLineup lineup)
    {
        _context.MatchLineups.Add(lineup);
        await _context.SaveChangesAsync();
        return await GetByIdAsync(lineup.Id) ?? lineup;
    }

    public async Task DeleteAsync(MatchLineup lineup)
    {
        _context.MatchLineups.Remove(lineup);
        await _context.SaveChangesAsync();
    }
}