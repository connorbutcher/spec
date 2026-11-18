using System.Diagnostics;
using System.Reflection;
using System.Text.Json.Serialization;
// Auth is disabled for local running; see DefaultUserClaimsAccessor.
// using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.OpenApi;
// using PUSpecSheet.Api.Common.Authentication;
using PUSpecSheet.Api.Common.Claims;
using PUSpecSheet.Api.Common.Cors;
using PUSpecSheet.Api.Common.Documentation;
using PUSpecSheet.Api.Common.Logging;
using PUSpecSheet.Api.DAL;
using PUSpecSheet.Api.Emails;
using PUSpecSheet.Api.Filters;

OpenApiInfo apiInfo = new()
{
    Title = "PUSpecSheet.API",
    Description = "API Service for the PU Spec Sheet system.",
    Version = FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly()!.Location).FileVersion
};

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();

builder.AddSerilog();
// Auth is disabled for local running: every request acts as the default admin user (DefaultUser).
// builder.AddIdentityWebApiAuthentication();
// builder.Services.AddClaimsAccessors();
builder.Services.AddDefaultUserClaimsAccessors();
builder.Services.AddCorsPolicy(builder.Configuration.GetRequiredSection("Cors"));

builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<GzipCompressionProvider>();
});

builder.Services.AddControllers(opt =>
{
    opt.MaxValidationDepth = 256;
    opt.Filters.Add<SheetEditExceptionFilter>();
}).AddJsonOptions(options =>
{
    options.JsonSerializerOptions.MaxDepth = 256;
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

// builder.AddDocumentation(apiInfo, (config) =>
// {
//     config.WithEntraIDAuthentication(JwtBearerDefaults.AuthenticationScheme);
// });
builder.AddDocumentation(apiInfo);

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();

builder.AddEmailModule();
builder.AddDataAccessLayerModule();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseDocumentation(apiInfo, (config) =>
{
    config
        // .WithEntraIDAuthentication()
        .WithNoSyntaxHighlight();
});

app.UseResponseCompression();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseCors(CorsConstants.DefaultCorsPolicy);

// app.UseAuthentication();
// app.UseAuthorization();

app.MapControllers();

app.Run();
