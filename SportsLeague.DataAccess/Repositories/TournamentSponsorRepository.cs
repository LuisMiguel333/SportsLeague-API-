using Microsoft.EntityFrameworkCore;
using SportsLeague.DataAccess.Context;
using SportsLeague.Domain.Entities;
using SportsLeague.Domain.Interfaces.Repositories;

namespace SportsLeague.DataAccess.Repositories
{
    public class TournamentSponsorRepository : ITournamentSponsorRepository
    {
        private readonly LeagueDbContext _context;

        public TournamentSponsorRepository(LeagueDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TournamentSponsor>> GetBySponsorIdAsync(int sponsorId)
        {
            return await _context.TournamentSponsors
                .Include(ts => ts.Tournament)
                .Include(ts => ts.Sponsor)
                .Where(ts => ts.SponsorId == sponsorId)
                .ToListAsync();
        }

        public async Task<TournamentSponsor?> GetByTournamentAndSponsorAsync(int tournamentId, int sponsorId)
        {
            return await _context.TournamentSponsors
                .FirstOrDefaultAsync(ts => ts.TournamentId == tournamentId && ts.SponsorId == sponsorId);
        }

        public async Task<TournamentSponsor> CreateAsync(TournamentSponsor entity)
        {
            _context.TournamentSponsors.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task DeleteAsync(TournamentSponsor entity)
        {
            _context.TournamentSponsors.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}