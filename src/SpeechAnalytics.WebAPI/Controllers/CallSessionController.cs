using Microsoft.AspNetCore.Mvc;
using SpeechAnalytics.Application.DTOs;
using SpeechAnalytics.Application.Interfaces;
using SpeechAnalytics.Application.Services;

namespace SpeechAnalytics.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CallSessionController : ControllerBase
{
    private readonly LiveCallOrchestrator _orchestrator;
    private readonly ICallSessionRepository _sessionRepo;

    public CallSessionController(LiveCallOrchestrator orchestrator, ICallSessionRepository sessionRepo)
    {
        _orchestrator = orchestrator;
        _sessionRepo = sessionRepo;
    }

    [HttpPost("start")]
    public async Task<ActionResult<StartCallResponse>> StartCall([FromBody] StartCallRequest request)
    {
        var response = await _orchestrator.StartCallAsync(request);
        return Ok(response);
    }

    [HttpPost("{sessionId}/end")]
    public async Task<IActionResult> EndCall(Guid sessionId)
    {
        await _orchestrator.EndCallAsync(sessionId);
        return Ok();
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActiveSessions()
    {
        var sessions = await _sessionRepo.GetActiveSessionsAsync();
        var result = sessions.Select(s => new
        {
            s.Id,
            s.AgentId,
            CallType = s.CallType.ToString(),
            Status = s.Status.ToString(),
            s.StartedAt,
            Duration = s.Duration.TotalSeconds,
            SegmentCount = s.TranscriptSegments.Count,
            HasTemperature = s.CurrentTemperature is not null,
            ChecklistProgress = s.CompletedChecklistSteps.Count
        });
        return Ok(result);
    }

    [HttpGet("{sessionId}")]
    public async Task<IActionResult> GetSession(Guid sessionId)
    {
        var session = await _sessionRepo.GetByIdAsync(sessionId);
        if (session is null) return NotFound();

        return Ok(new
        {
            session.Id,
            session.AgentId,
            CallType = session.CallType.ToString(),
            Status = session.Status.ToString(),
            session.StartedAt,
            session.CompletedAt,
            Duration = session.Duration.TotalSeconds,
            Transcript = session.GetFullTranscript(),
            Suggestions = session.Suggestions.Select(s => new { s.Text, s.Category, Priority = s.Priority.ToString() }),
            Temperature = session.CurrentTemperature is not null ? new
            {
                session.CurrentTemperature.Emotional,
                session.CurrentTemperature.Sales,
                session.CurrentTemperature.Conflict,
                session.CurrentTemperature.EmotionalLabel,
                session.CurrentTemperature.SalesLabel,
                session.CurrentTemperature.ConflictLabel,
                session.CurrentTemperature.RequiresAttention
            } : null,
            CompletedSteps = session.CompletedChecklistSteps
        });
    }
}
