using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using PepperDash.Core;
using Serilog.Events;

namespace PepperDash.Essentials;

/// <summary>
/// Handles extracting embedded asset bundles and moving configuration files from the
/// application directory to the program file-path prefix at startup.
/// Implemented using <c>System.IO</c> types so it can run (and be tested) outside
/// of a Crestron runtime environment.
/// </summary>
internal static class AssetLoader
{
    /// <summary>
    /// Scans <paramref name="applicationDirectoryPath"/> for well-known zip bundles and
    /// JSON configuration files and deploys them to <paramref name="filePathPrefix"/>.
    /// </summary>
    /// <param name="applicationDirectoryPath">
    /// The directory to scan (typically the Crestron application root).
    /// </param>
    /// <param name="filePathPrefix">
    /// The program's runtime working directory (e.g. <c>/nvram/program1/</c>).
    /// </param>
    internal static void Load(string applicationDirectoryPath, string filePathPrefix)
    {
        var applicationDirectory = new DirectoryInfo(applicationDirectoryPath);
        Debug.LogMessage(LogEventLevel.Information,
            "Searching: {applicationDirectory:l} for embedded assets - {Destination}",
            applicationDirectory.FullName, filePathPrefix);

        ExtractAssetsZip(applicationDirectory, filePathPrefix);
        ExtractHtmlAssetsZip(applicationDirectory, filePathPrefix);
        ExtractDevToolsZip(applicationDirectory, filePathPrefix);
        MoveConfigurationFile(applicationDirectory, filePathPrefix);
    }

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    private static void ExtractAssetsZip(DirectoryInfo applicationDirectory, string filePathPrefix)
    {
        var zipFiles = applicationDirectory.GetFiles("assets*.zip");

        if (zipFiles.Length > 1)
            throw new Exception("Multiple assets zip files found. Cannot continue.");

        if (zipFiles.Length == 1)
        {
            var zipFile = zipFiles[0];
            var assetsRoot = Path.GetFullPath(filePathPrefix);
            if (!assetsRoot.EndsWith(Path.DirectorySeparatorChar.ToString()) &&
                !assetsRoot.EndsWith(Path.AltDirectorySeparatorChar.ToString()))
            {
                assetsRoot += Path.DirectorySeparatorChar;
            }

            Debug.LogMessage(LogEventLevel.Information,
                "Found assets zip file: {zipFile:l}... Unzipping...", zipFile.FullName);

            using (var archive = ZipFile.OpenRead(zipFile.FullName))
            {
                foreach (var entry in archive.Entries)
                {
                    var destinationPath = Path.Combine(filePathPrefix, entry.FullName);
                    var fullDest = Path.GetFullPath(destinationPath);
                    if (!fullDest.StartsWith(assetsRoot, StringComparison.OrdinalIgnoreCase))
                        throw new InvalidOperationException(
                            $"Entry '{entry.FullName}' is trying to extract outside of the target directory.");

                    if (string.IsNullOrEmpty(entry.Name))
                    {
                        Directory.CreateDirectory(destinationPath);
                        continue;
                    }

                    if (Directory.Exists(destinationPath))
                        Directory.Delete(destinationPath, recursive: true);

                    Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);
                    entry.ExtractToFile(destinationPath, overwrite: true);
                    Debug.LogMessage(LogEventLevel.Information,
                        "Extracted: {entry:l} to {Destination}", entry.FullName, destinationPath);
                }
            }
        }

