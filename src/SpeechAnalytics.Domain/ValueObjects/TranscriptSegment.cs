namespace SpeechAnalytics.Domain.ValueObjects;

public sealed record TranscriptSegment
{
    public string Speaker { get; }
    public string Text { get; }
    public TimeSpan Timestamp { get; }
    public DateTime CreatedAt { get; }

    private TranscriptSegment(string speaker, string text, TimeSpan timestamp)
    {
        Speaker = speaker;
        Text = text;
        Timestamp = timestamp;
        CreatedAt = DateTime.UtcNow;
    }

    public static TranscriptSegment Create(string speaker, string text, TimeSpan timestamp)
    {
        if (string.IsNullOrWhiteSpace(speaker))
            throw new ArgumentException("Speaker cannot be empty.", nameof(speaker));
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Text cannot be empty.", nameof(text));

        return new TranscriptSegment(speaker, text, timestamp);
    }

    public override string ToString() => $"{Speaker}: {Text}";
}
