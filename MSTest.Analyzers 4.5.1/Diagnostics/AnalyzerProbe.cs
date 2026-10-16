using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace MSTest.Analyzers.Diagnostics;

internal static class AnalyzerProbe
{
    private static readonly ImmutableArray<MetadataReference> References =
        [.. Directory.GetFiles(System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory(), "*.dll")
        .Concat([typeof(Assert).Assembly.Location, typeof(TestContext).Assembly.Location])
        .Distinct(StringComparer.Ordinal)
        .Select(path => MetadataReference.CreateFromFile(path))];

    public static async Task<ImmutableArray<Diagnostic>> RunAsync(
        string source, DiagnosticAnalyzer analyzer, CancellationToken cancellationToken)
    {
        var tree = CSharpSyntaxTree.ParseText(source, path: "Example.cs", cancellationToken: cancellationToken);
        var compilation = CSharpCompilation.Create("Example", [tree], References,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = compilation.GetDiagnostics(cancellationToken)
            .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ToArray();
        Assert.IsEmpty(errors, string.Join(Environment.NewLine, errors.Select(error => error.ToString())));
        return await compilation.WithAnalyzers([analyzer])
            .GetAnalyzerDiagnosticsAsync(cancellationToken);
    }
}
