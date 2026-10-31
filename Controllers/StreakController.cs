using JavaMaster.Api.Data;
using JavaMaster.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JavaMaster.Api.Controllers;

[ApiController]
[Route("api/streak")]
public class StreakController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _me;
    public StreakController(AppDbContext db, ICurrentUserService me) { _db = db; _me = me; }

    // Brings back a streak that was broken by exactly one missed day.
    [HttpPost("repair")]
    public async Task<IActionResult> Repair()
    {
        // Consuming a repair also changes the player row, so it waits for any
        // other coin/XP change for the same player to finish first.
        await using var tx = await _db.Database.BeginTransactionAsync();
        var user = await UserRowLock.AcquireAsync(_db, _me);
        var today = StreakService.Today();

        // Only possible when the last active day was the day before yesterday
        if (user.LastActivityDate != today.AddDays(-2) || user.CurrentStreak <= 0)
            return BadRequest(new { error = "There is no streak to repair." });

        var item = await _db.ShopItems.FirstAsync(s => s.Code == "STREAK_REPAIR");
        var inv = await _db.UserInventory.FindAsync(user.Id, item.Id);
        if (inv == null || inv.Quantity < 1)
            return BadRequest(new { error = "You have no Streak Repair. Buy one in the shop." });

        inv.Quantity--;
        user.LastActivityDate = today.AddDays(-1);   // the missed day is now covered
        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        // The next completed activity today makes the streak one higher (14 becomes 15)
        return Ok(new { streak = user.CurrentStreak, repairsLeft = inv.Quantity });
    }
}