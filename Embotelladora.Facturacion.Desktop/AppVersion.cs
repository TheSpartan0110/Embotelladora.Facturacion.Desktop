using System.Reflection;

namespace Embotelladora.Facturacion.Desktop;

internal static class AppVersion
{
    private static string? _commitHash;
    private static string? _displayVersion;

    public static string CommitHash => _commitHash ??= ResolveCommitHash();

    public static string DisplayVersion => _displayVersion ??= BuildDisplayVersion();

    private static string ResolveCommitHash()
    {
        var info = Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion ?? string.Empty;

        // Format: "1.0.0+<hash>" — extract after '+'
        var plusIndex = info.IndexOf('+');
        if (plusIndex >= 0 && plusIndex < info.Length - 1)
        {
            return info[(plusIndex + 1)..];
        }

        return "dev";
    }

    private static string BuildDisplayVersion()
    {
        var hash = CommitHash;
        return hash is "unknown" or "dev"
            ? "v0.1 · local"
            : $"v0.1 · {hash}";
    }
}
