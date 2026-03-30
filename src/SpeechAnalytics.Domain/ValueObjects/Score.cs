namespace SpeechAnalytics.Domain.ValueObjects;

public sealed record Score
{
    public double Value { get; }
    public double MaxValue { get; }
    public double Percentage => Value / MaxValue * 100;

    private Score(double value, double maxValue)
    {
        Value = value;
        MaxValue = maxValue;
    }

    public static Score Create(double value, double maxValue)
    {
        if (maxValue <= 0)
            throw new ArgumentException("Max value must be greater than zero.", nameof(maxValue));
        if (value < 0)
            throw new ArgumentException("Value cannot be negative.", nameof(value));
        if (value > maxValue)
            throw new ArgumentException("Value cannot exceed max value.", nameof(value));

        return new Score(value, maxValue);
    }

    public static Score Zero(double maxValue) => Create(0, maxValue);
    public static Score Perfect(double maxValue) => Create(maxValue, maxValue);
}
