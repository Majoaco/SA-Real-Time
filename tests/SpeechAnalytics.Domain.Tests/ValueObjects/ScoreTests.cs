using FluentAssertions;
using SpeechAnalytics.Domain.ValueObjects;

namespace SpeechAnalytics.Domain.Tests.ValueObjects;

public class ScoreTests
{
    [Fact]
    public void Create_WithValidValues_ShouldSucceed()
    {
        var score = Score.Create(80, 100);

        score.Value.Should().Be(80);
        score.MaxValue.Should().Be(100);
    }

    [Fact]
    public void Create_WithZeroValue_ShouldSucceed()
    {
        var score = Score.Create(0, 100);

        score.Value.Should().Be(0);
    }

    [Fact]
    public void Create_WithNegativeValue_ShouldThrow()
    {
        var act = () => Score.Create(-1, 100);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithValueExceedingMax_ShouldThrow()
    {
        var act = () => Score.Create(101, 100);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithZeroMax_ShouldThrow()
    {
        var act = () => Score.Create(0, 0);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Percentage_ShouldReturnCorrectValue()
    {
        var score = Score.Create(75, 100);

        score.Percentage.Should().Be(75);
    }

    [Fact]
    public void Percentage_WithNonStandardMax_ShouldCalculateCorrectly()
    {
        var score = Score.Create(15, 20);

        score.Percentage.Should().Be(75);
    }

    [Fact]
    public void Two_Scores_WithSameValues_ShouldBeEqual()
    {
        var score1 = Score.Create(80, 100);
        var score2 = Score.Create(80, 100);

        score1.Should().Be(score2);
    }

    [Fact]
    public void Two_Scores_WithDifferentValues_ShouldNotBeEqual()
    {
        var score1 = Score.Create(80, 100);
        var score2 = Score.Create(70, 100);

        score1.Should().NotBe(score2);
    }

    [Fact]
    public void Zero_ShouldReturnScoreWithZeroValue()
    {
        var score = Score.Zero(100);

        score.Value.Should().Be(0);
        score.MaxValue.Should().Be(100);
    }

    [Fact]
    public void Perfect_ShouldReturnMaxScore()
    {
        var score = Score.Perfect(100);

        score.Value.Should().Be(100);
        score.MaxValue.Should().Be(100);
    }
}
