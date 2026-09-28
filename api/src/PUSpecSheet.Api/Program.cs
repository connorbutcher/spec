using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Api.ExceptionHandling;
using PUSpecSheet.Application.DependencyInjection;
using PUSpecSheet.Data;
using PUSpecSheet.Data.DependencyInjection;
using PUSpecSheet.Data.Seeding;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("Connection string 'Default' is not configured.");

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApplicationExceptionHandler>();
builder.Services.AddHealthChecks().AddDbContextCheck<PuSpecSheetDbContext>();

builder.Services.AddPuSpecSheetData(connectionString);
builder.Services.AddPuSpecSheetApplication();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // Creates the database on first run, applies any pending migrations and adds sample phases.
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<PuSpecSheetDbContext>();
    await db.Database.MigrateAsync();
    await DevelopmentDataSeeder.SeedAsync(db);

    app.MapOpenApi();
}

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/api/health");

await app.RunAsync();
