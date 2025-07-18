﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Reflection;
using System.Text.Json;
using Microsoft.Build.Framework;
using Microsoft.DotNet.Cli;
using Microsoft.DotNet.Configurer;
using Microsoft.DotNet.DotNetSdkResolver;
using Microsoft.DotNet.NativeWrapper;
using Microsoft.NET.Sdk.WorkloadMSBuildSdkResolver;
using static Microsoft.NET.Sdk.WorkloadMSBuildSdkResolver.CachingWorkloadResolver;

namespace Microsoft.DotNet.MSBuildSdkResolver
{
    // Thread-safety note:
    //  1. MSBuild can call the same resolver instance in parallel on multiple threads.
    //  2. Nevertheless, in the IDE, project re-evaluation can create new instances for each evaluation.
    //
    // As such, all state (instance or static) must be guarded against concurrent access/updates.
    // VSSettings are also effectively static (singleton instance that can be swapped by tests).

    public sealed class DotNetMSBuildSdkResolver : SdkResolver
    {
        public override string Name => "Microsoft.DotNet.MSBuildSdkResolver";

        // Default resolver has priority 10000 and we want to go before it and leave room on either side of us.
        public override int Priority => 5000;

        private readonly Func<string, string?> _getEnvironmentVariable;
        private readonly Func<string>? _getCurrentProcessPath;
        private readonly Func<string, string, string?> _getMsbuildRuntime;
        private readonly NETCoreSdkResolver _netCoreSdkResolver;

        private const string DotnetHostExperimentalKey = "DOTNET_EXPERIMENTAL_HOST_PATH";
        private const string MSBuildTaskHostRuntimeVersion = "SdkResolverMSBuildTaskHostRuntimeVersion";

        private static CachingWorkloadResolver _staticWorkloadResolver = new();

        private bool _shouldLog = false;

        public DotNetMSBuildSdkResolver()
            : this(Environment.GetEnvironmentVariable, null, GetMSbuildRuntimeVersion, VSSettings.Ambient)
        {
        }

