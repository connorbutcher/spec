using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Api.Cors;
using PUSpecSheet.Api.ExceptionHandling;
using PUSpecSheet.Api.Users;
using PUSpecSheet.Application.DependencyInjection;
using PUSpecSheet.Application.Sheets.Demo;
using PUSpecSheet.Application.Users;
using PUSpecSheet.Data;
using PUSpecSheet.Data.DependencyInjection;
using PUSpecSheet.Data.Seeding;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("Connection string 'Default' is not configured.");

// Enums such as a template's orientation or a cell type's kind go over the wire as their names. A cell
// configuration's "kind" discriminator can come anywhere in the object the UI sends.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.AllowOutOfOrderMetadataProperties = true;
    });
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApplicationExceptionHandler>();
builder.Services.AddHealthChecks().AddDbContextCheck<PuSpecSheetDbContext>();
builder.Services.AddPuSpecSheetCors(builder.Configuration);

builder.Services.AddPuSpecSheetData(connectionString);
builder.Services.AddPuSpecSheetApplication();

// Until sign-in is added, every request runs as the seeded developer user.
builder.Services.AddScoped<ICurrentUser, DeveloperCurrentUser>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // Creates the database on first run, applies any pending migrations and adds sample phases.
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<PuSpecSheetDbContext>();
    await db.Database.MigrateAsync();
    await DevelopmentDataSeeder.SeedAsync(db);
    await scope.ServiceProvider.GetRequiredService<DemoDataSeeder>().SeedAsync();

    app.MapOpenApi();
}

app.UseExceptionHandler();

// CORS runs before the HTTPS redirect: browsers reject a redirected preflight (OPTIONS) request, so
// a redirect ahead of CORS shows up in the UI as a CORS error. In development the API is called over
// plain http, so it isn't redirected at all.
app.UseCors(PuSpecSheetCorsExtensions.PolicyName);
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/api/health");

await app.RunAsync();
