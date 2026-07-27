using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Wox.Plugin.Runner.Infrastructure;

internal static class ExecutablePathResolver
{
    private static readonly string[] DefaultExtensions = new[] { ".exe", ".com", ".bat", ".cmd" };

    public static bool CanResolve(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return false;

        var candidate = Normalize(path);

        if (Uri.TryCreate(candidate, UriKind.Absolute, out var uri) && !uri.IsFile)
            return true;

        candidate = Environment.ExpandEnvironmentVariables(candidate);

        if (File.Exists(candidate))
            return true;

        if (Path.IsPathRooted(candidate))
            return false;

        // If a relative path contains directory segments, Windows won't search PATH for it.
        // Avoid treating it as a bare executable name (which can cause false positives).
        if (!string.IsNullOrEmpty(Path.GetDirectoryName(candidate)))
            return false;

        var fileName = Path.GetFileName(candidate);
        if (string.IsNullOrWhiteSpace(fileName))
            return false;
        var extensions = GetExtensions(fileName);
        foreach (var directory in GetSearchDirectories())
        {
            var baseCandidate = Path.Combine(directory, fileName);
            if (File.Exists(baseCandidate))
                return true;

            foreach (var extension in extensions)
            {
                if (extension.Length == 0)
                    continue;

                if (File.Exists(baseCandidate + extension))
                    return true;
            }
        }

        return false;
    }

private static IEnumerable<string> GetSearchDirectories()
    => (Environment.GetEnvironmentVariable("PATH") ?? string.Empty)
        .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
        .Select(dir => Environment.ExpandEnvironmentVariables(Normalize(dir)))
        .Where(dir => !string.IsNullOrWhiteSpace(dir));

    private static IEnumerable<string> GetExtensions(string fileName)
    {
        if (Path.HasExtension(fileName))
            return new[] { string.Empty };

        var pathext = Environment.GetEnvironmentVariable("PATHEXT");
        return string.IsNullOrWhiteSpace(pathext)
            ? DefaultExtensions
            : pathext
                .Split(';', StringSplitOptions.RemoveEmptyEntries)
                .Select(ext => ext.StartsWith('.') ? ext : $".{ext.TrimStart('.')}");
    }

    private static string Normalize(string value) => value.Trim().Trim('"');
}
