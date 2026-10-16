using System.Collections.Concurrent;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace MSTest.Platform.Changed.Telemetry;

internal sealed class OtlpCollector(WebApplication application,
    ConcurrentQueue<(string Path, byte[] Body)> requests) : IAsyncDisposable
{
    public string Endpoint => application.Urls.Single();
    public IReadOnlyCollection<(string Path, byte[] Body)> Requests => requests;

    public static async Task<OtlpCollector> StartAsync(CancellationToken cancellationToken)
    {
        var builder = WebApplication.CreateSlimBuilder();
        builder.Logging.ClearProviders();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        var application = builder.Build();
        ConcurrentQueue<(string Path, byte[] Body)> requests = new();
        application.Run(async context =>
        {
            using MemoryStream body = new();
            await context.Request.Body.CopyToAsync(body, context.RequestAborted);
            requests.Enqueue((context.Request.Path.Value!, body.ToArray()));
            context.Response.StatusCode = StatusCodes.Status200OK;
        });
        await application.StartAsync(cancellationToken);
        return new OtlpCollector(application, requests);
    }

    public async ValueTask DisposeAsync()
    {
        await application.StopAsync(CancellationToken.None);
        await application.DisposeAsync();
    }
}
