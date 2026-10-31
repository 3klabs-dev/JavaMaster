using JavaMaster.Api.Data;
using JavaMaster.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace JavaMaster.Api.Services;

/// <summary>
/// Makes the requests that change a player's coins, XP or inventory run one at
/// a time for that player.
///
/// The problem it solves: two requests can both read "140 coins", both decide a
/// purchase is affordable, and both save. The last save wins, so one purchase
/// is paid for and the other is not. A database row lock makes the second
/// request wait until the first one commits, so it reads the new total.
///
/// The caller must already be inside a transaction. The lock lasts until that
/// transaction commits or rolls back.
/// </summary>
public static class UserRowLock
{
    public static async Task<User> AcquireAsync(AppDbContext db, ICurrentUserService me)
    {
        // Make sure the player row exists first (CurrentUserService creates the
        // test player on the very first request).
        var user = await me.GetAsync();

        // UPDLOCK = "I am going to update this row, so block other writers."
        // ROWLOCK = lock this player's row, not the whole table.
        // SQL Server releases the lock when the transaction ends.
        await db.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT 1 FROM [Users] WITH (UPDLOCK, ROWLOCK) WHERE [Id] = {user.Id}");

        // The row was loaded before the lock was taken, so read it again now
        // that nobody else can change it.
        await db.Entry(user).ReloadAsync();

        return user;
    }
}
