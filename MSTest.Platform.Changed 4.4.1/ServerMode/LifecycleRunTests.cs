using System.Text.Json.Nodes;

namespace MSTest.Platform.Changed.ServerMode;

/// <summary>
/// A server answers a request whose identifier is a number, as it is or as a numeric text, with a result or
/// with an error that has a code, and the identifier comes back as it was sent.
/// <c>initialize</c> is the first request of a connection: a request before it has the error
/// <c>-32002</c>, and a second <c>initialize</c> has the error <c>-32600</c>. After it, a method that the
/// server does not know has the error <c>-32601</c> and a request with a parameter that is not valid has the
/// error <c>-32602</c>. A request whose identifier is not numeric, such as <c>"restock-7"</c>, gets no
/// answer: the server ends with an unhandled exception.
/// </summary>
[TestClass]
public sealed class LifecycleRunTests(TestContext testContext)
{
    private const int ServerNotInitialized = -32002;
    private const int InvalidRequest = -32600;
    private const int MethodNotFound = -32601;
    private const int InvalidParams = -32602;

    private const string Initialize =
        """
        {
            "jsonrpc": "2.0",
            "id": 1,
            "method": "initialize",
            "params": {
                "processId": 1,
                "clientInfo": { "name": "showcase", "version": "1.0.0" },
                "capabilities": { "testing": { "debuggerProvider": false } }
            }
        }
        """;

    [TestMethod]
    public async Task Request_before_the_initialization_is_rejected()
    {
        await using var server = await ServerModeSession.StartAsync(testContext.CancellationToken);

        var answer = await server.RequestAsync(
            """
            {
                "jsonrpc": "2.0",
                "id": 1,
                "method": "testing/discoverTests",
                "params": { "runId": "00000000-0000-0000-0000-000000000001" }
            }
            """,
            testContext.CancellationToken);

        Assert.AreEqual(ServerNotInitialized, (int)answer["error"]!["code"]!);
        Assert.AreEqual("The server must be initialized before this request can be processed.", (string?)answer["error"]!["message"]);
    }

    [TestMethod]
    public async Task Initialization_is_answered_with_the_server_and_its_capabilities()
    {
        await using var server = await ServerModeSession.StartAsync(testContext.CancellationToken);

        var answer = await server.RequestAsync(Initialize, testContext.CancellationToken);

        Assert.AreEqual(1, (int)answer["id"]!);
        Assert.AreEqual("2.5.1", (string?)answer["result"]!["serverInfo"]!["version"]);
        Assert.IsTrue((bool)answer["result"]!["capabilities"]!["testing"]!["supportsDiscovery"]!);
    }

    [TestMethod]
    public async Task Second_initialization_is_rejected()
    {
        await using var server = await InitializedServerAsync();

        var answer = await server.RequestAsync(Initialize.Replace("\"id\": 1", "\"id\": 2"), testContext.CancellationToken);

        Assert.AreEqual(InvalidRequest, (int)answer["error"]!["code"]!);
        Assert.AreEqual("The server has already received an initialize request.", (string?)answer["error"]!["message"]);
    }

    [TestMethod]
    public async Task Unknown_method_is_rejected_under_the_identifier_of_the_request()
    {
        await using var server = await InitializedServerAsync();

        var answer = await server.RequestAsync(
            """{ "jsonrpc": "2.0", "id": "7", "method": "testing/restock", "params": {} }""",
            testContext.CancellationToken);

        Assert.AreEqual("7", (string?)answer["id"]);
        Assert.AreEqual(MethodNotFound, (int)answer["error"]!["code"]!);
        Assert.AreEqual("The method 'testing/restock' is not supported.", (string?)answer["error"]!["message"]);
    }

    [TestMethod]
    public async Task Run_without_a_valid_identifier_is_rejected()
    {
        await using var server = await InitializedServerAsync();

        var answer = await server.RequestAsync(
            """{ "jsonrpc": "2.0", "id": 2, "method": "testing/runTests", "params": { "runId": "first run" } }""",
            testContext.CancellationToken);

        Assert.AreEqual(InvalidParams, (int)answer["error"]!["code"]!);
        Assert.AreEqual("'runId' field is not a valid Guid", (string?)answer["error"]!["message"]);
    }

    private async Task<ServerModeSession> InitializedServerAsync()
    {
        var server = await ServerModeSession.StartAsync(testContext.CancellationToken);
        JsonNode answer = await server.RequestAsync(Initialize, testContext.CancellationToken);

        Assert.IsNotNull(answer["result"]);

        return server;
    }
}
