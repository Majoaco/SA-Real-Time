using FluentAssertions;
using SpeechAnalytics.Domain.Entities;
using SpeechAnalytics.Domain.Enums;
using SpeechAnalytics.Domain.ValueObjects;

namespace SpeechAnalytics.Domain.Tests.Entities;

public class LiveCallSessionTests
{
    [Fact]
    public void Create_ShouldInitializeWithIdleStatus()
    {
        var session = LiveCallSession.Create("agent-1", CallType.Outbound);

        session.AgentId.Should().Be("agent-1");
        session.CallType.Should().Be(CallType.Outbound);
        session.Status.Should().Be(CallStatus.Idle);
        session.TranscriptSegments.Should().BeEmpty();
        session.Suggestions.Should().BeEmpty();
    }

    [Fact]
    public void Start_ShouldChangeStatusToInProgress()
    {
        var session = LiveCallSession.Create("agent-1", CallType.Outbound);

        session.Start();

        session.Status.Should().Be(CallStatus.InProgress);
        session.StartedAt.Should().NotBeNull();
    }

    [Fact]
    public void Start_WhenAlreadyInProgress_ShouldThrow()
    {
        var session = LiveCallSession.Create("agent-1", CallType.Outbound);
        session.Start();

        var act = () => session.Start();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void AppendTranscript_ShouldAddSegment()
    {
        var session = LiveCallSession.Create("agent-1", CallType.Outbound);
        session.Start();

        session.AppendTranscript("Empleado", "Buenos dias, le habla Juan de BBVA.", TimeSpan.FromSeconds(3));

        session.TranscriptSegments.Should().HaveCount(1);
        session.TranscriptSegments[0].Speaker.Should().Be("Empleado");
        session.TranscriptSegments[0].Text.Should().Be("Buenos dias, le habla Juan de BBVA.");
    }

    [Fact]
    public void AppendTranscript_WhenNotStarted_ShouldThrow()
    {
        var session = LiveCallSession.Create("agent-1", CallType.Outbound);

        var act = () => session.AppendTranscript("Empleado", "Hola", TimeSpan.Zero);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void GetFullTranscript_ShouldConcatenateAllSegments()
    {
        var session = LiveCallSession.Create("agent-1", CallType.Outbound);
        session.Start();
        session.AppendTranscript("Empleado", "Buenos dias.", TimeSpan.FromSeconds(1));
        session.AppendTranscript("Cliente", "Buenos dias.", TimeSpan.FromSeconds(3));

        var transcript = session.GetFullTranscript();

        transcript.Should().Contain("Empleado: Buenos dias.");
        transcript.Should().Contain("Cliente: Buenos dias.");
    }

    [Fact]
    public void AddSuggestion_ShouldStoreIt()
    {
        var session = LiveCallSession.Create("agent-1", CallType.Outbound);
        session.Start();

        session.AddSuggestion("Recorda presentarte con nombre y apellido.");

        session.Suggestions.Should().HaveCount(1);
        session.Suggestions[0].Text.Should().Be("Recorda presentarte con nombre y apellido.");
    }

    [Fact]
    public void UpdateTemperature_ShouldSetLatestReading()
    {
        var session = LiveCallSession.Create("agent-1", CallType.Outbound);
        session.Start();
        var reading = TemperatureReading.Create(70, 60, 15);

        session.UpdateTemperature(reading);

        session.CurrentTemperature.Should().Be(reading);
        session.TemperatureHistory.Should().HaveCount(1);
    }

    [Fact]
    public void Complete_ShouldChangeStatusAndSetEndTime()
    {
        var session = LiveCallSession.Create("agent-1", CallType.Outbound);
        session.Start();

        session.Complete();

        session.Status.Should().Be(CallStatus.Completed);
        session.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public void Complete_WhenNotInProgress_ShouldThrow()
    {
        var session = LiveCallSession.Create("agent-1", CallType.Outbound);

        var act = () => session.Complete();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Duration_WhenInProgress_ShouldReturnElapsedTime()
    {
        var session = LiveCallSession.Create("agent-1", CallType.Outbound);
        session.Start();

        session.Duration.Should().BeGreaterThanOrEqualTo(TimeSpan.Zero);
    }

    [Fact]
    public void ChecklistProgress_ShouldTrackCompletedSteps()
    {
        var session = LiveCallSession.Create("agent-1", CallType.Outbound);
        session.Start();

        session.MarkChecklistStep("apertura");

        session.CompletedChecklistSteps.Should().Contain("apertura");
    }

    [Fact]
    public void MarkChecklistStep_Duplicate_ShouldNotAddTwice()
    {
        var session = LiveCallSession.Create("agent-1", CallType.Outbound);
        session.Start();

        session.MarkChecklistStep("apertura");
        session.MarkChecklistStep("apertura");

        session.CompletedChecklistSteps.Should().HaveCount(1);
    }
}
