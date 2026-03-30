namespace SpeechAnalytics.Application.Interfaces;

public interface ITranscriptionService
{
    IAsyncEnumerable<TranscriptionChunk> TranscribeStreamAsync(
        Stream audioStream,
        string language = "es",
        CancellationToken cancellationToken = default);
}

public record TranscriptionChunk(
    string Text,
    string Speaker,
    TimeSpan Timestamp,
    bool IsFinal);
