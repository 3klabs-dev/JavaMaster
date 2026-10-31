using System.Text.Json;
using JavaMaster.Api.Entities;
using JavaMaster.Api.Services;

namespace JavaMaster.Api.Tests.Services;

public class AnswerCheckerTests
{
    private static Question QuestionOf(QuestionType type, string answerKey) =>
        new() { Type = type, AnswerKey = answerKey };

    private static JsonElement Json(string json)
    {
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.Clone();
    }

    [Fact]
    public void MultipleChoice_WithTheCorrectOption_IsCorrect()
    {
        var question = QuestionOf(QuestionType.MultipleChoice,
            """{"correctOptionId":"b"}""");

        Assert.True(AnswerChecker.IsCorrect(question, Json("""{"optionId":"b"}""")));
    }

    [Fact]
    public void MultipleChoice_WithAnotherOption_IsWrong()
    {
        var question = QuestionOf(QuestionType.MultipleChoice,
            """{"correctOptionId":"b"}""");

        Assert.False(AnswerChecker.IsCorrect(question, Json("""{"optionId":"a"}""")));
    }

    [Fact]
    public void FillBlank_IsCaseSensitiveByDefault()
    {
        var question = QuestionOf(QuestionType.FillBlank,
            """{"accepted":["double"]}""");

        Assert.True(AnswerChecker.IsCorrect(question, Json("""{"text":"double"}""")));
        Assert.False(AnswerChecker.IsCorrect(question, Json("""{"text":"DOUBLE"}""")));
    }

    [Fact]
    public void FillBlank_CanOptOutOfCaseSensitivity()
    {
        var question = QuestionOf(QuestionType.FillBlank,
            """{"accepted":["double"],"caseSensitive":false}""");

        Assert.True(AnswerChecker.IsCorrect(question, Json("""{"text":"DOUBLE"}""")));
    }

    [Fact]
    public void FillBlank_IgnoresWhitespaceAroundTheAnswer()
    {
        var question = QuestionOf(QuestionType.FillBlank,
            """{"accepted":["double"]}""");

        Assert.True(AnswerChecker.IsCorrect(question, Json("""{"text":"  double  "}""")));
    }

    [Fact]
    public void FillBlank_ABrokenCaseFlagFallsBackToTheStrictRule()
    {
        var question = QuestionOf(QuestionType.FillBlank,
            """{"accepted":["double"],"caseSensitive":"false"}""");

        Assert.True(AnswerChecker.IsCorrect(question, Json("""{"text":"double"}""")));
        Assert.False(AnswerChecker.IsCorrect(question, Json("""{"text":"DOUBLE"}""")));
    }

    [Fact]
    public void ArrangeTokens_RequiresTheExactOrder()
    {
        var question = QuestionOf(QuestionType.ArrangeTokens,
            """{"order":["System","out","println"]}""");

        Assert.True(AnswerChecker.IsCorrect(question,
            Json("""{"order":["System","out","println"]}""")));

        Assert.False(AnswerChecker.IsCorrect(question,
            Json("""{"order":["System","println","out"]}""")));
    }

    [Fact]
    public void ArrangeTokens_CanOptOutOfCaseSensitivity()
    {
        var question = QuestionOf(QuestionType.ArrangeTokens,
            """{"order":["System","out","println"],"caseSensitive":false}""");

        Assert.True(AnswerChecker.IsCorrect(question,
            Json("""{"order":["system","OUT","println"]}""")));
    }

    [Fact]
    public void FindBug_ChecksTheLineNumber()
    {
        var question = QuestionOf(QuestionType.FindBug,
            """{"lineNumber":2}""");

        Assert.True(AnswerChecker.IsCorrect(question, Json("""{"lineNumber":2}""")));
        Assert.False(AnswerChecker.IsCorrect(question, Json("""{"lineNumber":3}""")));
    }

    [Fact]
    public void FillSlots_RequiresEverySlotToMatch()
    {
        var question = QuestionOf(QuestionType.FillSlots,
            """{"slots":{"1":"print","2":"println"}}""");

        Assert.True(AnswerChecker.IsCorrect(question,
            Json("""{"slots":{"1":"print","2":"println"}}""")));

        Assert.False(AnswerChecker.IsCorrect(question,
            Json("""{"slots":{"1":"println","2":"println"}}""")));

        Assert.False(AnswerChecker.IsCorrect(question,
            Json("""{"slots":{"1":"print"}}""")));
    }

    [Fact]
    public void FillSlots_CanOptOutOfCaseSensitivity()
    {
        var question = QuestionOf(QuestionType.FillSlots,
            """{"slots":{"1":"print"},"caseSensitive":false}""");

        Assert.True(AnswerChecker.IsCorrect(question,
            Json("""{"slots":{"1":"PRINT"}}""")));
    }

    [Fact]
    public void ABrokenAnswerKey_IsTreatedAsWrong()
    {
        var question = QuestionOf(QuestionType.FillBlank, "{ this is not valid json");
        Assert.False(AnswerChecker.IsCorrect(question, Json("""{"text":"double"}""")));
    }

    [Fact]
    public void AnAnswerWithTheWrongShape_IsTreatedAsWrong()
    {
        var question = QuestionOf(QuestionType.MultipleChoice,
            """{"correctOptionId":"b"}""");

        Assert.False(AnswerChecker.IsCorrect(question, Json("""{}""")));
    }

    [Fact]
    public void AnUnknownQuestionType_IsTreatedAsWrong()
    {
        var question = QuestionOf((QuestionType)99,
            """{"accepted":["double"]}""");

        Assert.False(AnswerChecker.IsCorrect(question, Json("""{"text":"double"}""")));
    }
}
