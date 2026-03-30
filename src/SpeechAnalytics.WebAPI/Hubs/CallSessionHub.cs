using Microsoft.AspNetCore.SignalR;
using SpeechAnalytics.Application.DTOs;
using SpeechAnalytics.Application.Services;
using SpeechAnalytics.Domain.Enums;

namespace SpeechAnalytics.WebAPI.Hubs;

public sealed class CallSessionHub : Hub
{
    private readonly LiveCallOrchestrator _orchestrator;

    public CallSessionHub(LiveCallOrchestrator orchestrator)
    {
        _orchestrator = orchestrator;
    }

    public async Task StartCall(string agentId, string callType)
    {
        var type = Enum.Parse<CallType>(callType, ignoreCase: true);
        var response = await _orchestrator.StartCallAsync(new StartCallRequest(agentId, type));

        await Groups.AddToGroupAsync(Context.ConnectionId, response.SessionId.ToString());
        await Clients.Caller.SendAsync("CallStarted", response);
    }

    public async Task SendTranscriptChunk(string sessionId, string speaker, string text, double timestampSeconds)
    {
        var id = Guid.Parse(sessionId);
        await _orchestrator.ProcessTranscriptChunkAsync(id, speaker, text, timestampSeconds);
    }

    public async Task RequestSuggestions(string sessionId)
    {
        var id = Guid.Parse(sessionId);
        await _orchestrator.RequestSuggestionsAsync(id);
    }

    public async Task RequestTemperature(string sessionId)
    {
        var id = Guid.Parse(sessionId);
        await _orchestrator.RequestTemperatureAsync(id);
    }

    public async Task EndCall(string sessionId)
    {
        var id = Guid.Parse(sessionId);
        await _orchestrator.EndCallAsync(id);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, sessionId);
    }

    public async Task JoinSession(string sessionId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, sessionId);
    }
}
