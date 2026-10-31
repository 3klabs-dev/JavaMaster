using JavaMaster.Api.Entities;
using JavaMaster.Api.Services;

namespace JavaMaster.Api.Tests.Services;

public class StreakServiceTests
{
    private static User UserWith(int streak, int longest, int coins, DateOnly? lastActivity) =>
        new()
        {
            CurrentStreak = streak,
            LongestStreak = longest,
            Coins = coins,
            LastActivityDate = lastActivity
        };

    [Fact]
    public void RecordActivity_WhenThePlayerWasAlreadyActiveToday_DoesNothing()
    {
        var today = StreakService.Today();
        var user = UserWith(5, 5, 100, today);

        var bonus = StreakService.RecordActivity(user);

        Assert.Equal(0, bonus);
        Assert.Equal(5, user.CurrentStreak);
        Assert.Equal(5, user.LongestStreak);
        Assert.Equal(100, user.Coins);
        Assert.Equal(today, user.LastActivityDate);
    }

    [Fact]
    public void RecordActivity_OnTheNextDay_IncrementsTheStreakAndPaysTheBonus()
    {
        var today = StreakService.Today();
        var user = UserWith(5, 5, 100, today.AddDays(-1));

        var bonus = StreakService.RecordActivity(user);

        Assert.Equal(GameRules.StreakBonusCoins, bonus);
        Assert.Equal(6, user.CurrentStreak);
        Assert.Equal(6, user.LongestStreak);
        Assert.Equal(100 + GameRules.StreakBonusCoins, user.Coins);
        Assert.Equal(today, user.LastActivityDate);
    }

    [Fact]
    public void RecordActivity_AfterAMissedDay_StartsANewStreak()
    {
        var today = StreakService.Today();
        var user = UserWith(5, 10, 100, today.AddDays(-2));

        var bonus = StreakService.RecordActivity(user);

        Assert.Equal(GameRules.StreakBonusCoins, bonus);
        Assert.Equal(1, user.CurrentStreak);
        Assert.Equal(10, user.LongestStreak);   // the old best is never lost
        Assert.Equal(100 + GameRules.StreakBonusCoins, user.Coins);
        Assert.Equal(today, user.LastActivityDate);
    }

    [Fact]
    public void RecordActivity_ForANewPlayer_StartsAtOneDay()
    {
        var today = StreakService.Today();
        var user = UserWith(0, 0, 0, null);

        var bonus = StreakService.RecordActivity(user);

        Assert.Equal(GameRules.StreakBonusCoins, bonus);
        Assert.Equal(1, user.CurrentStreak);
        Assert.Equal(1, user.LongestStreak);
        Assert.Equal(GameRules.StreakBonusCoins, user.Coins);
        Assert.Equal(today, user.LastActivityDate);
    }

    [Fact]
    public void RecordActivity_KeepsTheOldBestStreakWhenTheCurrentStreakIsLower()
    {
        var today = StreakService.Today();
        var user = UserWith(3, 12, 0, today.AddDays(-1));

        StreakService.RecordActivity(user);

        Assert.Equal(4, user.CurrentStreak);
        Assert.Equal(12, user.LongestStreak);
    }
}
