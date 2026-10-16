using System.Collections.Concurrent;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MSTest.Hosting.Experimental;

internal static class TestHost
{
    public static Task<IHost> CreateHost()
    {
        var builder = Host.CreateApplicationBuilder();
        builder.Logging.ClearProviders();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?> { ["Greeting"] = "hello from the host" });
        builder.Services.AddMSTestTestClassInjection();
        builder.Services.AddSingleton<ScopeJournal>();
        builder.Services.AddScoped<InvocationState>();
        return Task.FromResult(builder.Build());
    }
}

public sealed class ScopeJournal
{
    public ConcurrentDictionary<Guid, string> Executions { get; } = new();
}

public sealed class InvocationState
{
    public Guid Id { get; } = Guid.NewGuid();
    public int Value { get; set; }
}
