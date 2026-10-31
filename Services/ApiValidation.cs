using Microsoft.AspNetCore.Mvc;

namespace JavaMaster.Api.Services;

/// <summary>
/// Builds the same { error, errors, traceId } response that the rest of the API
/// uses when the automatic model-validation filter rejects a request body.
///
/// Without this, ASP.NET Core returns a different "ProblemDetails" shape, and
/// the frontend (which reads `data.error`) can only show a generic message.
/// </summary>
public static class ApiValidation
{
    public static IActionResult InvalidModelState(ActionContext context)
    {
        var errors = context.ModelState
            .Where(entry => entry.Value is { Errors.Count: > 0 })
            .ToDictionary(
                entry => entry.Key,
                entry => entry.Value!.Errors
                    .Select(error => string.IsNullOrWhiteSpace(error.ErrorMessage)
                        ? "The value is not valid."
                        : error.ErrorMessage)
                    .ToArray());

        var messages = errors.Values.SelectMany(x => x).ToList();

        // Keep the top-level message readable. The detailed messages stay in
        // `errors`, keyed by field, for debugging and for the frontend later.
        string error;
        if (messages.Any(m => Contains(m, "could not be converted") || Contains(m, "JSON")))
            error = "One or more values have the wrong type.";
        else if (messages.Any(m => Contains(m, "required")))
            error = "Some required fields are missing.";
        else
            error = messages.FirstOrDefault() ?? "The request is not valid.";

        return new BadRequestObjectResult(new
        {
            error,
            errors,
            traceId = context.HttpContext.TraceIdentifier
        });
    }

    private static bool Contains(string value, string text) =>
        value.Contains(text, StringComparison.OrdinalIgnoreCase);
}
