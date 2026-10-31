using JavaMaster.Api.Entities;
using JavaMaster.Api.Services;

namespace JavaMaster.Api.Tests.Services;

public class RankCalculatorTests
{
    // The same ranges that DbSeeder writes on a fresh database.
    private static readonly Rank[] Ranks =
    {
        new() { Name = "Starter",  MinXp = 0,    MaxXp = 300,    IconFile = "starter.png" },
        new() { Name = "Beginner", MinXp = 300,  MaxXp = 700,    IconFile = "beginner.png" },
        new() { Name = "Rising",   MinXp = 700,  MaxXp = 1200,   IconFile = "rising.png" },
        new() { Name = "Skilled",  MinXp = 1200, MaxXp = 1800,   IconFile = "skilled.png" },
        new() { Name = "Pro",      MinXp = 1800, MaxXp = 3000,   IconFile = "pro.png" },
        new() { Name = "Elite",    MinXp = 3000, MaxXp = 4500,   IconFile = "elite.png" },
        new() { Name = "Boss",     MinXp = 4500, MaxXp = 6500,   IconFile = "boss.png" },
        new() { Name = "Legend",   MinXp = 6500, MaxXp = 999999, IconFile = "legend.png" }
    };

    [Theory]
    [InlineData(0, "Starter")]
    [InlineData(299, "Starter")]
    [InlineData(300, "Beginner")]
    [InlineData(699, "Beginner")]
    [InlineData(700, "Rising")]
    [InlineData(1199, "Rising")]
    [InlineData(1200, "Skilled")]
    [InlineData(4500, "Boss")]
    [InlineData(6499, "Boss")]
    [InlineData(6500, "Legend")]
    [InlineData(999999, "Legend")]
    [InlineData(1_000_000, "Legend")]
    public void For_UsesTheRankWhoseRangeContainsTheXp(int totalXp, string expectedName)
    {
        var rank = RankCalculator.For(totalXp, Ranks);

        Assert.Equal(expectedName, rank.Name);
    }

    [Fact]
    public void For_ReturnsTheRangeAndIconOfTheChosenRank()
    {
        var rank = RankCalculator.For(300, Ranks);

        Assert.Equal(300, rank.RankMinXp);
        Assert.Equal(700, rank.RankMaxXp);
        Assert.Equal("beginner.png", rank.IconFile);
    }

    [Fact]
    public void For_DoesNotDependOnTheOrderOfTheRanks()
    {
        var reversed = Ranks.Reverse().ToArray();

        var rank = RankCalculator.For(300, reversed);

        Assert.Equal("Beginner", rank.Name);
    }
}
