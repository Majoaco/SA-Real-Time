using SpeechAnalytics.Application.DTOs;
using SpeechAnalytics.Application.Interfaces;
using SpeechAnalytics.Domain.Entities;
using SpeechAnalytics.Domain.Enums;
using SpeechAnalytics.Domain.ValueObjects;

namespace SpeechAnalytics.Application.Services;

public sealed class LiveCallOrchestrator
{
    private readonly ICallSessionRepository _sessionRepo;
    private readonly ILlmAnalysisService _llmService;
    private readonly IRealTimeNotifier _notifier;

    public LiveCallOrchestrator(
        ICallSessionRepository sessionRepo,
        ILlmAnalysisService llmService,
        IRealTimeNotifier notifier)
    {
        _sessionRepo = sessionRepo;
        _llmService = llmService;
        _notifier = notifier;
    }

    public async Task<StartCallResponse> StartCallAsync(StartCallRequest request, CancellationToken ct = default)
    {
        var session = LiveCallSession.Create(request.AgentId, request.CallType);
        session.Start();

        await _sessionRepo.SaveAsync(session, ct);
        await _notifier.SendSessionStatusAsync(session.Id.ToString(), "InProgress", ct);

        return new StartCallResponse(session.Id, "InProgress");
    }

    public async Task ProcessTranscriptChunkAsync(Guid sessionId, string speaker, string text, double timestampSeconds, CancellationToken ct = default)
    {
        var session = await GetSessionOrThrow(sessionId, ct);

        session.AppendTranscript(speaker, text, TimeSpan.FromSeconds(timestampSeconds));
        await _sessionRepo.UpdateAsync(session, ct);

        var update = new TranscriptUpdateDto(speaker, text, timestampSeconds, true);
        await _notifier.SendTranscriptUpdateAsync(sessionId.ToString(), update, ct);
    }

    public async Task RequestSuggestionsAsync(Guid sessionId, CancellationToken ct = default)
    {
        var session = await GetSessionOrThrow(sessionId, ct);
        var transcript = session.GetFullTranscript();

        var result = await _llmService.GetRealTimeSuggestionsAsync(
            transcript, session.CallType, session.CompletedChecklistSteps, ct);

        foreach (var step in result.CompletedSteps)
            session.MarkChecklistStep(step);

        foreach (var suggestionText in result.Suggestions)
        {
            session.AddSuggestion(suggestionText);
            var dto = new SuggestionDto(suggestionText, "general", "Normal");
            await _notifier.SendSuggestionAsync(sessionId.ToString(), dto, ct);
        }

        var checklistDto = new ChecklistUpdateDto(
            result.CompletedSteps,
            result.CurrentPhase);
        await _notifier.SendChecklistUpdateAsync(sessionId.ToString(), checklistDto, ct);

        await _sessionRepo.UpdateAsync(session, ct);
    }

    public async Task RequestTemperatureAsync(Guid sessionId, CancellationToken ct = default)
    {
        var session = await GetSessionOrThrow(sessionId, ct);
        var transcript = session.GetFullTranscript();

        var result = await _llmService.AnalyzeTemperatureAsync(transcript, ct);

        var reading = TemperatureReading.Create(result.Emotional, result.Sales, result.Conflict);
        session.UpdateTemperature(reading);

        var dto = new TemperatureUpdateDto(
            reading.Emotional, reading.Sales, reading.Conflict,
            reading.EmotionalLabel, reading.SalesLabel, reading.ConflictLabel,
            reading.RequiresAttention);
        await _notifier.SendTemperatureUpdateAsync(sessionId.ToString(), dto, ct);

        await _sessionRepo.UpdateAsync(session, ct);
    }

    public async Task EndCallAsync(Guid sessionId, CancellationToken ct = default)
    {
        var session = await GetSessionOrThrow(sessionId, ct);

        session.Complete();
        await _sessionRepo.UpdateAsync(session, ct);
        await _notifier.SendSessionStatusAsync(sessionId.ToString(), "Completed", ct);
    }

    private async Task<LiveCallSession> GetSessionOrThrow(Guid sessionId, CancellationToken ct)
    {
        var session = await _sessionRepo.GetByIdAsync(sessionId, ct);
        if (session is null)
            throw new InvalidOperationException($"Session {sessionId} not found.");
        return session;
    }
}
