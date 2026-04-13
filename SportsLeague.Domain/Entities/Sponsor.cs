using SportsLeague.Domain.Entities;
using SportsLeague.Domain.Enums;

public class Sponsor : AuditBase
{
    public string Name { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? WebsiteUrl { get; set; }
    public SponsorCategory Category { get; set; }

    public ICollection<TournamentSponsor> TournamentSponsors { get; set; } = new List<TournamentSponsor>();
}