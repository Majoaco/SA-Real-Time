using SpeechAnalytics.Domain.ValueObjects;

namespace SpeechAnalytics.Domain.Entities;

public sealed class EvaluationSection
{
    public string Id { get; }
    public string Name { get; }
    public double MaxScore { get; }
    public bool IsSla { get; }

    private readonly List<EvaluationItem> _items = new();
    public IReadOnlyList<EvaluationItem> Items => _items.AsReadOnly();

    public Score TotalScore => Score.Create(
        _items.Sum(i => i.Score.Value),
        MaxScore
    );

    private EvaluationSection(string id, string name, double maxScore, bool isSla)
    {
        Id = id;
        Name = name;
        MaxScore = maxScore;
        IsSla = isSla;
    }

    public static EvaluationSection Create(string id, string name, double maxScore, bool isSla = false)
    {
        return new EvaluationSection(id, name, maxScore, isSla);
    }

    public void AddItem(EvaluationItem item)
    {
        _items.Add(item);
    }
}
