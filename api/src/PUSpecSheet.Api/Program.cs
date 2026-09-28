using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Application.DependencyInjection;
using PUSpecSheet.Data;
using PUSpecSheet.Data.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("Connection string 'Default' is not configured.");

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks().AddDbContextCheck<PuSpecSheetDbContext>();

builder.Services.AddPuSpecSheetData(connectionString);
builder.Services.AddPuSpecSheetApplication();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // Creates the database on first run and applies any pending migrations.
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<PuSpecSheetDbContext>();
    db.Database.Migrate();

    app.MapOpenApi();
}

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/api/health");

app.Run();
