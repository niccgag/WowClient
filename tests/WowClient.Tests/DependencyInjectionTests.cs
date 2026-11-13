using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using WowClient.Auth;
using WowClient.Endpoints;
using WowClient.Exceptions;
using WowClient.Extensions;

namespace WowClient.Tests;

public sealed class DependencyInjectionTests
{
    [Fact]
    public void AddWowClientRegistersClientOptionsAndHttpClients()
    {
        var services = new ServiceCollection();
        services.AddWowClient(options =>
        {
            options.ClientId = "id";
            options.ClientSecret = "secret";
            options.Region = WowRegion.Eu;
            options.Locale = WowLocale.GermanDe;
        });

        using var provider = services.BuildServiceProvider();

        var client = provider.GetRequiredService<IWowClient>();
        Assert.NotNull(client.GameData);
        Assert.NotNull(client.Profile);
        Assert.Same(client.GameData, provider.GetRequiredService<IGameDataClient>());
        Assert.Same(client.Profile, provider.GetRequiredService<IProfileClient>());

        var options = provider.GetRequiredService<IOptions<WowClientOptions>>().Value;
        Assert.Equal(WowRegion.Eu, options.Region);
        Assert.Equal(WowLocale.GermanDe, options.Locale);

        var httpClient = provider.GetRequiredService<IHttpClientFactory>().CreateClient("WowClient.Api");
        Assert.Equal(TimeSpan.FromSeconds(30), httpClient.Timeout);
    }

    [Fact]
    public void CustomTokenProviderRegisteredBeforeAddWowClientWins()
    {
        var services = new ServiceCollection();
        var stub = new StubTokenProvider();
        services.AddSingleton<IAccessTokenProvider>(stub);
        services.AddWowClient(_ => { });

        using var provider = services.BuildServiceProvider();

        Assert.Same(stub, provider.GetRequiredService<IAccessTokenProvider>());
        Assert.NotNull(provider.GetRequiredService<IWowClient>());
    }

    [Fact]
    public async Task MissingCredentialsThrowWhenTokenIsRequested()
    {
        var services = new ServiceCollection();
        services.AddWowClient(_ => { });

        using var provider = services.BuildServiceProvider();
        var tokenProvider = provider.GetRequiredService<IAccessTokenProvider>();

        await Assert.ThrowsAsync<WowAuthenticationException>(
            () => tokenProvider.GetAccessTokenAsync().AsTask());
    }

    [Fact]
    public void NonPositiveTimeoutFailsOptionsValidation()
    {
        var services = new ServiceCollection();
        services.AddWowClient(options => options.Timeout = TimeSpan.Zero);

        using var provider = services.BuildServiceProvider();

        Assert.Throws<OptionsValidationException>(
            () => provider.GetRequiredService<IOptions<WowClientOptions>>().Value);
    }

    private sealed class StubTokenProvider : IAccessTokenProvider
    {
        public ValueTask<string> GetAccessTokenAsync(CancellationToken cancellationToken = default) =>
            ValueTask.FromResult("stub-token");
    }
}
