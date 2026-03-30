namespace SpeechAnalytics.Domain.ValueObjects;

public sealed record Suggestion
{
    public string Text { get; }
    public string Category { get; }
    public SuggestionPriority Priority { get; }
    public DateTime CreatedAt { get; }

    private Suggestion(string text, string category, SuggestionPriority priority)
    {
        Text = text;
        Category = category;
        Priority = priority;
        CreatedAt = DateTime.UtcNow;
    }

    public static Suggestion Create(string text, string category = "general", SuggestionPriority priority = SuggestionPriority.Normal)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Suggestion text cannot be empty.", nameof(text));

        return new Suggestion(text, category, priority);
    }
}

public enum SuggestionPriority
{
    Low,
    Normal,
    High,
    Critical
}
