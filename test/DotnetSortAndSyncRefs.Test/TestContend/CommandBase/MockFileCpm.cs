namespace DotnetSortAndSyncRefs.Test.TestContend.CommandBase
{
    internal static class MockFileCpm
    {
        public static string GetDirectoryPackagesPropsSorted()
        {
            return
"""
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
  </PropertyGroup>
  <ItemGroup>
    <PackageVersion Include="LightInject" Version="7.0.1" />
    <PackageVersion Include="LightInject.Annotation" Version="1.1.0" />
    <PackageVersion Include="LightInject.Interception" Version="1.1.0" />
    <PackageVersion Include="McMaster.Extensions.CommandLineUtils" Version="4.1.1" />
    <PackageVersion Include="Spectre.IO" Version="0.12.0" />
    <PackageVersion Include="System.IO.Abstractions" Version="21.1.7" />
  </ItemGroup>
  <ItemGroup Condition="'$(TargetFramework)' == 'net8.0'">
    <PackageVersion Include="Microsoft.Extensions.DependencyInjection" Version="8.0.1" />
  </ItemGroup>
  <ItemGroup Condition="'$(TargetFramework)' == 'net7.0'">
    <PackageVersion Include="Microsoft.Extensions.DependencyInjection" Version="7.0.0" />
  </ItemGroup>
</Project>
""";
        }

        public static string GetDotnetResultUnsorted()
        {
            return
"""
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <Description>A .NET Core global tool to alphabetically sort package references in csproj or fsproj</Description>
    <VersionPrefix>3.0.0</VersionPrefix>
    <Authors>Babu Annamalai</Authors>
    <OutputType>Exe</OutputType>
    <TargetFrameworks>net8.0;net7.0;</TargetFrameworks>
    <AssemblyName>dotnet-sort-and-sync-refs</AssemblyName>
    <PackageId>dotnet-sort-and-sync-refs</PackageId>
    <PackageOutputPath>./nupkg</PackageOutputPath>
    <PackageProjectUrl>https://github.com/mysticmind/dotnet-sort-refs</PackageProjectUrl>
    <PackageLicenseExpression>MIT</PackageLicenseExpression>
    <IsPackable>true</IsPackable>
    <PackAsTool>true</PackAsTool>
    <ToolCommandName>dotnet-sort-and-sync-refs</ToolCommandName>
  </PropertyGroup>
  <ItemGroup>
    <None Remove="TestContent\Directory.Packages.props" />
  </ItemGroup>
  <ItemGroup>
    <EmbeddedResource Include="Sort.xsl" />
  </ItemGroup>
  <ItemGroup Condition=" '$(Configuration)' == 'Debug' ">
    <Content Include="TestContent\Directory.Packages.props">
      <CopyToOutputDirectory>Never</CopyToOutputDirectory>
    </Content>
    <Content Include="TestContent\Test.csproj">
      <CopyToOutputDirectory>Always</CopyToOutputDirectory>
    </Content>
  </ItemGroup>
  <ItemGroup>
    <PackageReference Include="LightInject.Annotation" />
    <PackageReference Include="LightInject" />
    <PackageReference Include="LightInject.Interception" />
    <PackageReference Include="McMaster.Extensions.CommandLineUtils" />
    <PackageReference Include="Spectre.IO" />
    <PackageReference Include="MassTransit" Version="8.3.4" />
    <PackageReference Include="System.IO.Abstractions" />
  </ItemGroup>
  <ItemGroup Condition="'$(TargetFramework)' == 'net7.0'">
    <PackageReference Include="Microsoft.Extensions.DependencyInjection" />
    <PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version="7.0.0" />
  </ItemGroup>
  <ItemGroup Condition="'$(TargetFramework)' == 'net8.0'">
    <PackageReference Include="Microsoft.Extensions.DependencyInjection" />
    <PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version="8.0.0" />
  </ItemGroup>
</Project>
""";
        }

        public static string GetNetstandardResultUnsorted()
        {
            return
"""
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <Description>A .NET Core global tool to alphabetically sort package references in csproj or fsproj</Description>
    <VersionPrefix>3.0.0</VersionPrefix>
    <Authors>Babu Annamalai</Authors>
    <OutputType>Exe</OutputType>
    <TargetFramework>netstandard2.0</TargetFramework>
    <AssemblyName>dotnet-sort-and-sync-refs</AssemblyName>
    <PackageId>dotnet-sort-and-sync-refs</PackageId>
    <PackageOutputPath>./nupkg</PackageOutputPath>
    <PackageProjectUrl>https://github.com/mysticmind/dotnet-sort-refs</PackageProjectUrl>
    <PackageLicenseExpression>MIT</PackageLicenseExpression>
    <IsPackable>true</IsPackable>
    <PackAsTool>true</PackAsTool>
    <ToolCommandName>dotnet-sort-and-sync-refs</ToolCommandName>
  </PropertyGroup>
  <ItemGroup>
    <None Remove="TestContent\Directory.Packages.props" />
  </ItemGroup>
  <ItemGroup>
    <EmbeddedResource Include="Sort.xsl" />
  </ItemGroup>
  <ItemGroup Condition=" '$(Configuration)' == 'Debug' ">
    <Content Include="TestContent\Directory.Packages.props">
      <CopyToOutputDirectory>Never</CopyToOutputDirectory>
    </Content>
    <Content Include="TestContent\Test.csproj">
      <CopyToOutputDirectory>Always</CopyToOutputDirectory>
    </Content>
  </ItemGroup>
  <ItemGroup>
    <PackageReference Include="LightInject.Annotation" />
    <PackageReference Include="MassTransit.Abstractions" Version="8.3.4" />
    <PackageReference Include="LightInject" />
    <PackageReference Include="LightInject.Interception" />
    <PackageReference Include="McMaster.Extensions.CommandLineUtils" />
    <PackageReference Include="Spectre.IO" />
    <PackageReference Include="MassTransit.Analyzers" Version="8.3.4" />
    <PackageReference Include="System.IO.Abstractions" />
  </ItemGroup>
</Project>
""";
        }
    }
}
