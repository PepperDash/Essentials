using System;
using System.IO;
using System.Linq;

namespace PepperDash.Essentials
{
    /// <summary>
    /// Determines whether the touchpanel wrapper app (the mobile control React app) deployed to this
    /// processor's <c>mcUserApp</c> folder matches the version configured in the system config's
    /// <c>versions.touchpanelWrapperApp</c>.
    /// </summary>
    /// <remarks>
    /// This reads the app's built .js/.html files directly from disk, so unlike the previous
    /// client-self-reported-version approach, it does not depend on any UI client being connected and
    /// does not accumulate per-client state over the life of the program.
    /// </remarks>
    public static class TouchpanelWrapperAppVersionChecker
    {
        private static readonly string[] SearchPatterns = { "*.js", "*.html" };

        /// <summary>
        /// Scans the deployed touchpanel wrapper app's .js/.html files under <paramref name="appPath"/>
        /// for the literal <paramref name="expectedVersion"/> string (the version build tooling bakes
        /// into the app at build time).
        /// </summary>
        /// <param name="appPath">The path to the deployed mcUserApp folder</param>
        /// <param name="expectedVersion">The expected version, from config's versions.touchpanelWrapperApp.version</param>
        public static TouchpanelWrapperAppVersionCheckResult CheckDeployedVersion(string appPath, string expectedVersion)
        {
            if (string.IsNullOrEmpty(expectedVersion))
            {
                return TouchpanelWrapperAppVersionCheckResult.NotConfigured(appPath);
            }

            if (string.IsNullOrEmpty(appPath) || !Directory.Exists(appPath))
            {
                return TouchpanelWrapperAppVersionCheckResult.AppNotDeployed(appPath, expectedVersion);
            }

            try
            {
                var files = SearchPatterns
                    .SelectMany(pattern => Directory.GetFiles(appPath, pattern, SearchOption.AllDirectories))
                    .ToList();

                if (files.Count == 0)
                {
                    return TouchpanelWrapperAppVersionCheckResult.AppNotDeployed(appPath, expectedVersion);
                }

                foreach (var file in files)
                {
                    var contents = File.ReadAllText(file);
                    if (contents.IndexOf(expectedVersion, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        return TouchpanelWrapperAppVersionCheckResult.Match(appPath, expectedVersion, file);
                    }
                }

                return TouchpanelWrapperAppVersionCheckResult.Mismatch(appPath, expectedVersion, files.Count);
            }
            catch (Exception ex)
            {
                return TouchpanelWrapperAppVersionCheckResult.Error(appPath, expectedVersion, ex.Message);
            }
        }
    }

    /// <summary>
    /// The outcome of a <see cref="TouchpanelWrapperAppVersionChecker.CheckDeployedVersion"/> check
    /// </summary>
    public class TouchpanelWrapperAppVersionCheckResult
    {
        /// <summary>
        /// The mcUserApp path that was checked
        /// </summary>
        public string AppPath { get; }

        /// <summary>
        /// The expected version from config, if any was configured
        /// </summary>
        public string ExpectedVersion { get; }

        /// <summary>
        /// True if any files were found deployed at <see cref="AppPath"/>
        /// </summary>
        public bool AppDeployed { get; }

        /// <summary>
        /// True if <see cref="ExpectedVersion"/> was found in one of the deployed files
        /// </summary>
        public bool VersionMatched { get; }

        /// <summary>
        /// The file the expected version was found in, if <see cref="VersionMatched"/> is true
        /// </summary>
        public string MatchedFile { get; }

        /// <summary>
        /// The number of files scanned
        /// </summary>
        public int FilesScanned { get; }

        /// <summary>
        /// A human-readable summary of the outcome, suitable for console output
        /// </summary>
        public string Summary { get; }

        private TouchpanelWrapperAppVersionCheckResult(string appPath, string expectedVersion, bool appDeployed,
            bool versionMatched, string matchedFile, int filesScanned, string summary)
        {
            AppPath = appPath;
            ExpectedVersion = expectedVersion;
            AppDeployed = appDeployed;
            VersionMatched = versionMatched;
            MatchedFile = matchedFile;
            FilesScanned = filesScanned;
            Summary = summary;
        }

        internal static TouchpanelWrapperAppVersionCheckResult NotConfigured(string appPath) =>
            new TouchpanelWrapperAppVersionCheckResult(appPath, null, false, false, null, 0,
                "versions.touchpanelWrapperApp.version is not configured; skipping check");

        internal static TouchpanelWrapperAppVersionCheckResult AppNotDeployed(string appPath, string expectedVersion) =>
            new TouchpanelWrapperAppVersionCheckResult(appPath, expectedVersion, false, false, null, 0,
                $"No app files found at '{appPath}'; expected version {expectedVersion}");

        internal static TouchpanelWrapperAppVersionCheckResult Match(string appPath, string expectedVersion, string matchedFile) =>
            new TouchpanelWrapperAppVersionCheckResult(appPath, expectedVersion, true, true, matchedFile, 1,
                $"Deployed app matches configured version {expectedVersion} (found in '{matchedFile}')");

        internal static TouchpanelWrapperAppVersionCheckResult Mismatch(string appPath, string expectedVersion, int filesScanned) =>
            new TouchpanelWrapperAppVersionCheckResult(appPath, expectedVersion, true, false, null, filesScanned,
                $"Deployed app at '{appPath}' does NOT contain expected version {expectedVersion} ({filesScanned} file(s) scanned) - MISMATCH");

        internal static TouchpanelWrapperAppVersionCheckResult Error(string appPath, string expectedVersion, string errorMessage) =>
            new TouchpanelWrapperAppVersionCheckResult(appPath, expectedVersion, false, false, null, 0,
                $"Error checking deployed app version at '{appPath}': {errorMessage}");
    }
}
