using JavaMaster.Api.Converters;
using JavaMaster.Api.Data;
using JavaMaster.Api.Services;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("Default")));
    builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(o =>
    {
        // Keep bad request bodies in the same { error } shape as every other
        // API error, so the frontend can show a real message.
        o.InvalidModelStateResponseFactory = ApiValidation.InvalidModelState;
    })
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        o.JsonSerializerOptions.Converters.Add(new UtcDateTimeJsonConverter());
    });

builder.Services.AddOpenApi();

var app = builder.Build();

// Apply migrations and seed data on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        await db.Database.MigrateAsync();
        await DbSeeder.SeedAsync(db);
        await LessonContent.SeedAsync(db);
    }
    catch (Exception ex)
    {
        // A missing or misconfigured database is the most common reason the app
        // will not start, so say what to do instead of printing a stack trace.
        Console.Error.WriteLine();
        Console.Error.WriteLine("  Java Master could not reach SQL Server, so it cannot start.");
        Console.Error.WriteLine();
        Console.Error.WriteLine($"  Connection string: {builder.Configuration.GetConnectionString("Default")}");
        Console.Error.WriteLine($"  Error: {ex.Message}");
        Console.Error.WriteLine();
        Console.Error.WriteLine("  Things to check:");
        Console.Error.WriteLine("    1. SQL Server is installed and running (open services.msc).");
        Console.Error.WriteLine("    2. The connection string names the right instance:");
        Console.Error.WriteLine("         Server=localhost              default instance");
        Console.Error.WriteLine("         Server=localhost\\SQLEXPRESS   SQL Server Express");
        Console.Error.WriteLine("         Server=(localdb)\\MSSQLLocalDB LocalDB");
        Console.Error.WriteLine("    3. Your Windows account can sign in to it (Windows authentication).");
        Console.Error.WriteLine();
        Console.Error.WriteLine("  The JavaMaster database is created and seeded for you, so there is");
        Console.Error.WriteLine("  nothing else to set up by hand.");
        Console.Error.WriteLine();
        Environment.ExitCode = 1;
        return;
    }
}

// Any unexpected exception is logged with its trace id, and the browser gets a
// small JSON error instead of a stack trace or an empty 500 page.
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
    {
        // The learner closed the tab. There is nobody left to answer.
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Unhandled exception. TraceId: {TraceId}", context.TraceIdentifier);

        // Once bytes are on the wire the response cannot be replaced.
        if (context.Response.HasStarted) throw;

        context.Response.Clear();
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsJsonAsync(new
        {
            error = "Something went wrong.",
            traceId = context.TraceIdentifier
        });
    }
});

// Actions that return a bare NotFound() (or another empty 4xx) get the same
// { error } shape as the rest of the API, so the frontend always has a message.
app.UseStatusCodePages(async statusCodeContext =>
{
    var context = statusCodeContext.HttpContext;
    if (!context.Request.Path.StartsWithSegments("/api")) return;

    var message = context.Response.StatusCode switch
    {
        400 => "Bad request.",
        401 => "Please sign in.",
        403 => "You do not have access.",
        404 => "Not found.",
        405 => "Method not allowed.",
        _   => "Request failed."
    };

    await context.Response.WriteAsJsonAsync(new
    {
        error = message,
        traceId = context.TraceIdentifier
    });
});

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    // While developing, never let the browser cache a page or a script, so an
    // edit shows up on the next refresh instead of the next hard reload.
    app.Use(async (context, next) =>
    {
        context.Response.Headers.CacheControl = "no-store, no-cache, must-revalidate, max-age=0";
        context.Response.Headers.Pragma = "no-cache";
        await next();
    });
}

app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseAuthorization();
app.MapControllers();

app.Run();