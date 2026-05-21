using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

const string MappedFolderTemplateFile = "./mapped_folder.xml.tmpl";
const string SandboxTemplateFile = "./Containerize.wsb.tmpl";
const string MappingFileDirectory = "./directory_mappings";
const string FinalSandboxFile = "Containerize.wsb";

var mappedFolderTemplate = File.ReadAllText(MappedFolderTemplateFile);
var sandboxTemplate = File.ReadAllText(SandboxTemplateFile);

var deserializer = new DeserializerBuilder()
    .WithNamingConvention(CamelCaseNamingConvention.Instance)
    .Build();

var seenSandboxPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
var mappedFolderBlocks = new List<string>();

foreach (var yamlFile in Directory.EnumerateFiles(MappingFileDirectory))
{
    List<DirectoryMapping>? mappings;
    try
    {
        var yamlText = File.ReadAllText(yamlFile);
        mappings = deserializer.Deserialize<List<DirectoryMapping>>(yamlText);
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"Warning: Failed to parse {yamlFile}: {ex.Message}");
        continue;
    }

    if (mappings is null)
        continue;

    foreach (var entry in mappings)
    {
        if (string.IsNullOrWhiteSpace(entry.Sandbox))
        {
            Console.Error.WriteLine($"Warning: Empty sandbox path in {yamlFile}, skipping entry.");
            continue;
        }

        if (!seenSandboxPaths.Add(entry.Sandbox))
        {
            Console.Error.WriteLine($"Warning: Duplicate sandbox path skipped: {entry.Sandbox} in file {yamlFile}");
            continue;
        }

        var absHostPath = Path.GetFullPath(entry.Host);

        try
        {
            Directory.CreateDirectory(absHostPath);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Warning: Could not create directory {absHostPath}: {ex.Message}");
        }

        var block = mappedFolderTemplate
            .Replace("{{sandbox}}", entry.Sandbox)
            .Replace("{{host}}", absHostPath)
            .Replace("{{ro}}", entry.Ro ? "true" : "false");

        mappedFolderBlocks.Add(block);
    }
}

var combinedMappedFolders = string.Join("\n", mappedFolderBlocks);
var finalXml = sandboxTemplate.Replace("{{OtherMappedFolders}}", combinedMappedFolders);
File.WriteAllText(FinalSandboxFile, finalXml);

Console.WriteLine($"Generated {FinalSandboxFile} with {mappedFolderBlocks.Count} mapped folder(s).");

internal sealed class DirectoryMapping
{
    public string Sandbox { get; set; } = "";
    public string Host { get; set; } = "";
    public bool Ro { get; set; }
}
