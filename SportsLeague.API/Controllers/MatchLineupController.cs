using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SportsLeague.API.DTOs.Request;
using SportsLeague.API.DTOs.Response;
using SportsLeague.Domain.Interfaces.Services;

namespace SportsLeague.API.Controllers;

[ApiController]
[Route("api/match/{matchId}/lineup")]
public class MatchLineupController : ControllerBase
{
    private readonly IMatchLineupService _lineupService;
    private readonly IMapper _mapper;

    public MatchLineupController(IMatchLineupService lineupService, IMapper mapper)
    {
        _lineupService = lineupService;
        _mapper = mapper;
    }

    // POST /api/match/{matchId}/lineup
    [HttpPost]
    public async Task<ActionResult<MatchLineupDTO>> AddToLineup(int matchId, [FromBody] CreateMatchLineupDTO dto)
    {
        try
        {
            var lineup = await _lineupService.AddPlayerToLineupAsync(
                matchId, dto.PlayerId, dto.IsStarter, dto.Position);
            var result = _mapper.Map<MatchLineupDTO>(lineup);
            return CreatedAtAction(nameof(GetLineup), new { matchId }, result);
        }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
    }

    // GET /api/match/{matchId}/lineup
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MatchLineupDTO>>> GetLineup(int matchId)
    {
        try
        {
            var lineups = await _lineupService.GetLineupByMatchAsync(matchId);
            return Ok(_mapper.Map<IEnumerable<MatchLineupDTO>>(lineups));
        }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
    }

    // GET /api/match/{matchId}/lineup/team/{teamId}
    [HttpGet("team/{teamId}")]
    public async Task<ActionResult<IEnumerable<MatchLineupDTO>>> GetLineupByTeam(int matchId, int teamId)
    {
        try
        {
            var lineups = await _lineupService.GetLineupByMatchAndTeamAsync(matchId, teamId);
            return Ok(_mapper.Map<IEnumerable<MatchLineupDTO>>(lineups));
        }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
    }

    // DELETE /api/match/{matchId}/lineup/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> RemoveFromLineup(int matchId, int id)
    {
        try
        {
            await _lineupService.DeleteLineupEntryAsync(matchId, id);
            return NoContent();
        }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
    }
}