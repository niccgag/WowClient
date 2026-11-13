namespace WowClient.Auth;

internal sealed record AccessToken(string Value, DateTimeOffset ExpiresAt);
