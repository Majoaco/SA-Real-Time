using FluentAssertions;
using NSubstitute;
using SpeechAnalytics.Application.DTOs;
using SpeechAnalytics.Application.Interfaces;
using SpeechAnalytics.Application.Services;
using SpeechAnalytics.Domain.Entities;
using SpeechAnalytics.Domain.Enums;

namespace SpeechAnalytics.Application.Tests.Services;

public class LiveCallOrchestratorTests
{
    private readonly ICallSessionRepository _sessionRepo;
    private readonly ILlmAnalysisService _llmService;
    private readonly IRealTimeNotifier _notifier;
    private readonly LiveCallOrchestrator _sut;

    public LiveCallOrchestratorTests()
    {
        _sessionRepo = Substitute.For<ICallSessionRepository>();
        _llmService = Substitute.For<ILlmAnalysisService>();
        _notifier = Substitute.For<IRealTimeNotifier>();
        _sut = new LiveCallOrchestrator(_sessionRepo, _llmService, _notifier);
    }

    [Fact]
    public async Task StartCallAsync_ShouldCreateSessionAndReturnId()
    {
        var request = new StartCallRequest("agent-1", CallType.Outbound);

        var response = await _sut.StartCallAsync(request);

        response.SessionId.Should().NotBeEmpty();
        response.Status.Should().Be("InProgress");
        await _sessionRepo.Received(1).SaveAsync(Arg.Any<LiveCallSession>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task StartCallAsync_ShouldNotifySessionStatus()
    {
        var request = new StartCallRequest("agent-1", CallType.Outbound);

        var response = await _sut.StartCallAsync(request);

        await _notifier.Received(1).SendSessionStatusAsync(
            response.SessionId.ToString(),
            "InProgress",
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessTranscriptChunkAsync_ShouldAppendToSession()
    {
        var session = LiveCallSession.Create("agent-1", CallType.Outbound);
        session.Start();
        _sessionRepo.GetByIdAsync(session.Id, Arg.Any<CancellationToken>()).Returns(session);

        await _sut.ProcessTranscriptChunkAsync(session.Id, "Empleado", "Hola buenas tardes", 1.5);

        session.TranscriptSegments.Should().HaveCount(1);
        await _notifier.Received(1).SendTranscriptUpdateAsync(
            session.Id.ToString(),
            Arg.Any<TranscriptUpdateDto>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessTranscriptChunkAsync_WithInvalidSession_ShouldThrow()
    {
        _sessionRepo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((LiveCallSession?)null);

        var act = () => _sut.ProcessTranscriptChunkAsync(Guid.NewGuid(), "Empleado", "Hola", 0);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task RequestSuggestionsAsync_ShouldCallLlmAndNotify()
    {
        var session = LiveCallSession.Create("agent-1", CallType.Outbound);
        session.Start();
        session.AppendTranscript("Empleado", "Hola buenas tardes", TimeSpan.FromSeconds(1));
        _sessionRepo.GetByIdAsync(session.Id, Arg.Any<CancellationToken>()).Returns(session);

        _llmService.GetRealTimeSuggestionsAsync(
            Arg.Any<string>(),
            Arg.Any<CallType>(),
            Arg.Any<IReadOnlyCollection<string>>(),
            Arg.Any<CancellationToken>())
            .Returns(new RealTimeSuggestionResult(
                new List<string> { "Presentate con nombre y apellido" },
                new List<string> { "apertura" },
                "apertura"));

        await _sut.RequestSuggestionsAsync(session.Id);

        await _notifier.Received(1).SendSuggestionAsync(
            session.Id.ToString(),
            Arg.Any<SuggestionDto>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RequestSuggestionsAsync_ShouldUpdateChecklist()
    {
        var session = LiveCallSession.Create("agent-1", CallType.Outbound);
        session.Start();
        session.AppendTranscript("Empleado", "Buenos dias le habla Juan Perez de BBVA", TimeSpan.FromSeconds(1));
        _sessionRepo.GetByIdAsync(session.Id, Arg.Any<CancellationToken>()).Returns(session);

        _llmService.GetRealTimeSuggestionsAsync(
            Arg.Any<string>(),
            Arg.Any<CallType>(),
            Arg.Any<IReadOnlyCollection<string>>(),
            Arg.Any<CancellationToken>())
            .Returns(new RealTimeSuggestionResult(
                new List<string>(),
                new List<string> { "apertura" },
                "discurso"));

        await _sut.RequestSuggestionsAsync(session.Id);

        session.CompletedChecklistSteps.Should().Contain("apertura");
        await _notifier.Received(1).SendChecklistUpdateAsync(
            session.Id.ToString(),
            Arg.Any<ChecklistUpdateDto>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RequestTemperatureAsync_ShouldAnalyzeAndNotify()
    {
        var session = LiveCallSession.Create("agent-1", CallType.Outbound);
        session.Start();
        session.AppendTranscript("Empleado", "Buenos dias", TimeSpan.FromSeconds(1));
        _sessionRepo.GetByIdAsync(session.Id, Arg.Any<CancellationToken>()).Returns(session);

        _llmService.AnalyzeTemperatureAsync(
            Arg.Any<string>(),
            Arg.Any<CancellationToken>())
            .Returns(new TemperatureAnalysisResult(70, 60, 15));

        await _sut.RequestTemperatureAsync(session.Id);

        session.CurrentTemperature.Should().NotBeNull();
        session.CurrentTemperature!.Emotional.Should().Be(70);
        await _notifier.Received(1).SendTemperatureUpdateAsync(
            session.Id.ToString(),
            Arg.Any<TemperatureUpdateDto>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EndCallAsync_ShouldCompleteSessionAndNotify()
    {
        var session = LiveCallSession.Create("agent-1", CallType.Outbound);
        session.Start();
        _sessionRepo.GetByIdAsync(session.Id, Arg.Any<CancellationToken>()).Returns(session);

        await _sut.EndCallAsync(session.Id);

        session.Status.Should().Be(CallStatus.Completed);
        await _sessionRepo.Received(1).UpdateAsync(session, Arg.Any<CancellationToken>());
        await _notifier.Received(1).SendSessionStatusAsync(
            session.Id.ToString(),
            "Completed",
            Arg.Any<CancellationToken>());
    }
}
