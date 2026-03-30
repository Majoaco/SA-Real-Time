namespace SpeechAnalytics.Application.DTOs;

public record SuggestionDto(
    string Text,
    string Category,
    string Priority);
