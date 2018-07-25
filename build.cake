#tool "nuget:?package=xunit.runner.console&version=2.3.1"
#addin "Newtonsoft.Json"

///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument<string>("target", "Default");
var configuration = Argument<string>("configuration", "Release");
var verbosity = Argument<Cake.Core.Diagnostics.Verbosity>("verbosity");
var version = Argument<string>("packVersion");

///////////////////////////////////////////////////////////////////////////////
// GLOBAL VARIABLES
///////////////////////////////////////////////////////////////////////////////

var solutions = GetFiles("./**/solve-bank.sln");
var solutionDir = Directory("./");
var solutionName = solutions.First().GetFilenameWithoutExtension().ToString();
var solutionPaths = solutions.Select(solution => solution.GetDirectory());

var unitTestsProjGlob = "./**/*.Tests.Unit.csproj";
var integrationTestsProjGlob = "./**/*.Tests.Integration.csproj";
var acceptanceTestsProjGlob = "./**/*.Tests.Acceptance.csproj";

var mainProjDir = GetDirectories("./**/*.Console").First();
var artifactsDir = Directory("./artifacts");
var publishDir = Directory("./publish");

var dotNetCoreVerbosity = (Cake.Common.Tools.DotNetCore.DotNetCoreVerbosity)(int)verbosity;

///////////////////////////////////////////////////////////////////////////////
// COMMON FUNCTION DEFINITIONS
///////////////////////////////////////////////////////////////////////////////

void Test(string testDllGlob, Cake.Common.Tools.DotNetCore.DotNetCoreVerbosity dotNetCoreVerbosity, string configuration)
{
    var testAssemblies = GetFiles(testDllGlob);

    var tempVerbosity = (dotNetCoreVerbosity - 1); // xUnit is wayyy too verbose
    Information("xUnit is quite verbose, so we're going to use `--verbosity='{0}'` for the following tests.", tempVerbosity);

    foreach(var testProject in testAssemblies)
    {
        Information("Testing '{0}'...", testProject.FullPath);
        var settings = new DotNetCoreTestSettings
        {
            Configuration = configuration,
            Verbosity = tempVerbosity
        };

        DotNetCoreTest(testProject.FullPath, settings);
    }
}

void Run(string executable, string arguments, string workingDirectory)
{
	var silence = verbosity != Verbosity.Diagnostic;

	var processSettings = new ProcessSettings
	{
		Silent = silence,
		RedirectStandardError = false,
		RedirectStandardOutput = silence,
		Arguments = ProcessArgumentBuilder.FromString(arguments),
		WorkingDirectory = workingDirectory
	};

	var process = StartAndReturnProcess(new FilePath(executable), processSettings);

	process.WaitForExit();
}

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////

Setup(ctx =>
{
    Information("Requested verbosity was '{0}'", verbosity);
    Information("Verbosity for DotNetCore operations will be set to '{0}'", dotNetCoreVerbosity);

    // Executed BEFORE the first task.
    EnsureDirectoryExists(artifactsDir);
    EnsureDirectoryExists(publishDir);
    Verbose("Required directories have been created. Let's go!");
});

Teardown(ctx =>
{
    // Executed AFTER the last task.
    Verbose("Finished running all tasks.");
});

///////////////////////////////////////////////////////////////////////////////
// TASK DEFINITIONS
///////////////////////////////////////////////////////////////////////////////

Task("Clean")
    .Description("Cleans all directories that are used during the build process.")
    .Does(() =>
{
    foreach(var path in solutionPaths)
    {
        Verbose("Cleaning '{0}'", path);
        CleanDirectories(path + "/**/bin/" + configuration);
        CleanDirectories(path + "/**/obj");
        CleanDirectory(artifactsDir);
		CleanDirectory(publishDir);
        Verbose("'{0}' cleaned.", path);
    }
    Information("Cleaning completed.");
});

