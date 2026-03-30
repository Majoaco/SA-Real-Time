using SpeechAnalytics.Domain.Enums;
using SpeechAnalytics.Domain.ValueObjects;

namespace SpeechAnalytics.Domain.Entities;

public sealed class LiveCallSession
{
    public Guid Id { get; }
    public string AgentId { get; }
    public CallType CallType { get; }
    public CallStatus Status { get; private set; }
    public DateTime CreatedAt { get; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    private readonly List<TranscriptSegment> _transcriptSegments = new();
    private readonly List<Suggestion> _suggestions = new();
    private readonly List<TemperatureReading> _temperatureHistory = new();
    private readonly HashSet<string> _completedChecklistSteps = new();

    public IReadOnlyList<TranscriptSegment> TranscriptSegments => _transcriptSegments.AsReadOnly();
    public IReadOnlyList<Suggestion> Suggestions => _suggestions.AsReadOnly();
    public IReadOnlyList<TemperatureReading> TemperatureHistory => _temperatureHistory.AsReadOnly();
    public IReadOnlyCollection<string> CompletedChecklistSteps => _completedChecklistSteps;

    public TemperatureReading? CurrentTemperature => _temperatureHistory.LastOrDefault();

    public TimeSpan Duration
    {
        get
        {
            if (StartedAt is null) return TimeSpan.Zero;
            var end = CompletedAt ?? DateTime.UtcNow;
            return end - StartedAt.Value;
        }
    }

    private LiveCallSession(string agentId, CallType callType)
    {
        Id = Guid.NewGuid();
        AgentId = agentId;
        CallType = callType;
        Status = CallStatus.Idle;
        CreatedAt = DateTime.UtcNow;
    }

    public static LiveCallSession Create(string agentId, CallType callType)
    {
        if (string.IsNullOrWhiteSpace(agentId))
            throw new ArgumentException("Agent ID cannot be empty.", nameof(agentId));

        return new LiveCallSession(agentId, callType);
    }

    public void Start()
    {
        if (Status != CallStatus.Idle)
            throw new InvalidOperationException($"Cannot start a session in {Status} status.");

        Status = CallStatus.InProgress;
        StartedAt = DateTime.UtcNow;
    }

    public void AppendTranscript(string speaker, string text, TimeSpan timestamp)
    {
        if (Status != CallStatus.InProgress)
            throw new InvalidOperationException("Cannot append transcript to a session that is not in progress.");

        var segment = TranscriptSegment.Create(speaker, text, timestamp);
        _transcriptSegments.Add(segment);
    }

    public string GetFullTranscript()
    {
        return string.Join("\n", _transcriptSegments.Select(s => s.ToString()));
    }

    public void AddSuggestion(string text, string category = "general", SuggestionPriority priority = SuggestionPriority.Normal)
    {
        var suggestion = Suggestion.Create(text, category, priority);
        _suggestions.Add(suggestion);
    }

    public void UpdateTemperature(TemperatureReading reading)
    {
        _temperatureHistory.Add(reading);
    }

    public void MarkChecklistStep(string stepId)
    {
        _completedChecklistSteps.Add(stepId);
    }

    public void Complete()
    {
        if (Status != CallStatus.InProgress)
            throw new InvalidOperationException($"Cannot complete a session in {Status} status.");

        Status = CallStatus.Completed;
        CompletedAt = DateTime.UtcNow;
    }

    public void MarkError()
    {
        Status = CallStatus.Error;
        CompletedAt = DateTime.UtcNow;
    }
}
