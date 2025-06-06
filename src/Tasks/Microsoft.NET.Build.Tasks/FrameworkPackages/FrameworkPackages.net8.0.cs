namespace Microsoft.ComponentDetection.Detectors.NuGet;

#nullable disable

using static global::NuGet.Frameworks.FrameworkConstants.CommonFrameworks;

/// <summary>
/// Framework packages for net8.0.
/// </summary>
internal partial class FrameworkPackages
{
    internal static class NETCoreApp80
    {
        internal static FrameworkPackages Instance { get; } = new(Net80, FrameworkNames.NetCoreApp, NETCoreApp70.Instance)
        {
            { "System.Collections.Immutable", "8.0.0" },
            { "System.Diagnostics.DiagnosticSource", "8.0.1" },
            { "System.Formats.Asn1", "8.0.1" },
            { "System.Net.Http.Json", "8.0.1" },
            { "System.Reflection.Metadata", "8.0.1" },
            { "System.Text.Encoding.CodePages", "8.0.0" },
            { "System.Text.Encodings.Web", "8.0.0" },
            { "System.Text.Json", "8.0.5" },
            { "System.Threading.Channels", "8.0.0" },
            { "System.Threading.Tasks.Dataflow", "8.0.1" },
        };

        internal static FrameworkPackages AspNetCore { get; } = new(Net80, FrameworkNames.AspNetCoreApp, NETCoreApp70.AspNetCore)
        {
            { "Microsoft.AspNetCore", "8.0.0" },
            { "Microsoft.AspNetCore.Antiforgery", "8.0.0" },
            { "Microsoft.AspNetCore.Authentication", "8.0.0" },
            { "Microsoft.AspNetCore.Authentication.Abstractions", "8.0.0" },
            { "Microsoft.AspNetCore.Authentication.BearerToken", "8.0.0" },
            { "Microsoft.AspNetCore.Authentication.Cookies", "8.0.0" },
            { "Microsoft.AspNetCore.Authentication.Core", "8.0.0" },
            { "Microsoft.AspNetCore.Authentication.OAuth", "8.0.0" },
            { "Microsoft.AspNetCore.Authorization", "8.0.0" },
            { "Microsoft.AspNetCore.Authorization.Policy", "8.0.0" },
            { "Microsoft.AspNetCore.Components", "8.0.0" },
            { "Microsoft.AspNetCore.Components.Authorization", "8.0.0" },
            { "Microsoft.AspNetCore.Components.Endpoints", "8.0.0" },
            { "Microsoft.AspNetCore.Components.Forms", "8.0.0" },
            { "Microsoft.AspNetCore.Components.Server", "8.0.0" },
            { "Microsoft.AspNetCore.Components.Web", "8.0.0" },
            { "Microsoft.AspNetCore.Connections.Abstractions", "8.0.0" },
            { "Microsoft.AspNetCore.CookiePolicy", "8.0.0" },
            { "Microsoft.AspNetCore.Cors", "8.0.0" },
            { "Microsoft.AspNetCore.Cryptography.Internal", "8.0.0" },
            { "Microsoft.AspNetCore.Cryptography.KeyDerivation", "8.0.0" },
            { "Microsoft.AspNetCore.DataProtection", "8.0.0" },
            { "Microsoft.AspNetCore.DataProtection.Abstractions", "8.0.0" },
            { "Microsoft.AspNetCore.DataProtection.Extensions", "8.0.0" },
            { "Microsoft.AspNetCore.Diagnostics", "8.0.0" },
            { "Microsoft.AspNetCore.Diagnostics.Abstractions", "8.0.0" },
            { "Microsoft.AspNetCore.Diagnostics.HealthChecks", "8.0.0" },
            { "Microsoft.AspNetCore.HostFiltering", "8.0.0" },
            { "Microsoft.AspNetCore.Hosting", "8.0.0" },
            { "Microsoft.AspNetCore.Hosting.Abstractions", "8.0.0" },
            { "Microsoft.AspNetCore.Hosting.Server.Abstractions", "8.0.0" },
            { "Microsoft.AspNetCore.Html.Abstractions", "8.0.0" },
            { "Microsoft.AspNetCore.Http", "8.0.0" },
            { "Microsoft.AspNetCore.Http.Abstractions", "8.0.0" },
            { "Microsoft.AspNetCore.Http.Connections", "8.0.0" },
            { "Microsoft.AspNetCore.Http.Connections.Common", "8.0.0" },
            { "Microsoft.AspNetCore.Http.Extensions", "8.0.0" },
            { "Microsoft.AspNetCore.Http.Features", "8.0.0" },
            { "Microsoft.AspNetCore.Http.Results", "8.0.0" },
            { "Microsoft.AspNetCore.HttpLogging", "8.0.0" },
            { "Microsoft.AspNetCore.HttpOverrides", "8.0.0" },
            { "Microsoft.AspNetCore.HttpsPolicy", "8.0.0" },
            { "Microsoft.AspNetCore.Identity", "8.0.0" },
            { "Microsoft.AspNetCore.Localization", "8.0.0" },
            { "Microsoft.AspNetCore.Localization.Routing", "8.0.0" },
            { "Microsoft.AspNetCore.Metadata", "8.0.0" },
            { "Microsoft.AspNetCore.Mvc", "8.0.0" },
            { "Microsoft.AspNetCore.Mvc.Abstractions", "8.0.0" },
            { "Microsoft.AspNetCore.Mvc.ApiExplorer", "8.0.0" },
            { "Microsoft.AspNetCore.Mvc.Core", "8.0.0" },
            { "Microsoft.AspNetCore.Mvc.Cors", "8.0.0" },
            { "Microsoft.AspNetCore.Mvc.DataAnnotations", "8.0.0" },
            { "Microsoft.AspNetCore.Mvc.Formatters.Json", "8.0.0" },
            { "Microsoft.AspNetCore.Mvc.Formatters.Xml", "8.0.0" },
            { "Microsoft.AspNetCore.Mvc.Localization", "8.0.0" },
            { "Microsoft.AspNetCore.Mvc.Razor", "8.0.0" },
            { "Microsoft.AspNetCore.Mvc.RazorPages", "8.0.0" },
            { "Microsoft.AspNetCore.Mvc.TagHelpers", "8.0.0" },
            { "Microsoft.AspNetCore.Mvc.ViewFeatures", "8.0.0" },
            { "Microsoft.AspNetCore.OutputCaching", "8.0.0" },
            { "Microsoft.AspNetCore.RateLimiting", "8.0.0" },
            { "Microsoft.AspNetCore.Razor", "8.0.0" },
            { "Microsoft.AspNetCore.Razor.Runtime", "8.0.0" },
            { "Microsoft.AspNetCore.RequestDecompression", "8.0.0" },
            { "Microsoft.AspNetCore.ResponseCaching", "8.0.0" },
            { "Microsoft.AspNetCore.ResponseCaching.Abstractions", "8.0.0" },
            { "Microsoft.AspNetCore.ResponseCompression", "8.0.0" },
            { "Microsoft.AspNetCore.Rewrite", "8.0.0" },
            { "Microsoft.AspNetCore.Routing", "8.0.0" },
            { "Microsoft.AspNetCore.Routing.Abstractions", "8.0.0" },
            { "Microsoft.AspNetCore.Server.HttpSys", "8.0.0" },
            { "Microsoft.AspNetCore.Server.IIS", "8.0.0" },
            { "Microsoft.AspNetCore.Server.IISIntegration", "8.0.0" },
            { "Microsoft.AspNetCore.Server.Kestrel", "8.0.0" },
            { "Microsoft.AspNetCore.Server.Kestrel.Core", "8.0.0" },
            { "Microsoft.AspNetCore.Server.Kestrel.Transport.NamedPipes", "8.0.0" },
            { "Microsoft.AspNetCore.Server.Kestrel.Transport.Quic", "8.0.0" },
            { "Microsoft.AspNetCore.Server.Kestrel.Transport.Sockets", "8.0.0" },
            { "Microsoft.AspNetCore.Session", "8.0.0" },
            { "Microsoft.AspNetCore.SignalR", "8.0.0" },
            { "Microsoft.AspNetCore.SignalR.Common", "8.0.0" },
            { "Microsoft.AspNetCore.SignalR.Core", "8.0.0" },
            { "Microsoft.AspNetCore.SignalR.Protocols.Json", "8.0.0" },
            { "Microsoft.AspNetCore.StaticFiles", "8.0.0" },
            { "Microsoft.AspNetCore.WebSockets", "8.0.0" },
            { "Microsoft.AspNetCore.WebUtilities", "8.0.0" },
            { "Microsoft.Extensions.Caching.Abstractions", "8.0.0" },
            { "Microsoft.Extensions.Caching.Memory", "8.0.0" },
            { "Microsoft.Extensions.Configuration", "8.0.0" },
            { "Microsoft.Extensions.Configuration.Abstractions", "8.0.0" },
            { "Microsoft.Extensions.Configuration.Binder", "8.0.0" },
            { "Microsoft.Extensions.Configuration.CommandLine", "8.0.0" },
            { "Microsoft.Extensions.Configuration.EnvironmentVariables", "8.0.0" },
            { "Microsoft.Extensions.Configuration.FileExtensions", "8.0.0" },
            { "Microsoft.Extensions.Configuration.Ini", "8.0.0" },
            { "Microsoft.Extensions.Configuration.Json", "8.0.0" },
            { "Microsoft.Extensions.Configuration.KeyPerFile", "8.0.0" },
            { "Microsoft.Extensions.Configuration.UserSecrets", "8.0.0" },
            { "Microsoft.Extensions.Configuration.Xml", "8.0.0" },
            { "Microsoft.Extensions.DependencyInjection", "8.0.0" },
            { "Microsoft.Extensions.DependencyInjection.Abstractions", "8.0.0" },
            { "Microsoft.Extensions.Diagnostics", "8.0.0" },
            { "Microsoft.Extensions.Diagnostics.Abstractions", "8.0.0" },
            { "Microsoft.Extensions.Diagnostics.HealthChecks", "8.0.0" },
            { "Microsoft.Extensions.Diagnostics.HealthChecks.Abstractions", "8.0.0" },
            { "Microsoft.Extensions.Features", "8.0.0" },
            { "Microsoft.Extensions.FileProviders.Abstractions", "8.0.0" },
            { "Microsoft.Extensions.FileProviders.Composite", "8.0.0" },
            { "Microsoft.Extensions.FileProviders.Embedded", "8.0.0" },
            { "Microsoft.Extensions.FileProviders.Physical", "8.0.0" },
            { "Microsoft.Extensions.FileSystemGlobbing", "8.0.0" },
            { "Microsoft.Extensions.Hosting", "8.0.0" },
            { "Microsoft.Extensions.Hosting.Abstractions", "8.0.0" },
            { "Microsoft.Extensions.Http", "8.0.0" },
            { "Microsoft.Extensions.Identity.Core", "8.0.0" },
            { "Microsoft.Extensions.Identity.Stores", "8.0.0" },
            { "Microsoft.Extensions.Localization", "8.0.0" },
            { "Microsoft.Extensions.Localization.Abstractions", "8.0.0" },
            { "Microsoft.Extensions.Logging", "8.0.0" },
            { "Microsoft.Extensions.Logging.Abstractions", "8.0.0" },
            { "Microsoft.Extensions.Logging.Configuration", "8.0.0" },
            { "Microsoft.Extensions.Logging.Console", "8.0.0" },
            { "Microsoft.Extensions.Logging.Debug", "8.0.0" },
            { "Microsoft.Extensions.Logging.EventLog", "8.0.0" },
            { "Microsoft.Extensions.Logging.EventSource", "8.0.0" },
            { "Microsoft.Extensions.Logging.TraceSource", "8.0.0" },
            { "Microsoft.Extensions.ObjectPool", "8.0.0" },
            { "Microsoft.Extensions.Options", "8.0.0" },
            { "Microsoft.Extensions.Options.ConfigurationExtensions", "8.0.0" },
            { "Microsoft.Extensions.Options.DataAnnotations", "8.0.0" },
            { "Microsoft.Extensions.Primitives", "8.0.0" },
            { "Microsoft.Extensions.WebEncoders", "8.0.0" },
            { "Microsoft.JSInterop", "8.0.0" },
            { "Microsoft.Net.Http.Headers", "8.0.0" },
            { "System.Diagnostics.EventLog", "8.0.0" },
            { "System.IO.Pipelines", "8.0.0" },
            { "System.Security.Cryptography.Pkcs", "8.0.0" },
            { "System.Security.Cryptography.Xml", "8.0.0" },
            { "System.Threading.RateLimiting", "8.0.0" },
        };

        internal static FrameworkPackages WindowsDesktop { get; } = new(Net80, FrameworkNames.WindowsDesktopApp, NETCoreApp70.WindowsDesktop)
        {
            { "Microsoft.Win32.Registry.AccessControl", "8.0.0" },
            { "Microsoft.Win32.SystemEvents", "8.0.0" },
            { "System.CodeDom", "8.0.0" },
            { "System.Configuration.ConfigurationManager", "8.0.0" },
            { "System.Diagnostics.EventLog", "8.0.0" },
            { "System.Diagnostics.PerformanceCounter", "8.0.0" },
            { "System.Drawing.Common", "8.0.0" },
            { "System.IO.Packaging", "8.0.0" },
            { "System.Resources.Extensions", "8.0.0" },
            { "System.Security.Cryptography.Pkcs", "8.0.0" },
            { "System.Security.Cryptography.ProtectedData", "8.0.0" },
            { "System.Security.Cryptography.Xml", "8.0.0" },
            { "System.Security.Permissions", "8.0.0" },
            { "System.Threading.AccessControl", "8.0.0" },
            { "System.Windows.Extensions", "8.0.0" },
        };

        internal static void Register() => FrameworkPackages.Register(Instance, AspNetCore, WindowsDesktop);
    }
}
