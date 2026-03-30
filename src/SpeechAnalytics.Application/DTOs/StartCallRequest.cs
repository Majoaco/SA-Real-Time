using SpeechAnalytics.Domain.Enums;

namespace SpeechAnalytics.Application.DTOs;

public record StartCallRequest(
    string AgentId,
    CallType CallType);

public record StartCallResponse(
    Guid SessionId,
    string Status);
