// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using System.CommandLine;
using Microsoft.Deployment.DotNet.Releases;
using Microsoft.DotNet.Cli.Commands.Workload.Install;
using Microsoft.DotNet.Cli.NuGetPackageDownloader;
using Microsoft.DotNet.Cli.Utils;
using Microsoft.Extensions.EnvironmentAbstractions;
using Microsoft.NET.Sdk.WorkloadManifestReader;

namespace Microsoft.DotNet.Cli.Commands.Workload.Repair;

internal class WorkloadRepairCommand : WorkloadCommandBase
{
    private readonly PackageSourceLocation _packageSourceLocation;
    private readonly IInstaller _workloadInstaller;
    protected readonly IWorkloadResolverFactory _workloadResolverFactory;
    private readonly IWorkloadResolver _workloadResolver;
    private readonly ReleaseVersion _sdkVersion;
    private readonly string _dotnetPath;
    private readonly string _userProfileDir;
    private readonly WorkloadHistoryRecorder _recorder;

    public WorkloadRepairCommand(
        ParseResult parseResult,
        IReporter reporter = null,
        IWorkloadResolverFactory workloadResolverFactory = null,
        IInstaller workloadInstaller = null,
        INuGetPackageDownloader nugetPackageDownloader = null)
        : base(parseResult, verbosityOptions: WorkloadRepairCommandParser.VerbosityOption, reporter: reporter, nugetPackageDownloader: nugetPackageDownloader)
    {
        var configOption = parseResult.GetValue(WorkloadRepairCommandParser.ConfigOption);
        var sourceOption = parseResult.GetValue(WorkloadRepairCommandParser.SourceOption);
        _packageSourceLocation = string.IsNullOrEmpty(configOption) && (sourceOption == null || !sourceOption.Any()) ? null :
            new PackageSourceLocation(string.IsNullOrEmpty(configOption) ? null : new FilePath(configOption), sourceFeedOverrides: sourceOption);

        _workloadResolverFactory = workloadResolverFactory ?? new WorkloadResolverFactory();

        if (!string.IsNullOrEmpty(parseResult.GetValue(WorkloadRepairCommandParser.VersionOption)))
        {
            throw new GracefulException(CliCommandStrings.SdkVersionOptionNotSupported);
        }

        var creationResult = _workloadResolverFactory.Create();

        _dotnetPath = creationResult.DotnetPath;
        _sdkVersion = creationResult.SdkVersion;
        _userProfileDir = creationResult.UserProfileDir;
        var sdkFeatureBand = new SdkFeatureBand(_sdkVersion);
        _workloadResolver = creationResult.WorkloadResolver;

        _workloadInstaller = workloadInstaller ??
                             WorkloadInstallerFactory.GetWorkloadInstaller(Reporter, sdkFeatureBand,
                                 _workloadResolver, Verbosity, creationResult.UserProfileDir, VerifySignatures, PackageDownloader, _dotnetPath, TempDirectoryPath,
                                 _packageSourceLocation, _parseResult.ToRestoreActionConfig());

        _recorder = new WorkloadHistoryRecorder(_workloadResolver, _workloadInstaller, () => _workloadResolverFactory.CreateForWorkloadSet(_dotnetPath, _sdkVersion.ToString(), _userProfileDir, null));
        _recorder.HistoryRecord.CommandName = "repair";
    }

    public override int Execute()
    {
        try
        {
            _recorder.Run(() =>
            {
                Reporter.WriteLine();

                var workloadIds = _workloadInstaller.GetWorkloadInstallationRecordRepository().GetInstalledWorkloads(new SdkFeatureBand(_sdkVersion));

                if (!workloadIds.Any())
                {
                    Reporter.WriteLine(CliCommandStrings.NoWorkloadsToRepair);
                    return;
                }

                Reporter.WriteLine(string.Format(CliCommandStrings.RepairingWorkloads, string.Join(" ", workloadIds)));

                ReinstallWorkloadsBasedOnCurrentManifests(workloadIds, new SdkFeatureBand(_sdkVersion));

                WorkloadInstallCommand.TryRunGarbageCollection(_workloadInstaller, Reporter, Verbosity, workloadSetVersion => _workloadResolverFactory.CreateForWorkloadSet(_dotnetPath, _sdkVersion.ToString(), _userProfileDir, workloadSetVersion));

                Reporter.WriteLine();
                Reporter.WriteLine(string.Format(CliCommandStrings.RepairSucceeded, string.Join(" ", workloadIds)));
                Reporter.WriteLine();
            });
        }
        catch (Exception e)
        {
            // Don't show entire stack trace
            throw new GracefulException(string.Format(CliCommandStrings.WorkloadRepairFailed, e.Message), e, isUserError: false);
        }
        finally
        {
            _workloadInstaller.Shutdown();
        }

        return _workloadInstaller.ExitCode;
    }

    private void ReinstallWorkloadsBasedOnCurrentManifests(IEnumerable<WorkloadId> workloadIds, SdkFeatureBand sdkFeatureBand)
    {
        _workloadInstaller.RepairWorkloads(workloadIds, sdkFeatureBand);
    }
}
