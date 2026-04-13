using System.Text.RegularExpressions;
using SportsLeague.Domain.Entities;
using SportsLeague.Domain.Interfaces.Repositories;
using SportsLeague.Domain.Interfaces.Services;

namespace SportsLeague.API.Services
{
    public class SponsorService : ISponsorService
    {
        private readonly ISponsorRepository _sponsorRepository;
        private readonly ITournamentSponsorRepository _tournamentSponsorRepository;
        private readonly IGenericRepository<Tournament> _tournamentRepository;

        public SponsorService(
            ISponsorRepository sponsorRepository,
            ITournamentSponsorRepository tournamentSponsorRepository,
            IGenericRepository<Tournament> tournamentRepository)
        {
            _sponsorRepository = sponsorRepository;
            _tournamentSponsorRepository = tournamentSponsorRepository;
            _tournamentRepository = tournamentRepository;
        }

        public async Task<IEnumerable<Sponsor>> GetAllAsync()
            => await _sponsorRepository.GetAllAsync();

        public async Task<Sponsor> GetByIdAsync(int id)
        {
            var sponsor = await _sponsorRepository.GetByIdAsync(id);
            if (sponsor == null) throw new KeyNotFoundException($"Patrocinadora con ID {id} no encontrada");
            return sponsor;
        }

        public async Task<Sponsor> CreateAsync(Sponsor sponsor)
        {
            ValidateEmail(sponsor.ContactEmail);
            if (await _sponsorRepository.ExistsByNameAsync(sponsor.Name))
                throw new InvalidOperationException($"Ya existe un patrocinador con el nombre '{sponsor.Name}'.");
            sponsor.CreatedAt = DateTime.UtcNow;
            return await _sponsorRepository.CreateAsync(sponsor);
        }

        public async Task UpdateAsync(int id, Sponsor sponsor)
        {
            var existing = await _sponsorRepository.GetByIdAsync(id);
            if (existing == null) throw new KeyNotFoundException($"Patrocinadora con ID {id} no encontrada.");
            ValidateEmail(sponsor.ContactEmail);
            if (await _sponsorRepository.ExistsByNameAsync(sponsor.Name, id))
                throw new InvalidOperationException($"Ya existe un patrocinador con el nombre '{sponsor.Name}'.");
            existing.Name = sponsor.Name;
            existing.ContactEmail = sponsor.ContactEmail;
            existing.Phone = sponsor.Phone;
            existing.WebsiteUrl = sponsor.WebsiteUrl;
            existing.Category = sponsor.Category;
            existing.UpdatedAt = DateTime.UtcNow;
            await _sponsorRepository.UpdateAsync(existing);
        }

        public async Task DeleteAsync(int id)
        {
            var sponsor = await _sponsorRepository.GetByIdAsync(id);
            if (sponsor == null) throw new KeyNotFoundException($"Patrocinadora con ID {id} no encontrada.");
            await _sponsorRepository.DeleteAsync(sponsor.Id);
        }

        public async Task<IEnumerable<TournamentSponsor>> GetTournamentsAsync(int sponsorId)
        {
            var sponsor = await _sponsorRepository.GetByIdAsync(sponsorId);
            if (sponsor == null) throw new KeyNotFoundException($"No se encontró el sponsor con el ID {sponsorId} .");
            return await _tournamentSponsorRepository.GetBySponsorIdAsync(sponsorId);
        }

        public async Task<TournamentSponsor> LinkToTournamentAsync(int sponsorId, int tournamentId, decimal contractAmount)
        {
            var sponsor = await _sponsorRepository.GetByIdAsync(sponsorId);
            if (sponsor == null) throw new KeyNotFoundException($"No se encontró el patrocinador con ID{sponsorId}.");
            var tournament = await _tournamentRepository.GetByIdAsync(tournamentId);
            if (tournament == null) throw new KeyNotFoundException($"No se encontró el torneo con ID {tournamentId} ");
            if (contractAmount <= 0)
                throw new InvalidOperationException("El importe del contrato debe ser mayor que 0.");
            var existing = await _tournamentSponsorRepository.GetByTournamentAndSponsorAsync(tournamentId, sponsorId);
            if (existing != null)
                throw new InvalidOperationException("Este patrocinador ya está vinculado al torneo especificado.");
            var link = new TournamentSponsor
            {
                SponsorId = sponsorId,
                TournamentId = tournamentId,
                ContractAmount = contractAmount,
                JoinedAt = DateTime.UtcNow
            };
            return await _tournamentSponsorRepository.CreateAsync(link);
        }

        public async Task UnlinkFromTournamentAsync(int sponsorId, int tournamentId)
        {
            var link = await _tournamentSponsorRepository.GetByTournamentAndSponsorAsync(tournamentId, sponsorId);
            if (link == null)
                throw new KeyNotFoundException("El patrocinador no está vinculado al torneo en cuestión.");
            await _tournamentSponsorRepository.DeleteAsync(link);
        }

        private static void ValidateEmail(string email)
        {
            var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            if (!regex.IsMatch(email))
                throw new InvalidOperationException($"'{email}'no es un formato de correo electrónico válido.");
        }
    }
}