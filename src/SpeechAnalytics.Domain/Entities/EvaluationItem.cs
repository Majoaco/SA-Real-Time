using SpeechAnalytics.Domain.Enums;
using SpeechAnalytics.Domain.ValueObjects;

namespace SpeechAnalytics.Domain.Entities;

public sealed class EvaluationItem
{
    public string Id { get; }
    public string Name { get; }
    public Score Score { get; private set; }
    public EvaluationResult Result { get; private set; }
    public string Observation { get; private set; }

    private EvaluationItem(string id, string name, double maxScore)
    {
        Id = id;
        Name = name;
        Score = Score.Zero(maxScore);
        Result = EvaluationResult.No;
        Observation = string.Empty;
    }

    public static EvaluationItem Create(string id, string name, double maxScore)
    {
        return new EvaluationItem(id, name, maxScore);
    }

    public void Evaluate(EvaluationResult result, string observation)
    {
        Result = result;
        Observation = observation;

        Score = result switch
        {
            EvaluationResult.Yes => Score.Perfect(Score.MaxValue),
            EvaluationResult.Exempt => Score.Perfect(Score.MaxValue),
            EvaluationResult.No => Score.Zero(Score.MaxValue),
            _ => Score.Zero(Score.MaxValue)
        };
    }
}
