using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json.Nodes;
using MSTest.Showcase;

namespace MSTest.Platform.Changed.ServerMode;

/// <summary>
///     One run of this test application in server mode, with the example as its client. The client listens on a
///     loopback port and starts the application with <c>--server jsonrpc</c>, <c>--client-host</c> and
///     <c>--client-port</c>. The application connects to that port, and both sides exchange JSON-RPC messages,
///     each after a <c>Content-Length</c> header with the number of its UTF-8 bytes and an empty line.
/// </summary>
internal sealed class ServerModeSession : IAsyncDisposable
{
    private const string ContentLength = "Content-Length:";
    private const string Exit = """{ "jsonrpc": "2.0", "method": "exit", "params": {} }""";

    private readonly Process _application;
    private readonly TcpClient _connection;
    private readonly Stream _incoming;
    private readonly List<JsonNode> _notifications = [];

    private ServerModeSession(Process application, TcpClient connection)
    {
        _application = application;
        _connection = connection;
        _incoming = new BufferedStream(connection.GetStream());
    }

    /// <summary>The messages without the identifier of a request that arrived while a request was answered.</summary>
    public IReadOnlyList<JsonNode> Notifications => _notifications;

    public async ValueTask DisposeAsync()
    {
        await SendAsync(Exit, CancellationToken.None);
        await _application.StandardOutput.ReadToEndAsync();
        await _application.WaitForExitAsync();
        _application.Dispose();
        await _incoming.DisposeAsync();
        _connection.Dispose();
    }

    public static async Task<ServerModeSession> StartAsync(CancellationToken cancellationToken,
        params string[] arguments)
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port.ToString(CultureInfo.InvariantCulture);
        var start = new ProcessStartInfo(
            Environment.ProcessPath!,
            [
                "--server", "jsonrpc", "--client-host", IPAddress.Loopback.ToString(), "--client-port", port,
                .. arguments
            ])
        {
            RedirectStandardOutput = true,
            Environment =
            {
                ["TESTINGPLATFORM_TELEMETRY_OPTOUT"] = "1",
                [ExplicitAttribute.Variable] = "1"
            }
        };

        var application = Process.Start(start)!;
        try
        {
            return new ServerModeSession(application, await listener.AcceptTcpClientAsync(cancellationToken));
        }
        catch
        {
            application.Kill();
            throw;
        }
    }

    /// <summary>Sends a request and returns the message that answers it: its result or its error.</summary>
    public async Task<JsonNode> RequestAsync(string request, CancellationToken cancellationToken)
    {
        var identifier = JsonNode.Parse(request)!["id"]!.ToJsonString();
        await SendAsync(request, cancellationToken);

        while (true)
        {
            var message = await ReceiveAsync(cancellationToken);
            if (message["id"]?.ToJsonString() == identifier) return message;

            _notifications.Add(message);
        }
    }

    private async Task SendAsync(string message, CancellationToken cancellationToken)
    {
        var body = Encoding.UTF8.GetBytes(message);
        var header = Encoding.ASCII.GetBytes($"{ContentLength} {body.Length}\r\n\r\n");

        await _connection.GetStream().WriteAsync((byte[])[.. header, .. body], cancellationToken);
    }

    private async Task<JsonNode> ReceiveAsync(CancellationToken cancellationToken)
    {
        var length = 0;
        while (await ReadHeaderAsync(cancellationToken) is { Length: > 0 } header)
            if (header.StartsWith(ContentLength, StringComparison.OrdinalIgnoreCase))
                length = int.Parse(header[ContentLength.Length..], CultureInfo.InvariantCulture);

        var body = new byte[length];
        await _incoming.ReadExactlyAsync(body, cancellationToken);

        return JsonNode.Parse(body)!;
    }

    private async Task<string> ReadHeaderAsync(CancellationToken cancellationToken)
    {
        var line = new StringBuilder();
        var next = new byte[1];
        while (true)
        {
            await _incoming.ReadExactlyAsync(next, cancellationToken);
            if (next[0] == '\n') return line.ToString().TrimEnd('\r');

            line.Append((char)next[0]);
        }
    }
}