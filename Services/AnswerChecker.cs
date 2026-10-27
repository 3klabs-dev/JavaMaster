using System.Text.Json;
using JavaMaster.Api.Entities;

namespace JavaMaster.Api.Services;

public static class AnswerChecker
{
    public static bool IsCorrect(Question q, JsonElement answer)
    {
        try
        {
            using var doc = JsonDocument.Parse(q.AnswerKey);
            var key = doc.RootElement;
            return q.Type switch
            {
                QuestionType.MultipleChoice => Choice(key, answer),
                QuestionType.FillBlank      => Text(key, answer),
                QuestionType.ArrangeTokens  => Order(key, answer),
                QuestionType.FindBug        => Line(key, answer),
                QuestionType.FillSlots      => Slots(key, answer),
                _ => false
            };
        }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException
                                      or KeyNotFoundException or FormatException)
        {
            return false;   // a wrongly shaped answer is simply wrong
        }
    }

    private static bool Choice(JsonElement key, JsonElement a) =>
        a.GetProperty("optionId").GetString() == key.GetProperty("correctOptionId").GetString();

    /// <summary>
    /// Java is a case-sensitive language, so an answer is compared exactly -
    /// DOUBLE is not double - unless the question asks for something else.
    ///
    /// A question opts out of that with "caseSensitive": false, which the long
    /// suffix question does, because Java accepts both L and l.
    ///
    /// A missing flag means the strict default, and a flag that is not a real
    /// boolean is ignored rather than read. A broken flag must never turn a
    /// question into one that can never be answered.
    /// </summary>
    private static bool IsCaseSensitive(JsonElement key)
    {
        if (!key.TryGetProperty("caseSensitive", out var flag)) return true;   // the strict default
        if (flag.ValueKind == JsonValueKind.False) return false;              // the explicit opt-out
        return true;
    }

    private static bool Text(JsonElement key, JsonElement a)
    {
        var text = a.GetProperty("text").GetString()?.Trim() ?? "";
        var cmp = IsCaseSensitive(key) ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
        return key.GetProperty("accepted").EnumerateArray()
                  .Any(x => string.Equals(x.GetString()?.Trim(), text, cmp));
    }

    private static bool Order(JsonElement key, JsonElement a)
    {
        var given = a.GetProperty("order").EnumerateArray().Select(x => x.GetString()).ToList();
        var expected = key.GetProperty("order").EnumerateArray().Select(x => x.GetString()).ToList();
        if (given.Count != expected.Count) return false;
        if (IsCaseSensitive(key)) return given.SequenceEqual(expected);
        return given.Zip(expected).All(p => string.Equals(p.First, p.Second, StringComparison.OrdinalIgnoreCase));
    }

    private static bool Line(JsonElement key, JsonElement a) =>
        a.GetProperty("lineNumber").GetInt32() == key.GetProperty("lineNumber").GetInt32();

    private static bool Slots(JsonElement key, JsonElement a)
    {
        var given = a.GetProperty("slots");
        var cmp = IsCaseSensitive(key) ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
        foreach (var slot in key.GetProperty("slots").EnumerateObject())
            if (!given.TryGetProperty(slot.Name, out var v) ||
                !string.Equals(v.GetString(), slot.Value.GetString(), cmp))
                return false;
        return true;
    }
}
