namespace SpeechAnalytics.Application.DTOs;

public record TranscriptUpdateDto(
    string Speaker,
    string Text,
    double TimestampSeconds,
    bool IsFinal);
