namespace SpeechAnalytics.Application.DTOs;

public record ChecklistUpdateDto(
    List<string> CompletedSteps,
    string? CurrentPhase);
