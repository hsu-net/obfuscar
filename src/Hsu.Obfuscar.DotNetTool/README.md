# Hsu.Obfuscar.DotNetTool
[![dev](https://github.com/hsu-net/obfuscar/actions/workflows/build.yml/badge.svg?branch=dev)](https://github.com/hsu-net/obfuscar/actions/workflows/build.yml)
[![preview](https://github.com/hsu-net/obfuscar/actions/workflows/deploy.yml/badge.svg?branch=preview)](https://github.com/hsu-net/obfuscar/actions/workflows/deploy.yml)
[![main](https://github.com/hsu-net/obfuscar/actions/workflows/deploy.yml/badge.svg?branch=main)](https://github.com/hsu-net/obfuscar/actions/workflows/deploy.yml)
[![Obfuscar](https://img.shields.io/badge/Obfuscar-GlobalTool-yellow.svg)](https://github.com/obfuscar/obfuscar)
[![Nuke Build](https://img.shields.io/badge/Nuke-Build-yellow.svg)](https://github.com/nuke-build/nuke)
![windows](https://img.shields.io/badge/OS-Windows-blue.svg)
![linux](https://img.shields.io/badge/OS-Linux-blue.svg)

Use [Obfuscar.GlobalTool](https://www.nuget.org/packages/Obfuscar.GlobalTool) to obfuscate .NET code.

## Package Version

| Name | Source | Stable | Preview |
|---|---|---|---|
| Hsu.Obfuscar.DotNetTool | Nuget | [![NuGet](https://img.shields.io/nuget/v/Hsu.Obfuscar.DotNetTool?style=flat-square)](https://www.nuget.org/packages/Hsu.Obfuscar.DotNetTool) | [![NuGet](https://img.shields.io/nuget/vpre/Hsu.Obfuscar.DotNetTool?style=flat-square)](https://www.nuget.org/packages/Hsu.Obfuscar.DotNetTool) |
| Hsu.Obfuscar.DotNetTool | MyGet | [![MyGet](https://img.shields.io/myget/godsharp/v/Hsu.Obfuscar.DotNetTool?style=flat-square&label=myget)](https://www.myget.org/feed/godsharp/package/nuget/Hsu.Obfuscar.DotNetTool) | [![MyGet](https://img.shields.io/myget/godsharp/vpre/Hsu.Obfuscar.DotNetTool?style=flat-square&label=myget)](https://www.myget.org/feed/godsharp/package/nuget/Hsu.Obfuscar.DotNetTool) |

## Usage

### Install Obfuscar.GlobalTool

Global installation:
```bash
dotnet tool install --global Obfuscar.GlobalTool --version 2.2.38
```

Local installation:
```bash
dotnet new tool-manifest # if you are setting up this repo
dotnet tool install --local Obfuscar.GlobalTool --version 2.2.38
```

### Install Hsu.Obfuscar.DotNetTool

See [NuGet](https://www.nuget.org/packages/Hsu.Obfuscar.DotNetTool).

### Configuration

- Add `AssemblySearchPath` file:
```csharp
    <PropertyGroup>
        <ObfuscarAssembliesInclude>true</ObfuscarAssembliesInclude>
    </PropertyGroup>
```

When you build the project, the `obfuscar.xml` file and `obfuscar.assemblies[-fw].xml` will be created to the `.obfuscar` folder.

- Default `obfuscar.xml` contents
```xml
<?xml version="1.0"?>
<Obfuscator>
  <!-- https://docs.obfuscar.com/getting-started/configuration -->
  <!-- https://github.com/obfuscar/obfuscar/blob/master/Obfuscar/Settings.cs -->
  
  <Var name="InPath" value="." />
  <Var name="OutPath" value=".\obfuscated" />
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

  <Module file="ConsoleSample.dll">
    <SkipEnums value="true" />
  </Module>
  
  <!-- <Include path="obfuscar.assemblies.xml" /> -->
  <Include path="obfuscar.assemblies.xml" />
</Obfuscator>
```

- Default `obfuscar.assemblies[-fw].xml` contents
```xml
<?xml version="1.0"?>
<Include>
    <AssemblySearchPath path='.' />
</Include>
```

You can also modify the `obfuscar.xml` and `obfuscar.assemblies[-fw].xml` file to change the default values.

### Generated files

- Single framework

```config
-- .obfuscar
  |-- obfuscar.xml
  |-- obfuscar.assemblies.xml
```

- Multiple framework
```config
-- .obfuscar
  |-- obfuscar.xml
  |-- obfuscar.assemblies-net462.xml
  |-- obfuscar.assemblies-net6.0.xml
  |-- obfuscar.assemblies-net8.0.xml
```

## License
MIT