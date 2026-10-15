using System.Text.Json;
using Microsoft.Testing.Platform.Extensions.ArtifactPostProcessing;

namespace MSTest.Platform.Artifacts;

/// <summary>
/// Combines outcome logs from distinct test modules into one JSON array. Inputs remain unchanged.
/// A truncated run can supply fewer complete logs; malformed or partially written JSON is rejected.
/// Retry attempts are not supported because combining their final outcomes requires a different policy.
/// </summary>
internal sealed class OutcomeLogMerger : IArtifactPostProcessor
{
    public string Uid => nameof(OutcomeLogMerger);
    public string Version => "1.0.0";
    public string DisplayName => "Merged outcome log";
    public string Description => "Combines outcome logs from distinct test modules.";
    public IReadOnlyList<ArtifactPostProcessingMode> SupportedModes => [ArtifactPostProcessingMode.TestModules];
    public bool SupportsTruncatedRuns => true;
    public IReadOnlyList<string> SupportedKinds => [OutcomeLog.Kind];
    public IReadOnlyList<string> SupportedFileExtensionsFallback => [];

    public Task<bool> IsEnabledAsync() => Task.FromResult(true);

    public async Task<ProcessedArtifact?> ProcessAsync(
        IReadOnlyList<InputArtifact> inputs,
        string outputDirectory,
        ArtifactPostProcessingContext context,
        CancellationToken cancellationToken)
    {
        if (inputs.Count == 0 || context.Mode != ArtifactPostProcessingMode.TestModules)
        {
            return null;
        }

        var outcomes = new List<OutcomeLog.Entry>();
        foreach (var input in inputs)
        {
            var json = await File.ReadAllTextAsync(input.Path, cancellationToken);
            var entries = JsonSerializer.Deserialize<OutcomeLog.Entry[]>(json)
                ?? throw new JsonException("An outcome log must contain a JSON array.");
            if (entries.Any(static entry => entry is null
                || string.IsNullOrEmpty(entry.Uid) || string.IsNullOrEmpty(entry.Name) || string.IsNullOrEmpty(entry.State)))
            {
                throw new JsonException("Each outcome must have an identity, a name and a state.");
            }

            outcomes.AddRange(entries);
        }

        Directory.CreateDirectory(outputDirectory);
        var path = Path.Combine(outputDirectory, OutcomeLog.FileName);
        var temporaryPath = Path.Combine(outputDirectory, Path.GetRandomFileName());
        var temporaryCreated = false;
        try
        {
            await using (var stream = new FileStream(temporaryPath, FileMode.CreateNew, FileAccess.Write, FileShare.None,
                bufferSize: 4096, FileOptions.Asynchronous))
            {
                temporaryCreated = true;
                await JsonSerializer.SerializeAsync(stream, outcomes, cancellationToken: cancellationToken);
            }

            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                File.Move(temporaryPath, path);
            }
            catch (IOException) when (File.Exists(path))
            {
                // A repeated merge can reuse its identical output. Never truncate an existing file:
                // it could also be an input reached through a symbolic link or a hard link.
                var existing = await File.ReadAllBytesAsync(path, cancellationToken);
                var merged = await File.ReadAllBytesAsync(temporaryPath, cancellationToken);
                if (!existing.AsSpan().SequenceEqual(merged))
                {
                    throw new IOException("The merged outcome log must not overwrite an existing artifact.");
                }
            }
        }
        finally
        {
            if (temporaryCreated)
            {
                File.Delete(temporaryPath);
            }
        }

        return new ProcessedArtifact(path, OutcomeLog.Kind, DisplayName, Description);
    }
}
