using System.Collections.Concurrent;
using SpeechAnalytics.Application.Interfaces;
using SpeechAnalytics.Domain.Entities;

namespace SpeechAnalytics.Infrastructure.Persistence;

public sealed class InMemoryCallSessionRepository : ICallSessionRepository
{
    private readonly ConcurrentDictionary<Guid, LiveCallSession> _sessions = new();

    public Task<LiveCallSession?> GetByIdAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        _sessions.TryGetValue(sessionId, out var session);
        return Task.FromResult(session);
    }

    public Task SaveAsync(LiveCallSession session, CancellationToken cancellationToken = default)
    {
        _sessions[session.Id] = session;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(LiveCallSession session, CancellationToken cancellationToken = default)
    {
        _sessions[session.Id] = session;
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<LiveCallSession>> GetActiveSessionsAsync(CancellationToken cancellationToken = default)
    {
        var active = _sessions.Values
            .Where(s => s.Status == Domain.Enums.CallStatus.InProgress)
            .ToList();
        return Task.FromResult<IReadOnlyList<LiveCallSession>>(active);
    }
}