        // Test constructor
        public DotNetMSBuildSdkResolver(Func<string, string?> getEnvironmentVariable, Func<string>? getCurrentProcessPath, Func<string, string, string?> getMsbuildRuntime, VSSettings vsSettings)
        {
            _getEnvironmentVariable = getEnvironmentVariable;
            _getCurrentProcessPath = getCurrentProcessPath;
            _netCoreSdkResolver = new NETCoreSdkResolver(getEnvironmentVariable, vsSettings);
            _getMsbuildRuntime = getMsbuildRuntime;

            if (_getEnvironmentVariable(EnvironmentVariableNames.DOTNET_MSBUILD_SDK_RESOLVER_ENABLE_LOG) is string val &&
                (string.Equals(val, "true", StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(val, "1", StringComparison.Ordinal)))
            {
                _shouldLog = true;
            }
        }

        private sealed class CachedState
        {
            public string? DotnetRoot;
            public string? MSBuildSdksDir;
            public string? NETCoreSdkVersion;
            public string? GlobalJsonPath;
            public IDictionary<string, string?>? PropertiesToAdd;
            public CachingWorkloadResolver? WorkloadResolver;
        }

        public override SdkResult? Resolve(SdkReference sdkReference, SdkResolverContext context, SdkResultFactory factory)
        {
            string? dotnetRoot = null;
            string? msbuildSdksDir = null;
            string? netcoreSdkVersion = null;
            string? globalJsonPath = null;
            IDictionary<string, string?>? propertiesToAdd = null;
            IDictionary<string, SdkResultItem>? itemsToAdd = null;
            List<string>? warnings = null;
            CachingWorkloadResolver? workloadResolver = null;

            ResolverLogger? logger = null;
            if (_shouldLog)
            {
                logger = new ResolverLogger();
            }

            logger?.LogMessage($"Attempting to resolve MSBuild SDK {sdkReference.Name}");

            if (context.State is CachedState priorResult)
            {
                logger?.LogString("Using previously cached state");

                dotnetRoot = priorResult.DotnetRoot;
                msbuildSdksDir = priorResult.MSBuildSdksDir;
                netcoreSdkVersion = priorResult.NETCoreSdkVersion;
                globalJsonPath = priorResult.GlobalJsonPath;
                propertiesToAdd = priorResult.PropertiesToAdd;
                workloadResolver = priorResult.WorkloadResolver;

                logger?.LogMessage($"\tDotnet root: {dotnetRoot}");
                logger?.LogMessage($"\tMSBuild SDKs Dir: {msbuildSdksDir}");
                logger?.LogMessage($"\t.NET Core SDK Version: {netcoreSdkVersion}");
            }

            if (context.IsRunningInVisualStudio)
            {
                logger?.LogString("Running in Visual Studio, using static workload resolver");
                workloadResolver = _staticWorkloadResolver;
            }

            if (workloadResolver == null)
            {
                workloadResolver = new CachingWorkloadResolver();
            }

            if (msbuildSdksDir == null)
            {
                dotnetRoot = EnvironmentProvider.GetDotnetExeDirectory(_getEnvironmentVariable, _getCurrentProcessPath, logger != null ? logger.LogMessage : null);
                logger?.LogMessage($"\tDotnet root: {dotnetRoot}");

                logger?.LogString("Resolving .NET Core SDK directory");
                string? globalJsonStartDir = GetGlobalJsonStartDir(context);
                logger?.LogMessage($"\tglobal.json start directory: {globalJsonStartDir}");
                var resolverResult = _netCoreSdkResolver.ResolveNETCoreSdkDirectory(globalJsonStartDir, context.MSBuildVersion, context.IsRunningInVisualStudio, dotnetRoot);

                if (resolverResult.ResolvedSdkDirectory == null)
                {
                    logger?.LogMessage($"Failed to resolve .NET SDK.  Global.json path: {resolverResult.GlobalJsonPath}");
                    return Failure(
                        factory,
                        logger,
                        context.Logger,
                        Strings.UnableToLocateNETCoreSdk);
                }

                logger?.LogMessage($"\tResolved SDK directory: {resolverResult.ResolvedSdkDirectory}");
                logger?.LogMessage($"\tglobal.json path: {resolverResult.GlobalJsonPath}");
                logger?.LogMessage($"\tFailed to resolve SDK from global.json: {resolverResult.FailedToResolveSDKSpecifiedInGlobalJson}");

                msbuildSdksDir = Path.Combine(resolverResult.ResolvedSdkDirectory, "Sdks");
                netcoreSdkVersion = new DirectoryInfo(resolverResult.ResolvedSdkDirectory).Name;
                globalJsonPath = resolverResult.GlobalJsonPath;

                // These are overrides that are used to force the resolved SDK tasks and targets to come from a given
                // base directory and report a given version to msbuild (which may be null if unknown. One key use case
                // for this is to test SDK tasks and targets without deploying them inside the .NET Core SDK.
                var msbuildSdksDirFromEnv = _getEnvironmentVariable(EnvironmentVariableNames.DOTNET_MSBUILD_SDK_RESOLVER_SDKS_DIR);
                var netcoreSdkVersionFromEnv = _getEnvironmentVariable(EnvironmentVariableNames.DOTNET_MSBUILD_SDK_RESOLVER_SDKS_VER);
                if (!string.IsNullOrEmpty(msbuildSdksDirFromEnv))
                {
                    logger?.LogMessage($"MSBuild SDKs dir overridden via DOTNET_MSBUILD_SDK_RESOLVER_SDKS_DIR to {msbuildSdksDirFromEnv}");
                    msbuildSdksDir = msbuildSdksDirFromEnv;
                }
                if (!string.IsNullOrEmpty(netcoreSdkVersionFromEnv))
                {
                    logger?.LogMessage($".NET Core SDK version overridden via DOTNET_MSBUILD_SDK_RESOLVER_SDKS_VER to {netcoreSdkVersionFromEnv}");
                    netcoreSdkVersion = netcoreSdkVersionFromEnv;
                }

                if (IsNetCoreSDKSmallerThanTheMinimumVersion(netcoreSdkVersion, sdkReference.MinimumVersion))
                {
                    return Failure(
                        factory,
                        logger,
                        context.Logger,
                        Strings.NETCoreSDKSmallerThanMinimumRequestedVersion,
                        netcoreSdkVersion,
                        sdkReference.MinimumVersion);
                }

                Version minimumMSBuildVersion = _netCoreSdkResolver.GetMinimumMSBuildVersion(resolverResult.ResolvedSdkDirectory);
                if (context.MSBuildVersion < minimumMSBuildVersion)
                {
                    return Failure(
                        factory,
                        logger,
                        context.Logger,
                        Strings.MSBuildSmallerThanMinimumVersion,
                        netcoreSdkVersion,
                        minimumMSBuildVersion,
                        context.MSBuildVersion);
                }

                string minimumVSDefinedSDKVersion = GetMinimumVSDefinedSDKVersion();
                if (IsNetCoreSDKSmallerThanTheMinimumVersion(netcoreSdkVersion, minimumVSDefinedSDKVersion))
                {
                    return Failure(
                        factory,
                        logger,
                        context.Logger,
                        Strings.NETCoreSDKSmallerThanMinimumVersionRequiredByVisualStudio,
                        netcoreSdkVersion,
                        minimumVSDefinedSDKVersion);
                }

                string? dotnetExe =
                    TryResolveDotnetExeFromSdkResolution(resolverResult)
                    ?? Path.Combine(dotnetRoot, RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? Constants.DotNetExe : Constants.DotNet);
                if (File.Exists(dotnetExe))
                {
                    propertiesToAdd ??= new Dictionary<string, string?>();
                    propertiesToAdd.Add(DotnetHostExperimentalKey, dotnetExe);
                }
                else
                {
                    logger?.LogMessage($"Could not set '{DotnetHostExperimentalKey}' because dotnet executable '{dotnetExe}' does not exist.");
                }

                string? runtimeVersion = dotnetRoot != null ?
                    _getMsbuildRuntime(resolverResult.ResolvedSdkDirectory, dotnetRoot) :
                    null;
                if (!string.IsNullOrEmpty(runtimeVersion))
                {
                    propertiesToAdd ??= new Dictionary<string, string?>();
                    propertiesToAdd.Add(MSBuildTaskHostRuntimeVersion, runtimeVersion);
                }
                else
                {
                    logger?.LogMessage($"Could not set '{MSBuildTaskHostRuntimeVersion}' because runtime version could not be determined.");
                }

                if (resolverResult.FailedToResolveSDKSpecifiedInGlobalJson)
                {
                    logger?.LogMessage($"Could not resolve SDK specified in '{resolverResult.GlobalJsonPath}'. Ignoring global.json for this resolution.");

                    if (warnings == null)
                    {
                        warnings = new List<string>();
                    }

                    if (!string.IsNullOrWhiteSpace(resolverResult.RequestedVersion))
                    {
                        warnings.Add(string.Format(Strings.GlobalJsonResolutionFailedSpecificVersion, resolverResult.RequestedVersion));
                    }
                    else
                    {
                        warnings.Add(Strings.GlobalJsonResolutionFailed);
                    }

                    propertiesToAdd ??= new Dictionary<string, string?>();
                    propertiesToAdd.Add("SdkResolverHonoredGlobalJson", "false");
                    propertiesToAdd.Add("SdkResolverGlobalJsonPath", resolverResult.GlobalJsonPath);

                    if (logger != null)
                    {
                        CopyLogMessages(logger, context.Logger);
                    }
                }
            }

            context.State = new CachedState
            {
                DotnetRoot = dotnetRoot,
                MSBuildSdksDir = msbuildSdksDir,
                NETCoreSdkVersion = netcoreSdkVersion,
                GlobalJsonPath = globalJsonPath,
                PropertiesToAdd = propertiesToAdd,
                WorkloadResolver = workloadResolver
            };

            //  First check if requested SDK resolves to a workload SDK pack
            string? userProfileDir = CliFolderPathCalculatorCore.GetDotnetUserProfileFolderPath();
            ResolutionResult? workloadResult = null;
            if (dotnetRoot is not null && netcoreSdkVersion is not null)
            {
                workloadResult = workloadResolver.Resolve(sdkReference.Name, dotnetRoot, netcoreSdkVersion, userProfileDir, globalJsonPath);
            }

            if (workloadResult is not CachingWorkloadResolver.NullResolutionResult)
            {
                return workloadResult?.ToSdkResult(sdkReference, factory);
            }

            string msbuildSdkDir = Path.Combine(msbuildSdksDir, sdkReference.Name, "Sdk");
            if (!Directory.Exists(msbuildSdkDir))
            {
                return Failure(
                    factory,
                    logger,
                    context.Logger,
                    Strings.MSBuildSDKDirectoryNotFound,
                    msbuildSdkDir);
            }

            return factory.IndicateSuccess(msbuildSdkDir, netcoreSdkVersion, propertiesToAdd, itemsToAdd, warnings);
        }

        /// <summary>Try to find the dotnet binary from the SDK resolution result upwards</summary>
        private static string? TryResolveDotnetExeFromSdkResolution(SdkResolutionResult resolverResult)
        {
            var expectedFileName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? Constants.DotNetExe : Constants.DotNet;
            var currentDir = resolverResult.ResolvedSdkDirectory;
            while (currentDir != null)
            {
                var dotnetExe = Path.Combine(currentDir, expectedFileName);
                if (File.Exists(dotnetExe))
                {
                    return dotnetExe;
                }

                currentDir = Path.GetDirectoryName(currentDir);
            }

            return null;
        }

        private static string? GetMSbuildRuntimeVersion(string sdkDirectory, string dotnetRoot)
        {
            // 1. Get the runtime version from the MSBuild.runtimeconfig.json file
            string runtimeConfigPath = Path.Combine(sdkDirectory, "MSBuild.runtimeconfig.json");
            if (!File.Exists(runtimeConfigPath)) return null;

            using var stream = File.OpenRead(runtimeConfigPath);
            using var jsonDoc = JsonDocument.Parse(stream);

            JsonElement root = jsonDoc.RootElement;
            if (!root.TryGetProperty("runtimeOptions", out JsonElement runtimeOptions) ||
                !runtimeOptions.TryGetProperty("framework", out JsonElement framework)) return null;

            string? runtimeName = framework.GetProperty("name").GetString();
            string? runtimeVersion = framework.GetProperty("version").GetString();

            // 2. Check that the runtime version is installed (in shared folder)
            return (!string.IsNullOrEmpty(runtimeName) && !string.IsNullOrEmpty(runtimeVersion) &&
                    Directory.Exists(Path.Combine(dotnetRoot, "shared", runtimeName, runtimeVersion)))
                    ? runtimeVersion : null;
        }

        private static SdkResult Failure(SdkResultFactory factory, ResolverLogger? logger, SdkLogger sdkLogger, string format, params object?[] args)
        {
            string error = string.Format(format, args);

            if (logger != null)
            {
                logger.LogMessage($"Failed to resolve SDK: {error}");
                CopyLogMessages(logger, sdkLogger);
            }

            return factory.IndicateFailure(new[] { error });
        }

        private static void CopyLogMessages(ResolverLogger source, SdkLogger destination)
        {
            foreach (var message in source.Messages)
            {
                destination.LogMessage(message.ToString(), MessageImportance.High);
            }
            //  Avoid copying the same messages again if CopyLogMessages is called multiple times
            source.Messages.Clear();
        }

        /// <summary>
        /// Gets the starting path to search for global.json.
        /// </summary>
        /// <param name="context">A <see cref="SdkResolverContext" /> that specifies where the current project is located.</param>
        /// <returns>The full path to a starting directory to use when searching for a global.json.</returns>
        private static string? GetGlobalJsonStartDir(SdkResolverContext context)
        {
            // Evaluating in-memory projects with MSBuild means that they won't have a solution or project path.
            // Default to using the current directory as a best effort to finding a global.json.  This could result in
            // using the wrong one but without a starting directory, SDK resolution won't work at all.  In most cases, a
            // global.json won't be found and the default SDK will be used.

            string? startDir = Environment.CurrentDirectory;

            if (!string.IsNullOrWhiteSpace(context.SolutionFilePath))
            {
                startDir = Path.GetDirectoryName(context.SolutionFilePath);
            }
            else if (!string.IsNullOrWhiteSpace(context.ProjectFilePath))
            {
                startDir = Path.GetDirectoryName(context.ProjectFilePath);
            }

            return startDir;
        }

        private static string GetMinimumVSDefinedSDKVersion()
        {
            string? dotnetMSBuildSdkResolverDirectory =
                Path.GetDirectoryName(typeof(DotNetMSBuildSdkResolver).GetTypeInfo().Assembly.Location);

            string minimumVSDefinedSdkVersionFilePath =
                Path.Combine(dotnetMSBuildSdkResolverDirectory ?? string.Empty, "minimumVSDefinedSDKVersion");

            if (!File.Exists(minimumVSDefinedSdkVersionFilePath))
            {
                // smallest version that is required by VS 15.3.
                return "1.0.4";
            }

            return File.ReadLines(minimumVSDefinedSdkVersionFilePath).First().Trim();
        }

        private bool IsNetCoreSDKSmallerThanTheMinimumVersion(string? netcoreSdkVersion, string minimumVersion)
        {
            FXVersion? netCoreSdkFXVersion;
            FXVersion? minimumFXVersion;

            if (string.IsNullOrEmpty(minimumVersion))
            {
                return false;
            }

            if (!FXVersion.TryParse(netcoreSdkVersion, out netCoreSdkFXVersion) ||
                netCoreSdkFXVersion is null ||
                !FXVersion.TryParse(minimumVersion, out minimumFXVersion) ||
                minimumFXVersion is null)
            {
                return true;
            }

            return FXVersion.Compare(netCoreSdkFXVersion, minimumFXVersion) < 0;
        }


        class ResolverLogger
        {
            public List<object> Messages = new();

            public ResolverLogger()
            {

            }

            public void LogMessage(FormattableString message)
            {
                Messages.Add(message);
            }

            public void LogString(string message)
            {
                Messages.Add(message);
            }
        }
    }
}
