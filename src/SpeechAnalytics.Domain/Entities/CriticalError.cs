namespace SpeechAnalytics.Domain.Entities;

public sealed class CriticalError
{
    public string Id { get; }
    public string Name { get; }
    public string Description { get; }
    public bool Detected { get; private set; }
    public string Observation { get; private set; }

    private CriticalError(string id, string name, string description)
    {
        Id = id;
        Name = name;
        Description = description;
        Detected = false;
        Observation = string.Empty;
    }

    public static CriticalError Create(string id, string name, string description)
    {
        return new CriticalError(id, name, description);
    }

    public void MarkDetected(string observation)
    {
        Detected = true;
        Observation = observation;
    }
}