Task("Restore")
    .Description("Restores all the NuGet packages that are used by the specified solution.")
    .Does(() =>
{
    string rawPackageSources = EnvironmentVariable("PackageSources");
    Verbose("Customising NuGet sources. Going to use [{0}]", rawPackageSources);

    IList<string> packageSources = null;
    if (!string.IsNullOrEmpty(rawPackageSources))
    {
        packageSources = rawPackageSources.Split(';');
    }

    var msBuildSettings = new DotNetCoreMSBuildSettings {
        TreatAllWarningsAs = MSBuildTreatAllWarningsAs.Error,
				Verbosity = dotNetCoreVerbosity,
        DiagnosticOutput = (dotNetCoreVerbosity >= Cake.Common.Tools.DotNetCore.DotNetCoreVerbosity.Detailed)
    };
		msBuildSettings.Properties.Add("Configuration", new [] { configuration });
     
    var restoreSettings = new DotNetCoreRestoreSettings { 
        Sources = packageSources,
        Verbosity = dotNetCoreVerbosity,
        DiagnosticOutput = (dotNetCoreVerbosity >= Cake.Common.Tools.DotNetCore.DotNetCoreVerbosity.Detailed),
				MSBuildSettings = msBuildSettings
    };

    foreach(var solution in solutions)
    {
        Verbose("Restoring NuGet packages for '{0}'...", solution);
        DotNetCoreRestore(solution.FullPath, restoreSettings);
        Verbose("NuGet packages restored for '{0}'.", solution);
    }
});

Task("Build")
    .Description("Builds all the different parts of the project.")
    .Does(() =>
{
    // Create build metadata:
    var buildMetadata = new {
        BuildMachine = System.Environment.MachineName,
        BuiltBy = EnvironmentVariable("BuildTriggeredBy"),
        BuiltWhen = System.DateTime.UtcNow.ToString("o"),
        GitSha1 = EnvironmentVariable("GitSha1"),
        BuildNumber = version != "" ? version : null
    };

    Verbose("Embedding the following build metadata:" + Environment.NewLine +
            "\tBuildMachine: " + (buildMetadata.BuildMachine ?? "(null)") + Environment.NewLine +
            "\tBuiltBy: " + (buildMetadata.BuiltBy ?? "(null)") + Environment.NewLine +
            "\tBuiltWhen: " + buildMetadata.BuiltWhen + Environment.NewLine +
            "\tGitSha1: " + (buildMetadata.GitSha1 ?? "(null)") + Environment.NewLine +
            "\tBuildNumber: " + (buildMetadata.BuildNumber ?? "(null)"));
   
    var buildMetadataFolder = mainProjDir.Combine("Properties");
    EnsureDirectoryExists(buildMetadataFolder);
    Verbose("Ensured 'build metadata' directory at '{0}' exists.", buildMetadataFolder);

    var jsonSettings = new Newtonsoft.Json.JsonSerializerSettings { 
        ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver() 
    };
    var buildMetadataJson = Newtonsoft.Json.JsonConvert.SerializeObject(buildMetadata, jsonSettings);
    var buildMetadataFile = File(buildMetadataFolder + "/build.meta.json");
    System.IO.File.WriteAllText(buildMetadataFile, buildMetadataJson, Encoding.UTF8);

    // Build each solution:
    foreach(var solution in solutions)
    {
        Verbose("Building '{0}'...", solution);
        var msBuildSettings = new DotNetCoreMSBuildSettings {
            Verbosity = dotNetCoreVerbosity,
						DiagnosticOutput = (dotNetCoreVerbosity >= Cake.Common.Tools.DotNetCore.DotNetCoreVerbosity.Detailed),
        };
		
		msBuildSettings.Properties.Add("AssemblyVersion", new [] { version });
		msBuildSettings.Properties.Add("PackageVersion", new [] { version });
		msBuildSettings.Properties.Add("FileVersion", new [] { version });
		msBuildSettings.Properties.Add("Version", new [] { version });

		msBuildSettings.Properties.Add("Authors", new [] { "Shipping and Delivery" });
		msBuildSettings.Properties.Add("Copyright", new [] { "Copyright 2018 (c) Sander van Vliet. All rights reserved." });
        
		var settings = new DotNetCoreBuildSettings {
            Configuration = configuration,
            MSBuildSettings = msBuildSettings
        };

        DotNetCoreBuild(solution.FullPath, settings);
        Verbose("'{0}' has been built.", solution);
    }
});

