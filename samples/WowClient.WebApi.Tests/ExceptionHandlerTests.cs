using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using WowClient.Exceptions;
using WowClient.WebApi.Errors;

namespace WowClient.WebApi.Tests;

public sealed class ExceptionHandlerTests
{
    [Fact]
    public async Task ProtocolExceptionIsTranslatedToBadGatewayProblem()
    {
        var handler = new WowApiExceptionHandler(NullLogger<WowApiExceptionHandler>.Instance);
        var httpContext = CreateContext();
        var requestUri = new Uri("https://api.test/data/wow/item/19019");
        var exception = new WowProtocolException(
            "The API returned an empty response body.",
            requestUri);

        var handled = await handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status502BadGateway, httpContext.Response.StatusCode);

        using var problem = await ReadProblemAsync(httpContext);
        Assert.Equal(502, problem.RootElement.GetProperty("status").GetInt32());
        Assert.Equal(
            "Blizzard API returned an invalid response",
            problem.RootElement.GetProperty("title").GetString());
        Assert.Equal(
            "The API returned an empty response body.",
            problem.RootElement.GetProperty("detail").GetString());
        Assert.Equal(requestUri.ToString(), problem.RootElement.GetProperty("requestUri").GetString());
    }

    [Fact]
    public async Task UnknownExceptionsAreLeftToTheDefaultHandler()
    {
        var handler = new WowApiExceptionHandler(NullLogger<WowApiExceptionHandler>.Instance);
        var httpContext = CreateContext();

        var handled = await handler.TryHandleAsync(
            httpContext,
            new InvalidOperationException("boom"),
            CancellationToken.None);

        Assert.False(handled);
        Assert.Equal(StatusCodes.Status200OK, httpContext.Response.StatusCode);
    }

    private static DefaultHttpContext CreateContext()
    {
        var httpContext = new DefaultHttpContext
        {
            RequestServices = new ServiceCollection()
                .AddLogging()
                .AddProblemDetails()
                .BuildServiceProvider(),
        };
        httpContext.Response.Body = new MemoryStream();
        return httpContext;
    }

    private static async Task<JsonDocument> ReadProblemAsync(HttpContext httpContext)
    {
        httpContext.Response.Body.Position = 0;
        return await JsonDocument.ParseAsync(httpContext.Response.Body);
    }
}
