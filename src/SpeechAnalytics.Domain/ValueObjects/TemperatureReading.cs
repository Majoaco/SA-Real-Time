namespace SpeechAnalytics.Domain.ValueObjects;

public sealed record TemperatureReading
{
    public int Emotional { get; }
    public int Sales { get; }
    public int Conflict { get; }
    public DateTime Timestamp { get; }

    public string EmotionalLabel => GetEmotionalLabel(Emotional);
    public string SalesLabel => GetSalesLabel(Sales);
    public string ConflictLabel => GetConflictLabel(Conflict);
    public bool RequiresAttention => Conflict > 60;

    private TemperatureReading(int emotional, int sales, int conflict)
    {
        Emotional = emotional;
        Sales = sales;
        Conflict = conflict;
        Timestamp = DateTime.UtcNow;
    }

    public static TemperatureReading Create(int emotional, int sales, int conflict)
    {
        ValidateRange(emotional, nameof(emotional));
        ValidateRange(sales, nameof(sales));
        ValidateRange(conflict, nameof(conflict));

        return new TemperatureReading(emotional, sales, conflict);
    }

    public static TemperatureReading Neutral() => new(50, 50, 0);

    private static void ValidateRange(int value, string paramName)
    {
        if (value < 0 || value > 100)
            throw new ArgumentException($"Value must be between 0 and 100.", paramName);
    }

    private static string GetEmotionalLabel(int value) => value switch
    {
        <= 20 => "Muy negativo",
        <= 40 => "Negativo",
        <= 60 => "Neutro",
        <= 80 => "Positivo",
        _ => "Muy positivo"
    };

    private static string GetSalesLabel(int value) => value switch
    {
        <= 20 => "Muy frio",
        <= 40 => "Frio",
        <= 60 => "Tibio",
        <= 80 => "Caliente",
        _ => "Muy caliente"
    };

    private static string GetConflictLabel(int value) => value switch
    {
        <= 20 => "Sin conflicto",
        <= 40 => "Tension leve",
        <= 60 => "Tension moderada",
        <= 80 => "Conflicto",
        _ => "Conflicto severo"
    };
}
