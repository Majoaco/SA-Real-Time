using SpeechAnalytics.Domain.Entities;

namespace SpeechAnalytics.Application.Interfaces;

public interface ICallSessionRepository
{
    Task<LiveCallSession?> GetByIdAsync(Guid sessionId, CancellationToken cancellationToken = default);
    Task SaveAsync(LiveCallSession session, CancellationToken cancellationToken = default);
    Task UpdateAsync(LiveCallSession session, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LiveCallSession>> GetActiveSessionsAsync(CancellationToken cancellationToken = default);
}