Task("Test-Unit")
    .Description("Runs all your unit tests, using xUnit.")
    .Does(() => { Test(unitTestsProjGlob, dotNetCoreVerbosity, configuration); });

Task("Test-Integration")
    .Description("Runs all your integration tests, using xUnit.")
    .Does(() => { Test(integrationTestsProjGlob, dotNetCoreVerbosity, configuration); });

Task("Test-Acceptance")
    .Description("Runs all your acceptance tests, using xUnit.")
    .Does(() => { Test(acceptanceTestsProjGlob, dotNetCoreVerbosity, configuration); });

Task("Publish")
    .Description("Publishes all the different parts of the project.")
    .Does(() => 
{
		var msBuildSettings = new DotNetCoreMSBuildSettings {};
		
		msBuildSettings.Properties.Add("AssemblyVersion", new [] { version });
		msBuildSettings.Properties.Add("PackageVersion", new [] { version });
		msBuildSettings.Properties.Add("FileVersion", new [] { version });
		msBuildSettings.Properties.Add("Version", new [] { version });

		msBuildSettings.Properties.Add("Authors", new [] { "Shipping and Delivery" });
		msBuildSettings.Properties.Add("Copyright", new [] { "Copyright 2018 (c) Sander van Vliet. All rights reserved." });
        
		
        Verbose("Publishing everything from '{0}'...", artifactsDir);
        var settings = new DotNetCorePublishSettings
        {
            Configuration = configuration,
			MSBuildSettings = msBuildSettings,
            OutputDirectory = artifactsDir,
						Verbosity = dotNetCoreVerbosity,
						DiagnosticOutput = (dotNetCoreVerbosity >= Cake.Common.Tools.DotNetCore.DotNetCoreVerbosity.Detailed),
        };
        DotNetCorePublish(mainProjDir.FullPath, settings);
        Verbose("Everything from '{0}' has been published.", artifactsDir);
});

///////////////////////////////////////////////////////////////////////////////
// COMBINATIONS - let's make life easier...
///////////////////////////////////////////////////////////////////////////////

Task("Build+Test")
    .Description("First runs Build, then Test targets.")
    .IsDependentOn("Clean")
    .IsDependentOn("Restore")
    .IsDependentOn("Build")
    .IsDependentOn("Test-Unit")
    .Does(() => { Information("Ran Build+Test target"); });

Task("Rebuild")
    .Description("Runs a full Clean+Restore+Build build.")
    .IsDependentOn("Clean")
    .IsDependentOn("Restore")
    .IsDependentOn("Build")
    .Does(() => { Information("Rebuilt everything"); });

Task("Test-All")
    .Description("Runs all your tests.")
    .IsDependentOn("Test-Unit")
    .IsDependentOn("Test-Integration")
    .IsDependentOn("Test-Acceptance")
    .Does(() => { Information("Tested everything"); });

// REQUIRED TEAMCITY COMBOS:
Task("Pull-Request")
    .Description("Runs on TeamCity after creating a PR.")
    .IsDependentOn("Clean")
    .IsDependentOn("Restore")
    .IsDependentOn("Build")
    .IsDependentOn("Test-Unit")
    .IsDependentOn("Test-Integration")
    .IsDependentOn("Test-Acceptance") // Removed for the moment
    .Does(() => { Information("Successfully rebuilt everything! Congrats!"); });

///////////////////////////////////////////////////////////////////////////////
// DEFAULT TARGET
///////////////////////////////////////////////////////////////////////////////

Task("Default")
    .Description("This is the default task which will run if no specific target is passed in.")
    .IsDependentOn("Pull-Request")
    .Does(() => { Warning("No 'Target' was passed in, so we ran the TeamCity 'Pull-Request' operation."); });

///////////////////////////////////////////////////////////////////////////////
// EXECUTION
///////////////////////////////////////////////////////////////////////////////

RunTarget(target);
