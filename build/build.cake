#tool "nuget:?package=GitVersion.CommandLine"
#tool "nuget:?package=xunit.runner.console"

///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target        = Argument<string>("target", "Default");
var configuration = Argument<string>("configuration", "Release");
var verbosity     = Argument<string>("verbosity", "Normal");

///////////////////////////////////////////////////////////////////////////////
// GLOBAL VARIABLES
///////////////////////////////////////////////////////////////////////////////

string solutionDirectory;
string projectDirectory;
string publishDirectory;
string mergedDirectory;
string packageDirectory;
GitVersion versionInfo;

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////

Setup(context => {
    Information("");
    Information("Target        -> {0}", target);
    Information("Configuration -> {0}", configuration);
    Information("Verbosity     -> {0}", verbosity);
    Information("");

    Information("----------------------------------------");
    Information("Generated Paths");
    Information("----------------------------------------");

    solutionDirectory = "../src";
    projectDirectory  = "../src/Puur";
    publishDirectory  = "artifacts/publish";
    mergedDirectory   = "artifacts/merged";
    packageDirectory  = "artifacts/package";

    Warning("");
    Warning("Solution -> {0}", solutionDirectory);
    Warning("Project  -> {0}", projectDirectory); 
    Warning("Publish  -> {0}", publishDirectory);
    Warning("Merged   -> {0}", mergedDirectory);
    Warning("Package  -> {0}", packageDirectory);
});

///////////////////////////////////////////////////////////////////////////////
// TASK DEFINITIONS
///////////////////////////////////////////////////////////////////////////////

Task("Clean").Does(() => { 
    CleanDirectories("../src/**/bin");
    CleanDirectories("../src/**/obj");
    CleanDirectories("../tests/**/bin");
    CleanDirectories("../tests/**/obj");
    CleanDirectories("artifacts");
});

Task("Restore").Does(() => {
    //NuGetRestore(solutionDirectory);
    DotNetCoreRestore(solutionDirectory);
});

Task("Version").Does(() => {
    versionInfo = GitVersion(new GitVersionSettings { 
        OutputType         = GitVersionOutput.Json,
        UpdateAssemblyInfo = false
    });

    if(verbosity == "Normal")
    {
        Information("");
        Information("Assembly Version               -> {0}", versionInfo.AssemblySemVer);
        Information("Assembly File Version          -> {0}", versionInfo.AssemblySemVer);
        Information("Assembly Informational Version -> {0}", versionInfo.InformationalVersion);
    }
});

Task("Build")   
    .IsDependentOn("Restore")
    .IsDependentOn("Version").Does(() => 
        DotNetCoreBuild(solutionDirectory, new DotNetCoreBuildSettings { Configuration = configuration }));

Task("Test").IsDependentOn("Build").Does(() => {   
    XUnit2(GetFiles("../tests/**/bin/**/*.tests.dll"), new XUnit2Settings {
        HtmlReport      = true,
        OutputDirectory = "artifacts"
    }); 
    // var settings  = new DotNetCoreTestSettings { Configuration = configuration, NoBuild = true };
    // foreach(var testFile in GetFiles("../tests/**/bin/**/*.tests.dll").Select(x => x.ToString()))
    //     DotNetCoreTest(testFile, settings);   
});

Task("Default")
    .IsDependentOn("Clean")
    .IsDependentOn("Restore")
    .IsDependentOn("Build");

///////////////////////////////////////////////////////////////////////////////
// EXECUTION
///////////////////////////////////////////////////////////////////////////////

RunTarget(target);