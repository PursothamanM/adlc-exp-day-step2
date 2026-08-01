using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.CosmosDB;
using Azure.ResourceManager.CosmosDB.Models;
using Azure.ResourceManager.Resources;
using Microsoft.Azure.Cosmos;
using OuterloopLabApi.Settings;

namespace OuterloopLabApi.Data;

public sealed class CosmosProvisioner
{
    private readonly CosmosDbOptions _options;

    public CosmosProvisioner(CosmosDbOptions options)
    {
        _options = options;
    }

    public async Task EnsureCosmosResourcesAsync(CancellationToken cancellationToken = default)
    {
        var credential = new DefaultAzureCredential(
            new DefaultAzureCredentialOptions { ManagedIdentityClientId = _options.ManagedIdentityClientId });

        // Control plane (ARM): best-effort. Data-plane below is authoritative.
        try
        {
            var armClient = new ArmClient(credential);
            var subscription = await armClient.GetDefaultSubscriptionAsync(cancellationToken);
            var resourceGroup = await subscription.GetResourceGroups().GetAsync(_options.ResourceGroupName, cancellationToken);
            var cosmosAccount = await resourceGroup.Value.GetCosmosDBAccounts().GetAsync(_options.AccountName, cancellationToken);

            // Best-effort: if the MI doesn't have ARM permissions, this is swallowed and data-plane below is authoritative.
            // NOTE: ARM provisioning is best-effort. If it fails (permissions/RBAC), startup must still succeed via data-plane.
            var location = new AzureLocation(_options.Region);

            var dbInfo = new CosmosDBSqlDatabaseResourceInfo(_options.DatabaseName);
            await cosmosAccount.Value.GetCosmosDBSqlDatabases()
                .CreateOrUpdateAsync(
                    WaitUntil.Completed,
                    _options.DatabaseName,
                    new CosmosDBSqlDatabaseCreateOrUpdateContent(location, dbInfo),
                    cancellationToken);

            var containerPartitionKey = new CosmosDBSqlContainerPartitionKey
            {
                Paths = new[] { "/auditId" },
                Kind = CosmosDBSqlContainerPartitionKind.Hash,
            };

            var containerResourceInfo = new CosmosDBSqlContainerResourceInfo(_options.ContainerName)
            {
                PartitionKey = containerPartitionKey,
            };

            await cosmosAccount.Value.GetCosmosDBSqlDatabases()
                .Get(_options.DatabaseName)
                .Value
                .GetCosmosDBSqlContainers()
                .CreateOrUpdateAsync(
                    WaitUntil.Completed,
                    _options.ContainerName,
                    new CosmosDBSqlContainerCreateOrUpdateContent(location, containerResourceInfo),
                    cancellationToken);
        }
        catch
        {
            // Best-effort ARM provisioning: swallow errors.
        }

        // Data plane: must succeed for startup to proceed.
        var cosmosClient = new CosmosClient(_options.CosmosDbUri, credential);

        DatabaseResponse dbResponse = await cosmosClient.CreateDatabaseIfNotExistsAsync(
            _options.DatabaseName,
            cancellationToken: cancellationToken);

        var containerProperties = new ContainerProperties(_options.ContainerName, "/auditId");
        await dbResponse.Database.CreateContainerIfNotExistsAsync(
            containerProperties,
            cancellationToken: cancellationToken);
    }
}
