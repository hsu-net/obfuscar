using System;
using NuGet.Versioning;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Utilities.Collections;
using Serilog;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
// ReSharper disable VariantPathSeparationHighlighting
// ReSharper disable StringLiteralTypo
// ReSharper disable AllUnderscoreLocalParameterName

#pragma warning disable S3903

[ShutdownDotNetAfterServerBuild]
partial class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode
    public static int Main() => Execute<Build>(x => x.Artifacts);

    const string DevelopBranch = "dev";
    const string PreviewBranch = "preview";
    const string MainBranch = "main";

    string Version;

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")] readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Parameter("Api key to push packages to nuget.org.")] [Secret] string NuGetApiKey;

    [Parameter("Api key to push packages to myget.org.")] [Secret] string MyGetApiKey;

    [Solution] readonly Solution Solution;
    [GitRepository] readonly GitRepository Repository;

    AbsolutePath SourceDirectory => RootDirectory / "src";
    AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";

    [LatestNuGetVersion("Hsu.Obfuscar.DotNetTool", IncludePrerelease = false)] readonly NuGetVersion NuGetVersion;

    protected override void OnBuildInitialized()
    {
        base.OnBuildInitialized();
        NuGetApiKey ??= Environment.GetEnvironmentVariable(nameof(NuGetApiKey));
        MyGetApiKey ??= Environment.GetEnvironmentVariable(nameof(MyGetApiKey));
    }

    Target Initial => _ => _
        .Description("Initial")
        .Executes(() =>
        {
            Log.Debug("{@NuGetVersion}", NuGetVersion);
            Version = $"{GetVersionPrefix()}{GetVersionSuffix()}";
        });

    Target Clean => _ => _
        .Description("Clean Solution")
        .DependsOn(Initial)
        .Executes(() =>
        {
            SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(x => x.DeleteDirectory());
            ArtifactsDirectory.CreateOrCleanDirectory();
        });
    
    Target Restore => _ => _
        .Description("Restore Solution")
        .DependsOn(Initial)
        .Executes(() =>
        {
            DotNetRestore(s => s
                .SetProjectFile(Solution)
            );
        });

    // Target Compile => _ => _
    //     .Description("Compile Projects")
    //     .DependsOn(Clean)
    //     .Executes(() =>
    //     {
    //         DotNetBuild(p => p
    //             .SetProjectFile(Solution)
    //             .SetConfiguration(Configuration)
    //             .SetVersion(Version)
    //             .EnableContinuousIntegrationBuild()
    //         );
    //     });

    Target Pack => _ => _
        .Description("Pack Projects")
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNetPack(p => p
                .SetProject(Solution)
                .SetConfiguration(Configuration)
                .SetVersion(Version)
                .SetNoBuild(true)
                .EnableContinuousIntegrationBuild()
            );
        });

    Target Artifacts => _ => _
        .DependsOn(Pack)
        .OnlyWhenStatic(() => IsServerBuild)
        .Description("Artifacts Upload")
        .Produces(ArtifactsDirectory / "**/*")
        .Executes(() =>
        {
            Log.Information("Artifacts uploaded.");
        });

    Target Push => _ => _
        .Description("Push NuGet Package")
        .OnlyWhenStatic(() => IsServerBuild && Configuration.Equals(Configuration.Release))
        .DependsOn(Pack)
        .Requires(() => NuGetApiKey)
        .Requires(() => MyGetApiKey)
        .Executes(() =>
        {
            ArtifactsDirectory
                .GlobFiles("**/*.nupkg", "**/*.snupkg")
                .ForEach(Nuget);
        });

    Target Deploy => _ => _
        .Description("Deploy")
        .DependsOn(Push, Artifacts, Release)
        .Executes(() =>
        {
            Log.Information("Deployed");
        });

    void Nuget(AbsolutePath x)
    {
        Nuget(x, "https://www.myget.org/F/godsharp/api/v3/index.json", MyGetApiKey);
        Nuget(x, "https://api.nuget.org/v3/index.json", NuGetApiKey);
    }

    void Nuget(string x, string source, string key) =>
        DotNetNuGetPush(s => s
            .SetTargetPath(x)
            .SetSource(source)
            .SetApiKey(key)
            .EnableSkipDuplicate()
            .EnableNoServiceEndpoint()
        );

    string GetVersionPrefix()
    {
        var dt = DateTimeNow();
        var main = $"{dt:yyyy}.{(dt.Month - 1) / 3 + 1}{dt:MM}.{dt:dd}";

        if (Repository.Branch == MainBranch && NuGetVersion != null && NuGetVersion.Version != null)
        {
            //Major=2023, Minor=307, Build=17, Revision=0
            var mmb = $"{NuGetVersion.Version.Major}.{NuGetVersion.Version.Minor}.{NuGetVersion.Version.Build:00}";
            if (mmb == main && NuGetVersion.Version.Revision != 0)
            {
                main = $"{main}.{NuGetVersion.Revision + 1}";
            }
        }

        return main;
    }

    string GetVersionSuffix()
    {
        return Repository.Branch?.ToLower() switch
        {
            MainBranch when IsServerBuild => null,
            _ => $"-rc{DateTimeNow():HHmmss}"
        };
    }

    static DateTimeOffset DateTimeNow()
    {
        return DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(8));
    }
}
