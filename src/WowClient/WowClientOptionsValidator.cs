using Microsoft.Extensions.Options;

namespace WowClient;

/// <summary>Validates <see cref="WowClientOptions"/> when they are resolved from dependency injection.</summary>
internal sealed class WowClientOptionsValidator : IValidateOptions<WowClientOptions>
{
    public ValidateOptionsResult Validate(string? name, WowClientOptions options)
    {
        var errors = options.Validate();

        return errors.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(errors);
    }
}
