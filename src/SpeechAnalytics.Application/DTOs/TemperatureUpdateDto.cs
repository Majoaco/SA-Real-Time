namespace SpeechAnalytics.Application.DTOs;

public record TemperatureUpdateDto(
    int Emotional,
    int Sales,
    int Conflict,
    string EmotionalLabel,
    string SalesLabel,
    string ConflictLabel,
    bool RequiresAttention);
