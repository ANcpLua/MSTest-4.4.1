using Microsoft.Testing.Platform.CommandLine;
using Microsoft.Testing.Platform.Configurations;

namespace MSTest.Platform.Configuration;

/// <summary>
/// A configuration source of the application: the defaults of the carrier, kept in code.
/// <see cref="IHierarchicalConfigurationProvider"/> makes its keys appear among the children of a section:
/// <c>TryGetScalar</c> answers for a key that has a value and <c>GetChildKeys</c> names the keys below a path,
/// where <see langword="null"/> stands for the root. Sources are asked in the order of <see cref="Order"/>.
/// The command line has 0, the environment variables 1 and the <c>testconfig.json</c> 3, so a default with 4
/// gives way to all of them. <c>Program.cs</c> registers the source with
/// <c>builder.Configuration.AddConfigurationSource</c>.
/// </summary>
internal sealed class CarrierDefaults : IConfigurationSource, IHierarchicalConfigurationProvider
{
    private static readonly Dictionary<string, string> Defaults = new(StringComparer.OrdinalIgnoreCase)
    {
        ["showcase:carrier:name"] = "Nordpost",
        ["showcase:carrier:pickup"] = "17:00",
    };

    public string Uid => nameof(CarrierDefaults);

    public string Version => "1.0.0";

    public string DisplayName => "Carrier defaults";

    public string Description => "Supplies the defaults of the carrier.";

    public int Order => 4;

    public Task<bool> IsEnabledAsync() => Task.FromResult(true);

    public Task<IConfigurationProvider> BuildAsync(CommandLineParseResult commandLineParseResult) =>
        Task.FromResult<IConfigurationProvider>(this);

    public Task LoadAsync() => Task.CompletedTask;

    public bool TryGet(string key, out string? value) => TryGetScalar(key, out value);

    public bool TryGetScalar(string key, out string? value)
    {
        var found = Defaults.TryGetValue(key, out var text);
        value = text;
        return found;
    }

    public IEnumerable<string> GetChildKeys(string? parentPath)
    {
        var prefix = parentPath is null ? string.Empty : $"{parentPath}:";

        return Defaults.Keys
            .Where(key => key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            .Select(key => key[prefix.Length..].Split(':')[0])
            .Distinct(StringComparer.OrdinalIgnoreCase);
    }
}
