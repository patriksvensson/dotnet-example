var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

////////////////////////////////////////////////////////////////
// Tasks

Task("Build")
    .Does(context => 
{
    DotNetBuild("./src/Example.sln", new DotNetBuildSettings {
        Configuration = configuration,
        NoIncremental = context.HasArgument("rebuild"),
        MSBuildSettings = new DotNetMSBuildSettings()
            .TreatAllWarningsAs(MSBuildTreatAllWarningsAs.Error)
    });
});

Task("Package")
    .IsDependentOn("Build")
    .Does(context => 
{
    context.CleanDirectory("./.artifacts");

    context.DotNetPack($"./src/Example.sln", new DotNetPackSettings {
        Configuration = configuration,
        NoRestore = true,
        NoBuild = true,
        OutputDirectory = "./.artifacts",
        MSBuildSettings = new DotNetMSBuildSettings()
            .TreatAllWarningsAs(MSBuildTreatAllWarningsAs.Error)
            .WithProperty("SymbolPackageFormat", "snupkg")
    });
});

Task("Integration-Test")
    .IsDependentOn("Package")
    .DoesForEach(
        static context => context
                            .GetFiles("./.artifacts/dotnet-example.*.nupkg")
                            .FirstOrDefault()
                                ?.GetFilenameWithoutExtension()
                                    .FullPath.Substring("dotnet-example.".Length)
                            is { } packageVersion
                                ? new[] { 
                                            (SdkVersion: "8.0.100", PackageVersion:packageVersion),
                                            (SdkVersion: "9.0.100", PackageVersion:packageVersion)
                                  }  
                                : throw new CakeException("No package version found.")
        ,
        static (test, context) => 
{
    context.Information("Running integration tests for .NET {0} with package version {1}", test.SdkVersion, test.PackageVersion);
    
    var artifactsPath = context.MakeAbsolute(context.Directory("./.artifacts"));
    var integrationTestPath = artifactsPath
                                .Combine("integration-test")
                                .Combine(test.SdkVersion);

    context.CleanDirectory(integrationTestPath);

    context.CopyDirectory("./examples", integrationTestPath.Combine("examples"));

    context.DotNetTool("new",
                    new DotNetToolSettings {
                        ArgumentCustomization = args => args
                                                            .Append("globaljson")
                                                            .Append("--force")
                                                            .AppendSwitchQuoted("--roll-forward", "latestFeature")
                                                            .AppendSwitchQuoted("--sdk-version", test.SdkVersion),
                        WorkingDirectory = integrationTestPath
                    });

    context.DotNetTool("new",
                        new DotNetToolSettings {
                            ArgumentCustomization = args => args
                                                                .Append("tool-manifest"),
                            WorkingDirectory = integrationTestPath
                        });

    context.DotNetTool(
                "tool",
                new DotNetToolSettings {
                    ArgumentCustomization = args => args
                                                        .Append("install")
                                                        .Append("dotnet-example")
                                                        .AppendSwitchQuoted(
                                                            test.SdkVersion switch {
                                                                "8.0.100" => "--add-source", 
                                                                _ =>  "--source", 
                                                            },
                                                            artifactsPath.FullPath)
                                                        .AppendSwitchQuoted("--version", test.PackageVersion),
                    WorkingDirectory = integrationTestPath
                });

    context.DotNetTool(
                "tool",
                new DotNetToolSettings {
                    ArgumentCustomization = args => args
                                                        .Append("run")
                                                        .Append("--")
                                                        .Append("dotnet-example")
                                                        .Append("--list"),
                    WorkingDirectory = integrationTestPath
                }
            );
});

Task("Publish-NuGet")
    .WithCriteria(ctx => BuildSystem.IsRunningOnGitHubActions, "Not running on GitHub Actions")
    .IsDependentOn("Integration-Test")
    .Does(context => 
{
    var apiKey = Argument<string>("nuget-key", null);
    if(string.IsNullOrWhiteSpace(apiKey)) {
        throw new CakeException("No NuGet API key was provided.");
    }

    // Publish to GitHub Packages
    foreach(var file in context.GetFiles("./.artifacts/*.nupkg")) 
    {
        context.Information("Publishing {0}...", file.GetFilename().FullPath);
        DotNetNuGetPush(file.FullPath, new DotNetNuGetPushSettings
        {
            Source = "https://api.nuget.org/v3/index.json",
            ApiKey = apiKey,
        });
    }
});

////////////////////////////////////////////////////////////////
// Targets

Task("Publish")
    .IsDependentOn("Publish-NuGet");

Task("Default")
    .IsDependentOn("Integration-Test");

////////////////////////////////////////////////////////////////
// Execution

RunTarget(target)