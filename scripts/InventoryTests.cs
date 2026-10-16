#:package Microsoft.CodeAnalysis.CSharp@5.9.0
#:property PublishAot=false
using System.Text.Json;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

// This is a syntax inventory, not a grading engine. A human reviews every recorded body.
// Usage: dotnet run --file scripts/InventoryTests.cs -- . artifacts/test-inventory.json
var root = Path.GetFullPath(args[0]);
var destination = Path.GetFullPath(args[1]);
var paths = Directory.EnumerateDirectories(root)
    .Where(path => Path.GetFileName(path).EndsWith(" 4.5.1", StringComparison.Ordinal)
        || Path.GetFileName(path) == "Shared")
    .SelectMany(path => Directory.EnumerateFiles(path, "*.cs", SearchOption.AllDirectories).Where(file => !file.Contains("/bin/") && !file.Contains("/obj/")))
    .Order(StringComparer.Ordinal).ToArray();
var trees = paths.Select(path => (Path: path, Tree: CSharpSyntaxTree.ParseText(File.ReadAllText(path)))).ToArray();
var methodAttributes = new HashSet<string>(StringComparer.Ordinal)
    { "TestMethod", "TestMethodAttribute", "STATestMethod", "STATestMethodAttribute" };
foreach (var (_, tree) in trees)
foreach (var type in tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>())
{
    if (type.BaseList?.Types.Any(type => type.Type.ToString().EndsWith("TestMethodAttribute", StringComparison.Ordinal)) != true) continue;
    methodAttributes.Add(type.Identifier.ValueText);
    methodAttributes.Add(type.Identifier.ValueText.EndsWith("Attribute", StringComparison.Ordinal) ? type.Identifier.ValueText[..^9] : type.Identifier.ValueText);
}
var methods = new List<object>();
var fixtures = new List<object>();
foreach (var (path, tree) in trees)
foreach (var type in tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>())
{
    var ns = type.Ancestors().OfType<BaseNamespaceDeclarationSyntax>().FirstOrDefault()?.Name.ToString() ?? "";
    var nestedName = string.Join(".", type.AncestorsAndSelf().OfType<TypeDeclarationSyntax>().Reverse().Select(owner => owner.Identifier.ValueText + owner.TypeParameterList));
    var className = $"{ns}.{nestedName}";
    var classAttributes = type.AttributeLists.SelectMany(list => list.Attributes).Select(attribute => attribute.ToString()).ToArray();
    var classMethods = type.Members.OfType<MethodDeclarationSyntax>().ToArray();
    var lifecycle = classMethods.Where(method => method.AttributeLists.SelectMany(list => list.Attributes)
        .Any(attribute => new[] { "ClassInitialize", "ClassCleanup", "AssemblyInitialize", "AssemblyCleanup", "GlobalTestInitialize", "GlobalTestCleanup", "TestInitialize", "TestCleanup" }
            .Contains(attribute.Name.ToString().Replace("Attribute", "", StringComparison.Ordinal))))
        .Select(method => new { name = method.Identifier.ValueText, body = method.ToString() }).ToArray();
    if (lifecycle.Length > 0)
        fixtures.Add(new { file = Path.GetRelativePath(root, path), @class = className,
            line = type.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
            attributes = classAttributes, lifecycle });
    foreach (var method in classMethods)
    {
        var attributes = method.AttributeLists.SelectMany(list => list.Attributes).ToArray();
        if (!attributes.Any(attribute => methodAttributes.Contains(attribute.Name.ToString()))) continue;
        methods.Add(new { file = Path.GetRelativePath(root, path), @class = className,
            method = method.Identifier.ValueText,
            line = method.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
            classAttributes,
            attributes = attributes.Select(attribute => attribute.ToString()).ToArray(),
            body = method.ToString() });
    }
}
Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
File.WriteAllText(destination, JsonSerializer.Serialize(new { methods, fixtures }, new JsonSerializerOptions { WriteIndented = true }));
Console.WriteLine($"Inventoried {methods.Count} test methods and {fixtures.Count} lifecycle fixtures from {paths.Length} source files.");
