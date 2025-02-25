﻿namespace DotnetSortAndSyncRefs.Test.TestContend.CommandBase
{
    internal static class MockFileStrings
    {
        public static string GetDirectoryPackagesPropsUnsorted()
        {
            return
"""
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
    <CentralPackageTransitivePinningEnabled>true</CentralPackageTransitivePinningEnabled>
  </PropertyGroup>
  <ItemGroup>
    <PackageVersion Include="Newtonsoft.Json" Version="13.0.1" />
    <PackageVersion Include="LightInject" Version="7.0.1" />
    <PackageVersion Include="LightInject.Attributes" Version="5.0.1" />
    <PackageVersion Include="LightInject.Attributes" Version="5.0.1" />
  </ItemGroup>
  <ItemGroup>
    <PackageVersion Include="LightInject.Attributes" Version="5.0.1" />
    <PackageVersion Include="LightInject.Attributes" Version="5.0.1" />
    <PackageVersion Include="Newtonsoft.Json" Version="13.0.1" />
    <PackageVersion Include="LightInject" Version="7.0.1" />
    <PackageVersion Include="McMaster.Extensions.CommandLineUtils" Version="4.1.1" />
  </ItemGroup>
</Project>
""";
        }

        public static string GetTestDotnetCsprojUnsorted()
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
    <PackageReference Include="System.IO.Abstractions" Version="21.1.7">
    </PackageReference>
  </ItemGroup>
  <ItemGroup>
    <PackageReference Include="Spectre.IO" Version="0.12.0" />
  </ItemGroup>
  <ItemGroup>
    <PackageReference Include="McMaster.Extensions.CommandLineUtils" Version="4.1.1" />
    <PackageReference Include="LightInject" Version="7.0.1" />
    <PackageReference Include="LightInject.Annotation" Version="1.1.0" />
    <PackageReference Include="LightInject.Interception" Version="1.1.0" />
  </ItemGroup>
  <ItemGroup Condition="'$(TargetFramework)' == 'net7.0'">
    <PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="7.0.0" />
  </ItemGroup>
  <ItemGroup Condition="'$(TargetFramework)' == 'net8.0'">
    <PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="8.0.1" />
  </ItemGroup>
</Project>
""";
        }


        public static string GetTestDotnetCsprojSorted()
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
   <PackageReference Include="LightInject" Version="7.0.1" />
   <PackageReference Include="LightInject.Annotation" Version="1.1.0" />
   <PackageReference Include="LightInject.Interception" Version="1.1.0" />
   <PackageReference Include="McMaster.Extensions.CommandLineUtils" Version="4.1.1" />
 </ItemGroup>
  <ItemGroup>
    <PackageReference Include="Spectre.IO" Version="0.12.0" />
  </ItemGroup>
  <ItemGroup>
    <PackageReference Include="System.IO.Abstractions" Version="21.1.7">
    </PackageReference>
  </ItemGroup>
  <ItemGroup Condition="'$(TargetFramework)' == 'net7.0'">
    <PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="7.0.0" />
  </ItemGroup>
  <ItemGroup Condition="'$(TargetFramework)' == 'net8.0'">
    <PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="8.0.1" />
  </ItemGroup>
</Project>
""";
        }

        public static string GetTestNetStandardCsprojUnsorted()
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
    <PackageReference Include="System.IO.Abstractions" Version="21.1.7">
    </PackageReference>
  </ItemGroup>
  <ItemGroup>
    <PackageReference Include="Spectre.IO" Version="0.12.0" />
  </ItemGroup>
  <ItemGroup>
    <PackageReference Include="McMaster.Extensions.CommandLineUtils" Version="4.1.1" />
    <PackageReference Include="LightInject" Version="7.0.1" />
    <PackageReference Include="LightInject.Annotation" Version="1.1.0" />
    <PackageReference Include="LightInject.Interception" Version="1.1.0" />
  </ItemGroup>
</Project>
""";
        }
    }
}
