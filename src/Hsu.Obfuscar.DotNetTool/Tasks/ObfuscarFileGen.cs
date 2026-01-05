using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

public sealed class ObfuscarFileGen : Task
{
    [Required] public string TargetFileName { get; set; }
    [Required] public string ObfuscatedDir { get; set; }
    [Required] public string ObfuscarAssembliesName { get; set; }
    [Required] public string OutputFile { get; set; }

    public override bool Execute()
    {
        Log.LogMessage(MessageImportance.Normal, "Generate Obfuscar config file skipped");
        if (File.Exists(OutputFile)) return true;

        Log.LogMessage(MessageImportance.Normal, "Generating Obfuscar config file...");
        var template = $"""
                        <?xml version="1.0"?>
                        <Obfuscator>
                          <!-- https://docs.obfuscar.com/getting-started/configuration -->
                          <!-- https://github.com/obfuscar/obfuscar/blob/master/Obfuscar/Settings.cs -->
                          
                          <Var name="InPath" value="." />
                          <Var name="OutPath" value=".\{ObfuscatedDir}" />
                          <Var name="RenameFields" value="true" />
                          <Var name="RenameProperties" value="true" />
                          <Var name="RenameEvents" value="true" />
                          <Var name="KeepPublicApi" value="true" />
                          <Var name="HidePrivateApi" value="true" />
                          <Var name="ReuseNames" value="true" />
                          <Var name="UseUnicodeNames" value="false" />
                          <Var name="UseKoreanNames" value="false" />
                          <Var name="HideStrings" value="false" />
                          <Var name="OptimizeMethods" value="true" />
                          <Var name="SuppressIldasm" value="false" />
                        
                          <Module file="{TargetFileName}">
                            <SkipEnums value="true" />
                          </Module>
                          
                          <Include path="{ObfuscarAssembliesName}.xml" />
                        </Obfuscator>
                        """;
        File.WriteAllText(OutputFile, template);
        Log.LogMessage(MessageImportance.Normal, "Generated Obfuscar config file");

        return true;
    }
}
