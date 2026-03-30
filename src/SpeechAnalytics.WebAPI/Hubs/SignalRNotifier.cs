using Microsoft.AspNetCore.SignalR;
using SpeechAnalytics.Application.DTOs;
using SpeechAnalytics.Application.Interfaces;

namespace SpeechAnalytics.WebAPI.Hubs;

public sealed class SignalRNotifier : IRealTimeNotifier
{
    private readonly IHubContext<CallSessionHub> _hubContext;

    public SignalRNotifier(IHubContext<CallSessionHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task SendTranscriptUpdateAsync(string sessionId, TranscriptUpdateDto update, CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients.Group(sessionId).SendAsync("TranscriptUpdate", update, cancellationToken);
    }

    public async Task SendSuggestionAsync(string sessionId, SuggestionDto suggestion, CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients.Group(sessionId).SendAsync("NewSuggestion", suggestion, cancellationToken);
    }

    public async Task SendTemperatureUpdateAsync(string sessionId, TemperatureUpdateDto update, CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients.Group(sessionId).SendAsync("TemperatureUpdate", update, cancellationToken);
    }

    public async Task SendChecklistUpdateAsync(string sessionId, ChecklistUpdateDto update, CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients.Group(sessionId).SendAsync("ChecklistUpdate", update, cancellationToken);
    }

    public async Task SendSessionStatusAsync(string sessionId, string status, CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients.Group(sessionId).SendAsync("SessionStatus", status, cancellationToken);
    }
}
