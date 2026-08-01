using Microsoft.Azure.Cosmos;
using OuterloopLabApi.Repositories;
using OuterloopLabApi.Settings;
using Azure.Identity;

namespace OuterloopLabApi.Data;

public sealed class CosmosContainerFactory
{
    private readonly CosmosDbOptions _options;
    private readonly DefaultAzureCredential _credential;

    public CosmosContainerFactory(CosmosDbOptions options)
    {
        _options = options;
        _credential = new DefaultAzureCredential(
            new DefaultAzureCredentialOptions
            {
                ManagedIdentityClientId = _options.ManagedIdentityClientId,
            });
    }

    public CosmosClient CreateCosmosClient() => new CosmosClient(_options.CosmosDbUri, _credential);

    public Container CreateContainer(CosmosClient client)
    {
        return client.GetContainer(_options.DatabaseName, _options.ContainerName);
    }
}
