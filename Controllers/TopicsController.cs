using JavaMaster.Api.Data;
using JavaMaster.Api.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JavaMaster.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TopicsController : ControllerBase
{
    private readonly AppDbContext _db;
    public TopicsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<List<Topic>>> GetAll() =>
        await _db.Topics.Where(t => t.IsActive).ToListAsync();

    // Creating a topic is intentionally not exposed yet. There is no login, so a
    // public POST would let any visitor add topics. It comes back when there is
    // an admin account to protect it.
}