using System.Net;
using System.Text.Json;
using Bizcord.ApiGateway.Extensions;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Vault read
#region Vault Read
var address = Environment.GetEnvironmentVariable("VAULT_ADDR")
              ?? "http://vault:8200";

var token = Environment.GetEnvironmentVariable("VAULT_TOKEN") ?? "DO NOT HARDCODE TOKEN";

using var client = new HttpClient
{
    BaseAddress = new Uri(address.TrimEnd('/') + "/"),
    Timeout = TimeSpan.FromSeconds(5)
};

client.DefaultRequestHeaders.Add("X-Vault-Token", token);
using var response = await client.GetAsync("v1/secret/data/pingpong");
if (response.StatusCode == HttpStatusCode.Forbidden)
    throw new InvalidOperationException("Vault denied access: check token, TTL and policy.");

response.EnsureSuccessStatusCode();
using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
var apiKey = document.RootElement.GetProperty("data").GetProperty("data")
    .GetProperty("ApiKey").GetString();
if (string.IsNullOrWhiteSpace(apiKey))
    throw new InvalidOperationException("Vault secret ApiKey is missing or empty.");

Environment.SetEnvironmentVariable("AuthenticationProviderKey", apiKey);


#endregion

// Ocelot merges these source files into a generated ocelot.json at startup.
builder.Configuration.AddOcelot("Configuration", builder.Environment);
builder.Services.AddJwtTokenService(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddApiGatewayServices(builder.Configuration);

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Execute the faucet controller before Ocelot's terminal proxy pipeline.
#pragma warning disable ASP0014
app.UseEndpoints(endpoints => endpoints.MapControllers());
#pragma warning restore ASP0014
// These exercise headers must come from validated token claims, not the client.
app.Use(async (context, next) =>
{
    context.Request.Headers.Remove("X-UserId");
    context.Request.Headers.Remove("X-Role");
    await next(context);
});
await app.UseOcelot();
await app.RunAsync();