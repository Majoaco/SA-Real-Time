using SpeechAnalytics.Application.DTOs;

namespace SpeechAnalytics.Application.Interfaces;

public interface IRealTimeNotifier
{
    Task SendTranscriptUpdateAsync(string sessionId, TranscriptUpdateDto update, CancellationToken cancellationToken = default);
    Task SendSuggestionAsync(string sessionId, SuggestionDto suggestion, CancellationToken cancellationToken = default);
    Task SendTemperatureUpdateAsync(string sessionId, TemperatureUpdateDto update, CancellationToken cancellationToken = default);
    Task SendChecklistUpdateAsync(string sessionId, ChecklistUpdateDto update, CancellationToken cancellationToken = default);
    Task SendSessionStatusAsync(string sessionId, string status, CancellationToken cancellationToken = default);
}
