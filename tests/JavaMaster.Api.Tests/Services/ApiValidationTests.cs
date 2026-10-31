using System.Text.Json;
using JavaMaster.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Routing;

namespace JavaMaster.Api.Tests.Services;

public class ApiValidationTests
{
    [Fact]
    public void AValueOfTheWrongType_ReturnsTheCommonErrorShape()
    {
        var context = ContextWithError(
            "$.packSize",
            "The JSON value could not be converted to System.Int32.");

        var result = Assert.IsType<BadRequestObjectResult>(
            ApiValidation.InvalidModelState(context));
        Assert.Equal(400, result.StatusCode);

        using var body = JsonDocument.Parse(JsonSerializer.Serialize(result.Value));
        var root = body.RootElement;

        Assert.Equal("One or more values have the wrong type.",
            root.GetProperty("error").GetString());
        Assert.Equal("test-trace", root.GetProperty("traceId").GetString());

        var errors = root.GetProperty("errors");
        Assert.True(errors.TryGetProperty("$.packSize", out var messages));
        Assert.Equal("The JSON value could not be converted to System.Int32.",
            messages[0].GetString());
    }

    [Fact]
    public void AMissingBody_UsesTheRequiredMessage()
    {
        var context = ContextWithError("", "A non-empty request body is required.");

        var result = Assert.IsType<BadRequestObjectResult>(
            ApiValidation.InvalidModelState(context));

        using var body = JsonDocument.Parse(JsonSerializer.Serialize(result.Value));
        Assert.Equal("Some required fields are missing.",
            body.RootElement.GetProperty("error").GetString());
    }

    [Fact]
    public void AnyOtherValidationMessage_IsShownAsItIs()
    {
        var context = ContextWithError("fullName", "That name is too long.");

        var result = Assert.IsType<BadRequestObjectResult>(
            ApiValidation.InvalidModelState(context));

        using var body = JsonDocument.Parse(JsonSerializer.Serialize(result.Value));
        Assert.Equal("That name is too long.",
            body.RootElement.GetProperty("error").GetString());
    }

    private static ActionContext ContextWithError(string key, string message)
    {
        var http = new DefaultHttpContext { TraceIdentifier = "test-trace" };
        var context = new ActionContext(http, new RouteData(), new ActionDescriptor());
        context.ModelState.AddModelError(key, message);
        return context;
    }
}
