using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using WowClient.Auth;
using WowClient.Endpoints;
using WowClient.Http;

namespace WowClient.Extensions;

/// <summary>
/// Dependency injection helpers for <see cref="IWowClient"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the WoW client, its options, and named HTTP clients.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <param name="configure">Options configuration callback.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddWowClient(
        this IServiceCollection services,
        Action<WowClientOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        services.AddOptions<WowClientOptions>().Configure(configure);
        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IValidateOptions<WowClientOptions>, WowClientOptionsValidator>());

        services.AddHttpClient(WowClientDefaults.ApiHttpClientName, ConfigureHttpClient);
        services.AddHttpClient(WowClientDefaults.OAuthHttpClientName, ConfigureHttpClient);

        services.TryAddSingleton(TimeProvider.System);

        services.TryAddSingleton<IAccessTokenProvider>(provider => new ClientCredentialsTokenProvider(
            provider.GetRequiredService<IHttpClientFactory>(),
            provider.GetRequiredService<IOptions<WowClientOptions>>().Value,
            provider.GetRequiredService<TimeProvider>()));

        services.TryAddSingleton(provider => new WowApiRequestExecutor(
            provider.GetRequiredService<IHttpClientFactory>(),
            provider.GetRequiredService<IAccessTokenProvider>(),
            provider.GetRequiredService<IOptions<WowClientOptions>>().Value,
            provider.GetRequiredService<TimeProvider>()));

        services.TryAddSingleton<IGameDataClient>(provider => new GameDataClient(
            provider.GetRequiredService<WowApiRequestExecutor>(),
            provider.GetRequiredService<IOptions<WowClientOptions>>().Value));

        services.TryAddSingleton<IProfileClient>(provider => new ProfileClient(
            provider.GetRequiredService<WowApiRequestExecutor>(),
            provider.GetRequiredService<IOptions<WowClientOptions>>().Value));

        services.TryAddSingleton<IWowClient>(provider => new WowClient(
            provider.GetRequiredService<IGameDataClient>(),
            provider.GetRequiredService<IProfileClient>(),
            provider.GetRequiredService<WowApiRequestExecutor>(),
            provider.GetRequiredService<IOptions<WowClientOptions>>().Value));

        return services;
    }

    private static void ConfigureHttpClient(IServiceProvider provider, HttpClient client) =>
        client.Timeout = provider.GetRequiredService<IOptions<WowClientOptions>>().Value.Timeout;
}
