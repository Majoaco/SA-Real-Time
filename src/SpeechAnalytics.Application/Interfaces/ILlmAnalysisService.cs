using SpeechAnalytics.Domain.Enums;

namespace SpeechAnalytics.Application.Interfaces;

public interface ILlmAnalysisService
{
    Task<RealTimeSuggestionResult> GetRealTimeSuggestionsAsync(
        string transcriptSoFar,
        CallType callType,
        IReadOnlyCollection<string> completedSteps,
        CancellationToken cancellationToken = default);

    Task<TemperatureAnalysisResult> AnalyzeTemperatureAsync(
        string transcriptSoFar,
        CancellationToken cancellationToken = default);
}

public record RealTimeSuggestionResult(
    List<string> Suggestions,
    List<string> CompletedSteps,
    string? CurrentPhase);

public record TemperatureAnalysisResult(
    int Emotional,
    int Sales,
    int Conflict);
