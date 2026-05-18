using Microsoft.Extensions.Logging;
using SportsLeague.Domain.Entities;
using SportsLeague.Domain.Enums;
using SportsLeague.Domain.Interfaces.Repositories;
using SportsLeague.Domain.Interfaces.Services;

namespace SportsLeague.Domain.Services;

public class MatchLineupService : IMatchLineupService
{
    private readonly IMatchLineupRepository _lineupRepository;
    private readonly IMatchRepository _matchRepository;
    private readonly IPlayerRepository _playerRepository;
    private readonly ILogger<MatchLineupService> _logger;

    public MatchLineupService(
        IMatchLineupRepository lineupRepository,
        IMatchRepository matchRepository,
        IPlayerRepository playerRepository,
        ILogger<MatchLineupService> logger)
    {
        _lineupRepository = lineupRepository;
        _matchRepository = matchRepository;
        _playerRepository = playerRepository;
        _logger = logger;
    }

    public async Task<MatchLineup> AddPlayerToLineupAsync(int matchId, int playerId, bool isStarter, string position)
    {
        // V1 - El partido debe existir
        var match = await _matchRepository.GetByIdAsync(matchId);
        if (match == null)
            throw new KeyNotFoundException($"No se encontró el partido con ID {matchId}");

        // V6 - El partido debe estar en estado Scheduled
        if (match.Status != MatchStatus.Scheduled)
            throw new InvalidOperationException("Solo se pueden registrar alineaciones en partidos Scheduled");

        // V2 - El jugador debe existir
        var player = await _playerRepository.GetByIdAsync(playerId);
        if (player == null)
            throw new KeyNotFoundException($"No se encontró el jugador con ID {playerId}");

        // V3 - El jugador debe pertenecer al HomeTeam o AwayTeam
        if (player.TeamId != match.HomeTeamId && player.TeamId != match.AwayTeamId)
            throw new InvalidOperationException("El jugador no pertenece a ninguno de los equipos del partido");

        // V4 - No puede estar registrado dos veces
        if (await _lineupRepository.ExistsByMatchAndPlayerAsync(matchId, playerId))
            throw new InvalidOperationException("El jugador ya está registrado en la alineación de este partido");

        // V5 - Máximo 11 titulares por equipo
        if (isStarter)
        {
            var teamLineup = await _lineupRepository.GetByMatchAndTeamAsync(matchId, player.TeamId);
            var starterCount = teamLineup.Count(l => l.IsStarter);
            if (starterCount >= 11)
                throw new InvalidOperationException("El equipo ya tiene 11 titulares registrados en este partido");
        }

        var lineup = new MatchLineup
        {
            MatchId = matchId,
            PlayerId = playerId,
            IsStarter = isStarter,
            Position = position
        };

        var created = await _lineupRepository.CreateAsync(lineup);
        _logger.LogInformation("Player {PlayerId} added to lineup of match {MatchId}", playerId, matchId);
        return created;
    }

    public async Task<IEnumerable<MatchLineup>> GetLineupByMatchAsync(int matchId)
    {
        var match = await _matchRepository.GetByIdAsync(matchId);
        if (match == null)
            throw new KeyNotFoundException($"No se encontró el partido con ID {matchId}");

        return await _lineupRepository.GetByMatchAsync(matchId);
    }

    public async Task<IEnumerable<MatchLineup>> GetLineupByMatchAndTeamAsync(int matchId, int teamId)
    {
        var match = await _matchRepository.GetByIdAsync(matchId);
        if (match == null)
            throw new KeyNotFoundException($"No se encontró el partido con ID {matchId}");

        return await _lineupRepository.GetByMatchAndTeamAsync(matchId, teamId);
    }

    public async Task DeleteLineupEntryAsync(int matchId, int id)
    {
        var lineup = await _lineupRepository.GetByIdAsync(id);
        if (lineup == null || lineup.MatchId != matchId)
            throw new KeyNotFoundException($"No se encontró el registro de alineación con ID {id}");

        await _lineupRepository.DeleteAsync(lineup);
        _logger.LogInformation("Lineup entry {Id} removed from match {MatchId}", id, matchId);
    }
}