using JamieMagee.WindowsContainerScan.Utils;
using Microsoft.Rest;
using Valleysoft.DockerCredsProvider;
using Valleysoft.DockerRegistryClient;

namespace JamieMagee.WindowsContainerScan.Services;

internal class ContainerRegistryClientFactory : IContainerRegistryClientFactory
{
    public async Task<IContainerRegistryClient> GetClientAsync(string? registry)
    {
        ServiceClientCredentials? clientCredentials;

        DockerCredentials credentials;
        try
        {
            credentials = await CredsProvider.GetCredentialsAsync(DockerHubHelper.GetAuthRegistry(registry));
        }
        catch (Exception e) when (e is CredsNotFoundException or FileNotFoundException)
        {
            return new ContainerRegistryClientWrapper(CreateClient(DockerHubHelper.GetApiRegistry(registry)));
        }

        if (credentials.IdentityToken is not null)
        {
            clientCredentials = new TokenCredentials(credentials.IdentityToken);
        }
        else
        {
            clientCredentials = new BasicAuthenticationCredentials
            {
                UserName = credentials.Username,
                Password = credentials.Password
            };
        }

        return new ContainerRegistryClientWrapper(CreateClient(registry, clientCredentials));
    }

    private RegistryClient CreateClient(string? registry, ServiceClientCredentials? clientCredentials = null)
    {
        var client = new RegistryClient(DockerHubHelper.GetApiRegistry(registry), clientCredentials);
        client.HttpClient.Timeout = new TimeSpan(0, 30, 0);
        return client;
    }
}