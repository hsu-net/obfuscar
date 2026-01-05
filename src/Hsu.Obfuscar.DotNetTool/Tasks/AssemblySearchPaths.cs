using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

public sealed class GetAssemblySearchPaths : Task
{
    [Required] public string TargetAssembly { get; set; }
    [Required] public string ReferencePaths { get; set; }
    [Required] public string OutputFile { get; set; }

    public override bool Execute()
    {
        Log.LogMessage(MessageImportance.Normal, "Generating Obfuscar `AssemblySearchPaths` ...");
        var paths = ReferencePaths
            .Split(';')
            .ToDictionary(Path.GetFileNameWithoutExtension, Path.GetDirectoryName);

        var assembly = Assembly.LoadFrom(TargetAssembly);
        var assemblies = assembly.GetReferencedAssemblies();

        var builder = new StringBuilder("<?xml version=\"1.0\"?>");
        builder.AppendLine("  <Include>");
        var counter = 0;
        foreach(var item in assemblies)
        {
            try
            {
                var asm = Assembly.Load(item);
            }
            catch (FileNotFoundException)
            {
                if (!paths.TryGetValue(item.Name, out var path)) continue;
                builder.AppendLine($"    <AssemblySearchPath path='{path}' />");
                counter++;
            }
        }

        builder.AppendLine("  </Include>");
        File.WriteAllText(OutputFile, builder.ToString());

        Log.LogMessage(MessageImportance.Normal, $"Generated {counter} Obfuscar `AssemblySearchPaths`");
        return true;
    }
}
