using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using CliWrap;
using CliWrap.Buffered;
using Flurl.Http;
using Nuke.Common;
using Nuke.Common.CI.TeamCity;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.GitHub;
using Nuke.Common.Tools.NerdbankGitVersioning;
using Serilog;
using static Nuke.Common.EnvironmentInfo;

[TeamCity()]
class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode


    /// <summary>
    /// Nerdbank gitversioning tool.
    /// </summary>
    [NerdbankGitVersioning]
    readonly NerdbankGitVersioning NerdbankVersioning;

    #region Test  Properties

    public string CodeCoverage = "true";

    public string coverageFormat = "opencover";

    #endregion


    #region Pulumi

    string Project = "Kinderworx.Pulumi.AzCostMgmnt";

    string PulumiUrl = "https://api.pulumi.com/api/";

    string Organisation = "GurdipS5";

    string[] Stack =  {"Dev", "", "" };

string Deployment = "";

    /// <summary>
    /// Pulumi project path.
    /// </summary>
    string pulumiProj = string.Empty;
    #endregion

    /// <summary>
    ///
    /// </summary>
    public string projectName = "Kinderworx.Pulumi.AzMngmnt";


    public string OctopusVersion { get; private set; }

    /// <summary>
    ///
    /// </summary>
    public string Space = " ";

    /// <summary>
    ///
    /// </summary>
    string CoverletFile = string.Empty;

    #region NetCoreBuild

    string SelfContained = string.Empty;

    string runtime = "win-x64";

    #endregion

    /// <summary>
    ///
    /// </summary>
    public string CloudBuildNo { get; set; }


    /// <summary>
    ///  Visual Studio solution object.
    /// </summary>
    [Solution]
    readonly Solution Sln;

    /// <summary>
    ///     Git repository object.
    /// </summary>
    [GitRepository]
    readonly GitRepository Repository;


    #region Remote Services
    /// <summary>
    ///     DependencyTrack application URL.
    /// </summary>
    readonly string DependencyTrackUrl = "http://10.0.0.47:8081";

    /// <summary>
    ///     Sonarqube URL.
    /// </summary>
    readonly string SonarqubeUrl = "http://10.1.0.11:9000";

    /// <summary>
    /// Teamscale server URL.
    /// </summary>
    string TeamscaleUrl = "";

    #endregion'

    #region Tools

    /// <summary>
    /// Auto change log cmd for changelog creation.
    /// </summary>
    [PathVariable("auto-changelog")] readonly Tool AutoChangelogTool;

    /// <summary>
    ///  Dotnet-sonarscanner cli tool.
    /// </summary>
    [PathVariable("dotnet-sonarscanner")]
    readonly Tool SonarscannerTool;

    /// <summary>
    /// TrojanSource Finder.
    /// </summary>
    [PathVariable("tsfinder")] readonly Tool TsFinderTool;

    /// <summary>
    /// NDepend Console exe.
    /// </summary>
    [PathVariable(@"NDepend.Console.exe")] readonly Tool NDependConsoleTool;

    /// <summary>
    /// PVS Studio Cmd.
    /// </summary>
    [PathVariable(@"PVS-Studio_Cmd.exe")] readonly Tool PvsStudioTool;

    //    /// <summary>
    //    /// PlogConverter tool from PVS-Studio.
    //    /// </summary>
    [PathVariable(@"PlogConverter.exe")] readonly Tool PlogConverter;

    //    /// <summary>
    //    /// Dotnet Reactor Console exe.
    //    /// </summary>
    [PathVariable(@"dotNET_Reactor.Console.exe")] readonly Tool Eziriz;

    //    /// <summary>
    //    /// Go cli.
    //    /// </summary>
    [PathVariable(@"go.exe")] readonly Tool Go;

    //    /// <summary>
    //    /// DependencyTrack-audit CLI tool.
    //    /// </summary>
    [PathVariable(@"dtrack-audit")] readonly Tool DTrackAudit;

    /// <summary>
    ///     Qodana CLI.
    /// </summary>
    [PathVariable("qodana")]
    readonly Tool Qodana;


    // <summary>
    /// DocFX CLI.
    /// </summary>
    [PathVariable("docfx")]
    readonly Tool DocFx;

    /// <summary>
    ///     dotnet cli.
    /// </summary>
    [PathVariable("dotnet")]
    readonly Tool DotNet;

    /// <summary>
    ///     Dotnet-format dotnet tool.
    /// </summary>
    [PathVariable("dotnet-format")]
    readonly Tool DotnetFormatTool;


    /// <summary>
    ///     GGShield CLI for detecting secrets.
    /// </summary>
    [PathVariable("ggshield")]
    readonly Tool GgShield;

    /// <summary>
    ///     ReportGenerator tool.
    /// </summary>
    [PathVariable("reportgenerator")]
    readonly Tool ReportGenerator;

    //    /// <summary>
    //    /// Codecov CLI.
    //    /// </summary>
    [PathVariable("codecov")] readonly Tool Codecov;

    /// <summary>
    ///
    /// </summary>
    [PathVariable("ggshield")] readonly Tool GGShield;

    /// <summary>
    ///
    /// </summary>
    [PathVariable("NDepend.console.exe")] readonly Tool NDepend;


    [PathVariable("pulumi")] readonly Tool Pulumi;

    // <summary>
    // Snyk cli.
    // </summary>
    [PathVariable("snyk")] readonly Tool SnykTool;


    #endregion

    #region Secrets


    /// <summary>
    ///
    /// </summary>
    [Parameter] [Secret]
    readonly string CODECOV_SECRET;

    /// <summary>
    ///
    /// </summary>
    [Secret]
    readonly string PULUMI_TOKEN;

    /// <summary>
    /// s
    /// </summary>
    [Parameter][Secret] readonly string SonarKey;

    /// <summary>
    ///
    /// </summary>
    [Parameter][Secret] readonly string GitHubToken;

    /// <summary>
    /// Sny API Token.
    /// </summary>
    [Parameter] [Secret]
    readonly string SNYK_TOKEN;


    /// <summary>
    ///     License key for Report Generator.
    /// </summary>
    [Parameter]
    [Secret]
    readonly string ReportGeneratorLicense;

    /// <summary>
    ///     Dependency Track API Key.
    /// </summary>
    [Parameter]
    [Secret]
    readonly string DTrackApiKey2;



    #endregion

    #region Paths

    /// <summary>
    /// s.
    /// </summary>
    readonly AbsolutePath BuildDir = RootDirectory / "Nuke" / "Output" / "Build";

    /// <summary>
    ///
    /// </summary>
    readonly AbsolutePath CodecovYml = RootDirectory / "codecov.yml";

    /// <summary>
    ///
    /// </summary>
    readonly AbsolutePath Artifacts = RootDirectory / "Nuke" / "Artifacts";


    /// <summary>
    ///     Output of coverlet code  coverage report.
    /// </summary>
    readonly AbsolutePath CoverletOutput = RootDirectory / "Nuke" / "Output" / "Coverlet";

    /// <summary>
    /// NDependOutput folder.
    /// </summary>
    readonly AbsolutePath NukeOut = RootDirectory / "Nuke";

    /// <summary>
    ///  NDependOutput folder.
    /// </summary>
    readonly AbsolutePath NDependOutput = RootDirectory / "Nuke" / "Output" / "NDependOut";

    /// <summary>
    ///
    /// </summary>
    readonly AbsolutePath GgConfig = RootDirectory / "gitguardian.yml";

    /// <summary>
    ///     Dotnet publish output directory
    /// </summary>
    readonly AbsolutePath PublishFolder = RootDirectory / "Nuke" / "Output" / "Publish";

    /// <summary>
    ///     PVS Studio log output folder.
    /// </summary>
    readonly AbsolutePath PvsStudio = RootDirectory / "Nuke" / "Output" / "PVS";

    /// <summary>
    ///     Path to nupkg file from the project
    /// </summary>
    readonly AbsolutePath NupkgPath = RootDirectory / "Nuke" / "Output" / "Nuget";

    /// <summary>
    ///
    /// </summary>
    readonly AbsolutePath ReportOut = RootDirectory / "Nuke" / "Output" / "Coverlet" / "Report";

    /// <summary>
    ///     Output directory of the SBOM file from CycloneDX
    /// </summary>
    readonly AbsolutePath Sbom = RootDirectory / "Nuke" / "Output" / "SBOM";


    /// <summary>
    /// Filename of changelog file.
    /// </summary>
    string ChangeLogFile => RootDirectory / "changelog.md";

    /// <summary>
    /// </summary>

    AbsolutePath DocFxLibrary => RootDirectory / "docfx_project";

    /// <summary>
    /// Directory of MSTests project.
    /// </summary>
    AbsolutePath TestsDirectory => RootDirectory.GlobDirectories("*.Tests").Single();

    /// <summary>
    /// Target path.
    ///
    /// </summary>
    readonly AbsolutePath TargetPath = RootDirectory / "Nuke" / "Output" / "Coverlet" / "Report";

    #endregion

    public static int Main() => Execute<Build>(x => x.InvokePulumiDeployments);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    AbsolutePath SourceDirectory => RootDirectory / "docs";



    /// <summary>
    /// Set and create build paths.
    /// </summary>
    Target SetPathsTarget => _ => _
        .Before(SetVariablesTarget)
        .Executes(() =>
        {
            Directory.CreateDirectory(NupkgPath.ToString());
            Directory.CreateDirectory(PublishFolder.ToString());
            Directory.CreateDirectory(NDependOutput.ToString());
            Directory.CreateDirectory(BuildDir.ToString());
            Directory.CreateDirectory(PvsStudio.ToString());
            Directory.CreateDirectory(CoverletOutput.ToString());
            Directory.CreateDirectory(ReportOut.ToString());
            Directory.CreateDirectory(TargetPath.ToString());
            Directory.CreateDirectory(Sbom.ToString());
            Directory.CreateDirectory(Artifacts.ToString());
        });

    /// <summary>
    ///
    /// </summary>
    Target SetVariablesTarget => _ => _
        .After(SetPathsTarget)

        .Executes(() =>
        {
            Log.Information(SNYK_TOKEN);
            pulumiProj = Sln.GetProject(projectName).Path.ToString();
        });

    /// <summary>
    /// Creates documentation.
    /// </summary>
    Target CreateDocFx => _ => _
        .DependsOn(SetVariablesTarget)
        .AssuredAfterFailure()
        .Executes(async () =>
        {

            DocFx("build docfx.json", DocFxLibrary);
        });

    /// <summary>
    ///  Authenticate to Synk service.
    /// </summary>
    Target SnykAuth => _ => _
        .DependsOn(CreateDocFx)
        .Description("Authenticate to Snyk.")
        .AssuredAfterFailure()
        .Executes(() =>
        {

            SnykTool($"auth 1");
        });

    Target SnykScan => _ => _
        .DependsOn(SnykAuth)
        .AssuredAfterFailure()
        .Executes(() =>
        {
            SnykTool($"code test", RootDirectory);
        });

    /// <summary>
    /// Run dotnet format to format code.
    /// </summary>
    Target RunDotnetFormatTarget => _ => _
        .DependsOn(SnykScan)
        .Executes(() =>
        {
            DotNet("format");
        });


    /// <summary>
    ///  Scan code for hardcoded secrets.
    /// </summary>
    Target SecretScan => _ => _
        .DependsOn(RunDotnetFormatTarget)
        .AssuredAfterFailure()
        .Executes(() =>
        {
            if (IsLocalBuild)
            {
                //  GGShield("auth login");
                GgShield($"--config-path {GgConfig} secret scan commit-range HEAD~1");
            }
        });

    /// <summary>
    ///  Runs dotnet outdated against Nuget packages.
    /// </summary>
    Target RunDotnetOutdated => _ => _
        .DependsOn(SecretScan)
        .AssuredAfterFailure()
        .Executes(() =>
        {
            DotNet($"outdated {RootDirectory}");
        });

    /// <summary>
    ///     Runs dotnet outdated against Nuget packages.
    /// </summary>
    Target RunQodanaScan => _ => _
        .DependsOn(RunDotnetOutdated)
        .Description("Runs Qodana linter")
        .AssuredAfterFailure()
        .Executes(() =>
        {
            Qodana("scan --ide QDNET");
        });

    /// <summary>
    /// Executes NDepend Analysis.
    /// </summary>
    Target RunNDepend => _ => _
        .DependsOn(RunQodanaScan)
        .Produces(NDependOutput / "*.zip")
        .AssuredAfterFailure()
        .Executes(() =>
        {
            var nDependProj = RootDirectory.GlobFiles("*.ndproj").FirstOrDefault();

            NDependConsoleTool(string.Format(nDependProj.ToString() + Space + @"/OutDir {0}", NDependOutput));

            if (IsServerBuild)
            {
                ZipFile.CreateFromDirectory(NDependOutput, Artifacts);
            }
        });

    /// <summary>
    ///  Create sbom json using CycloneDX.
    /// </summary>
    Target CycloneDx => _ => _
        .DependsOn(RunNDepend)
        .Produces(Sbom / "bom.json")
        .AssuredAfterFailure()
        .Executes(() =>
        {
            DotNet(@$"cyclonedx {pulumiProj} -o {Sbom} -j -dgl");
        });

    /// <summary>
    ///     Push to Dependency-Track.
    /// </summary>
    Target PushToDTrack => _ => _
        .DependsOn(CycloneDx)
        .Produces(Sbom / "bom.json")
        .AssuredAfterFailure()
        .Executes(() =>
        {
            var sbomPath = Sbom / "bom.json";

            if (IsLocalBuild)
            {
                DTrackAudit(
                    @$"-a -k {DTrackApiKey2} -n {BuildUtils.ReplaceDotsToDashes(projectName)} -u {DependencyTrackUrl} -v {OctopusVersion} -i {sbomPath}");
            }

            else if (IsServerBuild)
                DTrackAudit(
                    @$"run main.go -ApiPath -k {DTrackApiKey2} -n {BuildUtils.ReplaceDotsToDashes(projectName)} -u {DependencyTrackUrl} -v {CloudBuildNo} -i {sbomPath}",
                    RootDirectory);
        });

    /// <summary>
    ///
    /// </summary>
    Target RunPvsStudio => _ => _
        .DependsOn(PushToDTrack)
        .Executes(() =>
        {
            var pvsfile = string.Empty;
            var plogFile = PvsStudio / "pvs-studio.plog";
            pvsfile = plogFile.ToString();
            System.IO.File.Create(plogFile);

            string sln = Sln.Path;

            if (IsLocalBuild)
            {
                PvsStudioTool($@"-t {sln} -o {pvsfile}");
                PlogConverter($@"-t FullHtml -o {PvsStudio} -n PVS-Log {pvsfile}");
            }

            else
            {
                ZipFile.CreateFromDirectory(PvsStudio, Artifacts);
            }
        });

    /// <summary>
    ///     Starts Sonarqube scanner.
    /// </summary>
    Target StartSonarscan => _ => _
        .DependsOn(RunPvsStudio)
        .AssuredAfterFailure()
        .Executes(() =>
        {
            SonarscannerTool(
                @$"begin /k:{BuildUtils.ReplaceDotsToDashes(projectName)}  /d:sonar.host.url={SonarqubeUrl} /d:sonar.token={SonarKey}");
        });




    /// <summary>
    ///
    /// </summary>
    Target Clean => _ => _
        .DependsOn(StartSonarscan)
        .Executes(() =>
        {
            RootDirectory
                .GlobDirectories("**/{obj,bin}")
                .DeleteFiles();
        });

    Target Restore => _ => _
        .DependsOn(Clean)

        .Executes(() =>
        {
            DotNet($"restore {pulumiProj}");
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            if (IsServerBuild)
                DotNet(
                    $"publish {pulumiProj} -f {Framework} --self-contained {SelfContained} --output {PublishFolder}");


            else if (IsLocalBuild)
                DotNet(
                    $"publish {pulumiProj} -f {Framework} -r {runtime} --self-contained false --output {PublishFolder}");

        });


    /// <summary>
    ///  Obfuscate build dll.
    /// </summary>
    Target RunTests => _ => _
        .DependsOn(Compile)
        .AssuredAfterFailure()
        .Executes(() =>
        {
            Log.Information(TestsDirectory.ToString());

            var testProj = TestsDirectory.GlobFiles("*.Tests.csproj").FirstOrDefault();

            // Execute dotnet test to run the unit tests.
            DotNet($@"test {testProj.ToString()} /p:CollectCoverage={CodeCoverage} /p:CoverletOutputFormat=opencover");

            // Coverage xml file.
            var sourceFile = Path.Combine(TestsDirectory.ToString(), "coverage.opencover.xml");
            CoverletFile = Path.Combine(CoverletOutput.ToString(), "coverage.opencover.xml");

            File.Copy(sourceFile, CoverletFile, true);

            if (File.Exists(CoverletFile)) File.Delete(sourceFile);
        });


    /// <summary>
    ///  Creates Report of test coverage.
    /// </summary>
    Target RunReportGeneratorTarget => _ => _
        .DependsOn(RunTests)
        .AssuredAfterFailure()
        .Executes(() =>
        {
            string zipPath = Path.Combine(Artifacts, "testreports.zip");

            // targetPath is the folder for report.
            // so this will be ApiPath sub folder of coverletOutput.
            ReportGenerator(
                $"-reports:{CoverletFile} -targetdir:{ReportOut} -reporttypes:Html;TeamCitySummary;PngChart;Badges --license:{ReportGeneratorLicense}");

            if (IsServerBuild)
            {
                ZipFile.CreateFromDirectory(ReportOut, Artifacts);
                Console.WriteLine($"##teamcity[publishArtifacts '{zipPath}']");
            }
        });

    /// <summary>
    ///  Upload code coverage results to codecov
    /// </summary>
    Target UploadToCodeCov => _ => _
        .DependsOn(RunReportGeneratorTarget)
        .AssuredAfterFailure()
        .Executes(() =>
        {
            if (IsLocalBuild)
            {
                Log.Information(CodecovYml);

                Codecov($"--codecov-yml-path {CodecovYml} create-commit -t {CODECOV_SECRET} ", RootDirectory.ToString());
                Codecov($"--codecov-yml-path {CodecovYml}  create-report -t {CODECOV_SECRET} ", RootDirectory.ToString());
                Codecov($"--codecov-yml-path {CodecovYml}  do-upload -t {CODECOV_SECRET} ", RootDirectory.ToString());
            }

            // This runs on Teamcity, using env vars.
            if (IsServerBuild) Codecov($"-f {CoverletFile} -t {CODECOV_SECRET}", CoverletOutput.ToString());
        });


    /// <summary>
    ///     Ends Sonarqube analysis.
    /// </summary>
    Target EndSonarscanTarget => _ => _
        .DependsOn(UploadToCodeCov)
        .Description("End SonarQube scan")
        .AssuredAfterFailure()
        .Executes(() =>
        {
            SonarscannerTool($"end /d:sonar.login=\"{SonarKey}\"");
        });


    /// <summary>
    /// Versions the project using Nerdbank.
    /// </summary>
    Target SetVersionTarget => _ => _
        .DependsOn(EndSonarscanTarget)
        .AssuredAfterFailure()
        .Executes(() =>
        {
            if (IsLocalBuild || (IsServerBuild && !Repository.IsOnMainOrMasterBranch()))
            {
                var stdOutBuffer = new StringBuilder();
                var stdErrBuffer = new StringBuilder();

                var dbDailyTasks = Cli.Wrap("powershell")
                    .WithArguments(new[] { "nbgv get-version | convertto-json" })
                    .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdOutBuffer))
                    .WithStandardErrorPipe(PipeTarget.ToStringBuilder(stdErrBuffer))
                    .WithWorkingDirectory(RootDirectory)
                    .ExecuteBufferedAsync();

                BuildUtils.ExtractVersion(stdOutBuffer, stdErrBuffer);
            }

            // When the code is merged.
            if (IsServerBuild)
            {
                var c =
                    new NerdbankGitVersioningCloudSettings();

                c.SetProcessWorkingDirectory(RootDirectory);

                NerdbankGitVersioningTasks.NerdbankGitVersioningCloud(c);

                CloudBuildNo = NerdbankVersioning.CloudBuildNumber;

            }
        });

    /// <summary>
    ///  Set changelog file.
    /// </summary>
    Target AmendChangelogTarget => _ => _
        .DependsOn(SetVersionTarget)
        .Description("Creates a changelog of the current commit.")
        .AssuredAfterFailure()
        .Executes(() =>
        {
            if (IsLocalBuild)
                AutoChangelogTool($"-v  {OctopusVersion} -o {ChangeLogFile}",
                    RootDirectory.ToString()); // Use .autochangelog settings in file.
        });


    Target PushToGitHub => _ => _
        .DependsOn(AmendChangelogTarget)
        .Description("Push formatted code and changelog.md to GitHub repo.")
        .AssuredAfterFailure()
        .Executes(async () =>
        {
            if (IsLocalBuild)
            {
                var dbDailyTasks = await Cli.Wrap("powershell")
                    .WithArguments(new[] { "Split-Path -Leaf (git remote get-url origin)" })
                    .ExecuteBufferedAsync();

                var repoName = dbDailyTasks.StandardOutput.TrimEnd();

                var gitCommand = "git";
                var gitAddArgument = @"add -A";
                var gitCommitArgument = @"commit -m ""chore(ci): checking in changed code from local ci""";
                var gitPushArgument =
                    $@"push https://{GitHubToken}@github.com/{Repository.GetGitHubOwner()}/{repoName}";

                Log.Information(gitPushArgument);

                Process.Start(gitCommand, gitAddArgument).WaitForExit();
                Process.Start(gitCommand, gitCommitArgument).WaitForExit();
                Process.Start(gitCommand, gitPushArgument).WaitForExit();
            }
        });



    /// <summary>
    /// Prints the preview of Pulumi i.e. changes to be made.
    /// </summary>
    Target PulumiPreviewTarget => _ => _
        .DependsOn(PushToGitHub)
        .Executes(() =>
        {
            Pulumi("preview");
        });

    Target PausePulumiDeployment => _ => _
        .DependsOn(PulumiPreviewTarget)
        .Executes(() => {
            var client = new FlurlClient(PulumiUrl)
                .WithHeader("", PULUMI_TOKEN).Request().PostAsync();
        });

    Target InvokePulumiDeployments => _ => _
        .DependsOn(PulumiPreviewTarget)
        .Executes(() => {

            UriBuilder uriBuilder = new UriBuilder
            {
                Scheme = "https",
                Host = "www.example.com",
                Path = $"/{Organisation}/{Project}/{Stack}/deployments",
            };

            var client = new FlurlClient(uriBuilder.ToString())
            .WithHeader("", PULUMI_TOKEN).Request().PostAsync();
        });

    // On failure

    /// <summary>
    ///
    /// </summary>
    /// <param name="target"></param>
    protected override void OnTargetFailed(string target)
    {
        Log.Error("Error code 1. Exiting build from target {Target}.", target); // This will execute the pre-push commit.
        Environment.Exit(1);
    }

}
