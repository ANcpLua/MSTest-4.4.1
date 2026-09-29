namespace MSTest.Platform.Changed.ServerMode;

/// <summary>
/// Client and server agree on the version of the protocol during the initialization, apart from the versions
/// of their products. The client names the versions it supports in <c>protocolVersions</c>, and the server
/// answers with the one it selects in <c>protocolVersion</c>. A client that names no version gets
/// <c>1.0.0</c>. A client that names only versions the server does not know gets the error <c>-31699</c>
/// and can initialize the connection again.
/// </summary>
[TestClass]
public sealed class ProtocolVersionRunTests(TestContext testContext)
{
    private const int ProtocolVersionNotSupported = -31699;

    private const string Initialize =
        """
        {
            "jsonrpc": "2.0",
            "id": 1,
            "method": "initialize",
            "params": {
                "processId": 1,
                "clientInfo": { "name": "showcase", "version": "7.0.0" },
                "capabilities": { "testing": { "debuggerProvider": false } }
                VERSIONS
            }
        }
        """;

    [TestMethod]
    [DataRow("", DisplayName = "Server_selects_1.0.0_for_a_client_that_names_no_versions")]
    [DataRow(""", "protocolVersions": null""", DisplayName = "Server_selects_1.0.0_for_a_client_that_names_null")]
    [DataRow(""", "protocolVersions": []""", DisplayName = "Server_selects_1.0.0_for_a_client_that_names_an_empty_list")]
    [DataRow(""", "protocolVersions": ["1.0.0"]""", DisplayName = "Server_selects_1.0.0_for_a_client_that_names_1.0.0")]
    [DataRow(""", "protocolVersions": ["99.0.0", "1.0.0"]""", DisplayName = "Server_selects_1.0.0_for_a_client_that_names_99.0.0_and_1.0.0")]
    public async Task Server_selects_the_version_of_the_protocol(string versions)
    {
        await using var server = await ServerModeSession.StartAsync(testContext.CancellationToken);

        var answer = await server.RequestAsync(Initialize.Replace("VERSIONS", versions), testContext.CancellationToken);

        Assert.AreEqual("1.0.0", (string?)answer["result"]!["protocolVersion"]);
        Assert.AreEqual("2.4.1", (string?)answer["result"]!["serverInfo"]!["version"]);
    }

    [TestMethod]
    public async Task Server_rejects_a_client_without_a_common_version_and_accepts_its_next_attempt()
    {
        await using var server = await ServerModeSession.StartAsync(testContext.CancellationToken);

        var rejection = await server.RequestAsync(
            Initialize.Replace("VERSIONS", """, "protocolVersions": ["99.0.0"]"""),
            testContext.CancellationToken);
        var answer = await server.RequestAsync(
            Initialize.Replace("VERSIONS", """, "protocolVersions": ["1.0.0"]"""),
            testContext.CancellationToken);

        Assert.AreEqual(ProtocolVersionNotSupported, (int)rejection["error"]!["code"]!);
        Assert.AreEqual(
            "None of the client's protocol versions are supported. Server versions: 1.0.0.",
            (string?)rejection["error"]!["message"]);
        Assert.AreEqual("1.0.0", (string?)answer["result"]!["protocolVersion"]);
    }
}
