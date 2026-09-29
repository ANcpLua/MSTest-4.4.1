using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MSTest.Platform.Changed.Telemetry;

/// <summary>
/// A collector on a loopback port that keeps the path and the body of each request it receives. It stands
/// where the endpoint of an observability backend is, so that an example can see what the OTLP exporter of
/// a run sends.
/// </summary>
internal sealed class OtlpCollector : IAsyncDisposable
{
    private readonly HttpListener _listener = new();
    private readonly ConcurrentQueue<(string Path, string Body)> _requests = [];
    private readonly Task _receiving;

    public OtlpCollector()
    {
        using (var port = new TcpListener(IPAddress.Loopback, port: 0))
        {
            port.Start();
            Endpoint = $"http://{port.LocalEndpoint}";
        }

        _listener.Prefixes.Add($"{Endpoint}/");
        _listener.Start();
        _receiving = ReceiveAsync();
    }

    public string Endpoint { get; }

    public IReadOnlyCollection<(string Path, string Body)> Requests => _requests;

    public async ValueTask DisposeAsync()
    {
        _listener.Stop();
        await _receiving;
        _listener.Close();
    }

    private async Task ReceiveAsync()
    {
        while (_listener.IsListening)
        {
            HttpListenerContext exchange;
            try
            {
                exchange = await _listener.GetContextAsync();
            }
            catch (Exception stopped) when (stopped is HttpListenerException or ObjectDisposedException)
            {
                return;
            }

            using var body = new StreamReader(exchange.Request.InputStream, Encoding.Latin1);
            _requests.Enqueue((exchange.Request.Url!.AbsolutePath, await body.ReadToEndAsync()));
            exchange.Response.Close();
        }
    }
}
