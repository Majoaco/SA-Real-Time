using FluentAssertions;
using SpeechAnalytics.Domain.ValueObjects;

namespace SpeechAnalytics.Domain.Tests.ValueObjects;

public class TemperatureReadingTests
{
    [Fact]
    public void Create_WithValidValues_ShouldSucceed()
    {
        var reading = TemperatureReading.Create(70, 60, 20);

        reading.Emotional.Should().Be(70);
        reading.Sales.Should().Be(60);
        reading.Conflict.Should().Be(20);
    }

    [Fact]
    public void Create_WithValueBelowZero_ShouldThrow()
    {
        var act = () => TemperatureReading.Create(-1, 50, 50);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithValueAbove100_ShouldThrow()
    {
        var act = () => TemperatureReading.Create(101, 50, 50);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void EmotionalLabel_ShouldReturnCorrectLabel()
    {
        var veryNegative = TemperatureReading.Create(10, 50, 50);
        var negative = TemperatureReading.Create(30, 50, 50);
        var neutral = TemperatureReading.Create(50, 50, 50);
        var positive = TemperatureReading.Create(70, 50, 50);
        var veryPositive = TemperatureReading.Create(90, 50, 50);

        veryNegative.EmotionalLabel.Should().Be("Muy negativo");
        negative.EmotionalLabel.Should().Be("Negativo");
        neutral.EmotionalLabel.Should().Be("Neutro");
        positive.EmotionalLabel.Should().Be("Positivo");
        veryPositive.EmotionalLabel.Should().Be("Muy positivo");
    }

    [Fact]
    public void SalesLabel_ShouldReturnCorrectLabel()
    {
        var veryCold = TemperatureReading.Create(50, 10, 50);
        var hot = TemperatureReading.Create(50, 75, 50);

        veryCold.SalesLabel.Should().Be("Muy frio");
        hot.SalesLabel.Should().Be("Caliente");
    }

    [Fact]
    public void RequiresAttention_ShouldBeTrueWhenConflictIsHigh()
    {
        var reading = TemperatureReading.Create(50, 50, 70);

        reading.RequiresAttention.Should().BeTrue();
    }

    [Fact]
    public void RequiresAttention_ShouldBeFalseWhenConflictIsLow()
    {
        var reading = TemperatureReading.Create(50, 50, 30);

        reading.RequiresAttention.Should().BeFalse();
    }

    [Fact]
    public void Neutral_ShouldReturn50ForAll()
    {
        var reading = TemperatureReading.Neutral();

        reading.Emotional.Should().Be(50);
        reading.Sales.Should().Be(50);
        reading.Conflict.Should().Be(0);
    }
}
