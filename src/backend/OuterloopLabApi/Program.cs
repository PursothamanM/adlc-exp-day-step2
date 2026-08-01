using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.CosmosDB;
using Azure.ResourceManager.CosmosDB.Models;
using Microsoft.Azure.Cosmos;
using OuterloopLabApi.Data;
using OuterloopLabApi.Domain;
using OuterloopLabApi.Providers;
using OuterloopLabApi.Repositories;
using OuterloopLabApi.Services;
using OuterloopLabApi.Settings;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<CosmosDbOptions>(sp =>
{
    static string MustGet(string key)
    {
        var value = Environment.GetEnvironmentVariable(key);
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException($"Missing required environment variable: {key}");
        return value;
    }

    return new CosmosDbOptions
    {
        CosmosDbUri = MustGet("COSMOS_DB_URI"),
        DatabaseName = MustGet("COSMOS_DB_DATABASE"),
        ContainerName = MustGet("COSMOS_DB_CONTAINER"),
        AccountName = MustGet("COSMOS_DB_ACCOUNT_NAME"),
        ResourceGroupName = MustGet("COSMOS_DB_RESOURCE_GROUP"),
        Region = MustGet("COSMOS_DB_REGION"),
        ManagedIdentityClientId = MustGet("AZURE_MANAGED_IDENTITY_CLIENT_ID"),
    };
});

builder.Services.AddSingleton<IClock, SystemClock>();

builder.Services.AddHttpClient<ICurrencyRateProvider, FrankfurterCurrencyRateProvider>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(10);
});

builder.Services.AddSingleton<IConversionAuditRepository, CosmosConversionAuditRepository>();
builder.Services.AddSingleton<IConversionService, ConversionService>();

builder.Services.AddSingleton<CosmosProvisioner>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var provisioner = scope.ServiceProvider.GetRequiredService<CosmosProvisioner>();
    await provisioner.EnsureCosmosResourcesAsync();
}

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();
