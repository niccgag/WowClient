using WowClient;
using WowClient.Extensions;
using WowClient.WebApi.Contracts;
using WowClient.WebApi.Endpoints;
using WowClient.WebApi.Errors;

var builder = WebApplication.CreateBuilder(args);

// Bind the client options from the "Blizzard" configuration section.
builder.Services.AddWowClient(options =>
    builder.Configuration.GetSection("Blizzard").Bind(options));

// Fail fast with a clear message when credentials have not been configured.
builder.Services.AddOptions<WowClientOptions>()
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.ClientId),
        "Blizzard:ClientId is required. Use user-secrets, environment variables (Blizzard__ClientId), or local configuration.")
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.ClientSecret),
        "Blizzard:ClientSecret is required. Use user-secrets, environment variables (Blizzard__ClientSecret), or local configuration.")
    .ValidateOnStart();

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<WowApiExceptionHandler>();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/", () => TypedResults.Ok(new ServiceInfoResponse(
        "WowClient sample API",
        app.Environment.IsDevelopment() ? "/openapi/v1.json" : "OpenAPI is mapped in Development only.")))
    .WithTags("Service")
    .WithName("GetServiceInfo");

app.MapGet("/health", () => TypedResults.Ok(new HealthResponse("ok")))
    .WithTags("Service")
    .WithName("GetHealth");

app.MapGameDataEndpoints();
app.MapProfileEndpoints();

app.Run();

// Exposed for integration testing with WebApplicationFactory<Program>.
public partial class Program;
