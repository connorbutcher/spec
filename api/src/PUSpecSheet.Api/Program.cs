using System.Diagnostics;
using System.Reflection;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.OpenApi;
using PUSpecSheet.Api.Common.Authentication;
using PUSpecSheet.Api.Common.Claims;
using PUSpecSheet.Api.Common.Cors;
using PUSpecSheet.Api.Common.Documentation;
using PUSpecSheet.Api.Common.Logging;
using PUSpecSheet.Api.DAL;
using PUSpecSheet.Api.Emails;

OpenApiInfo apiInfo = new()
{
    Title = "PUSpecSheet.API",
    Description = "API Service for the PU Spec Sheet system.",
    Version = FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly()!.Location).FileVersion
};

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();

builder.AddSerilog();
builder.AddIdentityWebApiAuthentication();

builder.Services.AddClaimsAccessors();
builder.Services.AddCorsPolicy(builder.Configuration.GetRequiredSection("Cors"));

builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<GzipCompressionProvider>();
});

builder.Services.AddControllers(opt =>
{
    opt.MaxValidationDepth = 256;
}).AddJsonOptions(options =>
{
    options.JsonSerializerOptions.MaxDepth = 256;
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

builder.AddDocumentation(apiInfo, (config) =>
{
    config.WithEntraIDAuthentication(JwtBearerDefaults.AuthenticationScheme);
});

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
        .WithEntraIDAuthentication()
        .WithNoSyntaxHighlight();
});

app.UseResponseCompression();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseCors(CorsConstants.DefaultCorsPolicy);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