        foreach (var file in zipFiles)
            File.Delete(file.FullName);
    }

    private static void ExtractHtmlAssetsZip(DirectoryInfo applicationDirectory, string filePathPrefix)
    {
        var htmlZipFiles = applicationDirectory.GetFiles("htmlassets*.zip");

        if (htmlZipFiles.Length > 1)
            throw new Exception(
                "Multiple htmlassets zip files found in application directory. " +
                "Please ensure only one htmlassets*.zip file is present and retry.");

        if (htmlZipFiles.Length == 1)
        {
            var htmlZipFile = htmlZipFiles[0];
            var programDir = new DirectoryInfo(
                filePathPrefix.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            var userOrNvramDir = programDir.Parent;
            var rootDir = userOrNvramDir?.Parent;
            if (rootDir == null)
                throw new Exception(
                    $"Unable to determine root directory for html extraction. Current path: {filePathPrefix}");

            var htmlDir = Path.Combine(rootDir.FullName, "html");
            var htmlRoot = Path.GetFullPath(htmlDir);
            if (!htmlRoot.EndsWith(Path.DirectorySeparatorChar.ToString()) &&
                !htmlRoot.EndsWith(Path.AltDirectorySeparatorChar.ToString()))
            {
                htmlRoot += Path.DirectorySeparatorChar;
            }

            Debug.LogMessage(LogEventLevel.Information,
                "Found htmlassets zip file: {zipFile:l}... Unzipping...", htmlZipFile.FullName);

            using (var archive = ZipFile.OpenRead(htmlZipFile.FullName))
            {
                foreach (var entry in archive.Entries)
                {
                    var destinationPath = Path.Combine(htmlDir, entry.FullName);
                    var fullDest = Path.GetFullPath(destinationPath);
                    if (!fullDest.StartsWith(htmlRoot, StringComparison.OrdinalIgnoreCase))
                        throw new InvalidOperationException(
                            $"Entry '{entry.FullName}' is trying to extract outside of the target directory.");

                    if (string.IsNullOrEmpty(entry.Name))
                    {
                        Directory.CreateDirectory(destinationPath);
                        continue;
                    }

                    if (File.Exists(destinationPath))
                        File.Delete(destinationPath);

                    var parentDir = Path.GetDirectoryName(destinationPath);
                    if (!string.IsNullOrEmpty(parentDir))
                        Directory.CreateDirectory(parentDir);

                    entry.ExtractToFile(destinationPath, overwrite: true);
                    Debug.LogMessage(LogEventLevel.Information,
                        "Extracted: {entry:l} to {Destination}", entry.FullName, destinationPath);
                }
            }
        }

        foreach (var file in htmlZipFiles)
            File.Delete(file.FullName);
    }

    private static void ExtractDevToolsZip(DirectoryInfo applicationDirectory, string filePathPrefix)
    {
        var devToolsZipFiles = applicationDirectory.GetFiles("essentials-devtools*.zip");

        if (devToolsZipFiles.Length > 1)
            throw new Exception(
                "Multiple essentials-devtools zip files found in application directory. " +
                "Please ensure only one essentials-devtools*.zip file is present and retry.");

        if (devToolsZipFiles.Length == 1)
        {
            var devToolsZipFile = devToolsZipFiles[0];
            var programDir = new DirectoryInfo(
                filePathPrefix.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            var userOrNvramDir = programDir.Parent;
            var rootDir = userOrNvramDir?.Parent;
            if (rootDir == null)
                throw new Exception(
                    $"Unable to determine root directory for debug html extraction. Current path: {filePathPrefix}");

            var debugDir = Path.Combine(rootDir.FullName, "html", "debug");
            var debugRoot = Path.GetFullPath(debugDir);
            if (!debugRoot.EndsWith(Path.DirectorySeparatorChar.ToString()) &&
                !debugRoot.EndsWith(Path.AltDirectorySeparatorChar.ToString()))
            {
                debugRoot += Path.DirectorySeparatorChar;
            }

            Debug.LogMessage(LogEventLevel.Information,
                "Found essentials-devtools zip file: {zipFile:l}... Unzipping to {Destination}...",
                devToolsZipFile.FullName, debugDir);

            using (var archive = ZipFile.OpenRead(devToolsZipFile.FullName))
            {
                foreach (var entry in archive.Entries)
                {
                    var destinationPath = Path.Combine(debugDir, entry.FullName);
                    var fullDest = Path.GetFullPath(destinationPath);
                    if (!fullDest.StartsWith(debugRoot, StringComparison.OrdinalIgnoreCase))
                        throw new InvalidOperationException(
                            $"Entry '{entry.FullName}' is trying to extract outside of the target directory.");

                    if (string.IsNullOrEmpty(entry.Name))
                    {
                        Directory.CreateDirectory(destinationPath);
                        continue;
                    }

                    if (File.Exists(destinationPath))
                        File.Delete(destinationPath);

                    var parentDir = Path.GetDirectoryName(destinationPath);
                    if (!string.IsNullOrEmpty(parentDir))
                        Directory.CreateDirectory(parentDir);

                    entry.ExtractToFile(destinationPath, overwrite: true);
                    Debug.LogMessage(LogEventLevel.Information,
                        "Extracted: {entry:l} to {Destination}", entry.FullName, destinationPath);
                }
            }
        }

        foreach (var file in devToolsZipFiles)
            File.Delete(file.FullName);
    }

    private static void MoveConfigurationFile(DirectoryInfo applicationDirectory, string filePathPrefix)
    {
        var jsonFiles = applicationDirectory.GetFiles("*configurationFile*.json");

        if (jsonFiles.Length > 1)
        {
            Debug.LogError("Multiple configuration files found in application directory: {@jsonFiles}",
                jsonFiles.Select(f => f.FullName).ToArray());
            throw new Exception("Multiple configuration files found. Cannot continue.");
        }

        if (jsonFiles.Length == 1)
        {
            var jsonFile = jsonFiles[0];
            var finalPath = Path.Combine(filePathPrefix, jsonFile.Name);
            Debug.LogMessage(LogEventLevel.Information,
                "Found configuration file: {jsonFile:l}... Moving to: {Destination}",
                jsonFile.FullName, finalPath);

            if (File.Exists(finalPath))
            {
                Debug.LogMessage(LogEventLevel.Information,
                    "Removing existing configuration file: {Destination}", finalPath);
                File.Delete(finalPath);
            }

            jsonFile.MoveTo(finalPath);
        }
    }
}
