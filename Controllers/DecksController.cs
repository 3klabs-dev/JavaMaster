using System.Text.Json;
using JavaMaster.Api.Data;
using JavaMaster.Api.Entities;
using JavaMaster.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JavaMaster.Api.Controllers;

[ApiController]
[Route("api/decks")]
public class DecksController : ControllerBase
{
    private static readonly string[] ShareValues = { "public", "private" };

    private readonly AppDbContext _db;
    private readonly ICurrentUserService _me;

    public DecksController(AppDbContext db, ICurrentUserService me)
    {
        _db = db;
        _me = me;
    }

    // Every field is optional because the deck builder saves partial changes:
    // the older Create Deck page sends Items + Types, the builder sends Cards.
    public record DeckRequest(
        string? Name,
        string? Description,
        int? Cover,
        int? Items,
        string[]? Types,
        string? Share,
        JsonElement? Cards);

    // The player's own decks, newest edited first.
    [HttpGet]
    public async Task<IActionResult> List()
    {
        var user = await _me.GetAsync();
        var decks = await _db.Decks
            .AsNoTracking()
            .Where(d => d.OwnerUserId == user.Id)
            .OrderByDescending(d => d.UpdatedAt)
            .ToListAsync();

        return Ok(decks.Select(Summary));
    }

    // A deck opens for its owner, and for anyone with the link when it is public.
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var user = await _me.GetAsync();
        var deck = await _db.Decks.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id);
        if (deck == null) return NotFound(new { error = "Deck not found." });
        if (deck.OwnerUserId != user.Id && deck.Share != "public")
            return NotFound(new { error = "Deck not found." });

        return Ok(Summary(deck));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] DeckRequest req)
    {
        var user = await _me.GetAsync();

        var applied = Apply(req, null);
        if (applied.error != null) return BadRequest(new { error = applied.error });

        var deck = applied.deck!;
        deck.OwnerUserId = user.Id;
        deck.CreatedAt = DateTime.UtcNow;
        deck.UpdatedAt = deck.CreatedAt;

        _db.Decks.Add(deck);
        await _db.SaveChangesAsync();

        return StatusCode(201, Summary(deck));
    }

    // Publishes or saves a deck. Only the fields that were sent are replaced.
    [HttpPatch("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] DeckRequest req)
    {
        var user = await _me.GetAsync();
        var deck = await _db.Decks.FirstOrDefaultAsync(d => d.Id == id && d.OwnerUserId == user.Id);
        if (deck == null) return NotFound(new { error = "Deck not found." });

        var applied = Apply(req, deck);
        if (applied.error != null) return BadRequest(new { error = applied.error });

        deck.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(Summary(deck));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await _me.GetAsync();
        var deck = await _db.Decks.FirstOrDefaultAsync(d => d.Id == id && d.OwnerUserId == user.Id);
        if (deck == null) return NotFound(new { error = "Deck not found." });

        _db.Decks.Remove(deck);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private record Applied(Deck? deck, string? error);

    // Validates the request and applies it to a new deck (create) or a stored deck (update).
    private static Applied Apply(DeckRequest req, Deck? target)
    {
        var deck = target ?? new Deck();
        var isCreate = target == null;

        if (isCreate || req.Name != null)
        {
            var name = req.Name?.Trim() ?? "";
            if (name.Length == 0)
                return new(null, "Enter a deck name.");
            if (name.Length > GameRules.DeckNameMaxLength)
                return new(null, $"Deck name can be at most {GameRules.DeckNameMaxLength} characters.");
            deck.Name = name;
        }

        if (req.Description != null)
        {
            var description = req.Description.Trim();
            if (description.Length > GameRules.DeckDescriptionMaxLength)
                return new(null, $"Description can be at most {GameRules.DeckDescriptionMaxLength} characters.");
            deck.Description = description.Length == 0 ? null : description;
        }

        if (isCreate || req.Cover != null)
        {
            if (req.Cover == null || req.Cover < 1 || req.Cover > GameRules.DeckCoverCount)
                return new(null, "Choose a cover image.");
            deck.CoverNumber = req.Cover.Value;
        }

        if (req.Share != null)
        {
            if (!ShareValues.Contains(req.Share))
                return new(null, "Choose who can see this deck.");
            deck.Share = req.Share;
        }

        if (req.Cards != null)
        {
            // The builder saves the card list as JSON. Keep the important checks here
            // (a real list, the allowed types, a size limit) so the API cannot be used
            // to store an arbitrary blob.
            if (req.Cards.Value.ValueKind != JsonValueKind.Array)
                return new(null, "The cards must be a list.");

            var cards = req.Cards.Value;
            var count = cards.GetArrayLength();
            if (count < 1)
                return new(null, "Add at least one card.");
            if (count > GameRules.DeckCardMaxCount)
                return new(null, $"A deck can have at most {GameRules.DeckCardMaxCount} cards.");

            var json = cards.GetRawText();
            if (json.Length > GameRules.DeckCardsMaxJsonLength)
                return new(null, "This deck is too large to save.");

            var types = new List<string>();
            foreach (var card in cards.EnumerateArray())
            {
                if (card.ValueKind != JsonValueKind.Object ||
                    !card.TryGetProperty("type", out var typeElement) ||
                    typeElement.ValueKind != JsonValueKind.String)
                    return new(null, "Every card needs a type.");

                var type = typeElement.GetString() ?? "";
                if (!GameRules.DeckQuestionTypes.Contains(type))
                    return new(null, $"Unknown card type: {type}");
                if (!types.Contains(type)) types.Add(type);
            }

            deck.Cards = json;
            deck.ItemCount = count;
            deck.QuestionTypes = JsonSerializer.Serialize(
                GameRules.DeckQuestionTypes.Where(types.Contains));
        }
        else if (isCreate)
        {
            // The older Create Deck page sends a target item count and a type list
            // instead of cards. It still works, so that page is not broken.
            if (req.Items == null || !GameRules.DeckItemOptions.Contains(req.Items.Value))
                return new(null, "Select the number of items.");

            var chosen = req.Types ?? Array.Empty<string>();
            if (chosen.Length == 0 || chosen.Any(t => !GameRules.DeckQuestionTypes.Contains(t)))
                return new(null, "Keep at least one question type.");

            deck.ItemCount = req.Items.Value;
            deck.QuestionTypes = JsonSerializer.Serialize(
                GameRules.DeckQuestionTypes.Where(chosen.Contains));
        }

        return new(deck, null);
    }

    private static object Summary(Deck deck)
    {
        JsonElement cards;
        string[] types;
        try { cards = JsonSerializer.Deserialize<JsonElement>(deck.Cards); }
        catch (JsonException) { cards = JsonSerializer.Deserialize<JsonElement>("[]"); }
        try { types = JsonSerializer.Deserialize<string[]>(deck.QuestionTypes) ?? Array.Empty<string>(); }
        catch (JsonException) { types = Array.Empty<string>(); }

        // Epoch milliseconds keep the browser-side sorting simple (newest edited first).
        static long Ms(DateTime t) =>
            new DateTimeOffset(DateTime.SpecifyKind(t, DateTimeKind.Utc)).ToUnixTimeMilliseconds();

        return new
        {
            id = deck.Id,
            name = deck.Name,
            description = deck.Description,
            cover = deck.CoverNumber,
            items = deck.ItemCount,
            share = deck.Share,
            types,
            cards,
            createdAt = Ms(deck.CreatedAt),
            updatedAt = Ms(deck.UpdatedAt)
        };
    }
}
