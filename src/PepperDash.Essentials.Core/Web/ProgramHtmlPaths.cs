// This file is also compiled into PepperDash.Essentials.Core.Tests, which enables nullable
// reference types. Core does not, so the annotations here would be read inconsistently between the
// two. Disabling explicitly keeps the behaviour identical in both, and is a no-op in Core.
#nullable disable

using System.IO;

namespace PepperDash.Essentials.Core.Web;

/// <summary>
/// Works out where a program's HTML content (the htmlassets bundle and the Essentials Dev Tools)
/// belongs, from <c>Global.FilePathPrefix</c>. Both extraction at startup and the Dev Tools request
/// handler use this, so they always agree on the folder.
/// </summary>
/// <remarks>
/// <para>On a 4-series appliance the file path prefix is <c>{root}/user/programNN/</c>, so the HTML
/// folder is two levels up: <c>{root}/html</c>.</para>
/// <para>On Virtual Control it is <c>{room}/User/</c>, with no program folder, so the HTML folder is
/// one level up: <c>{room}/html</c>. VC-4 links <c>html</c> to the room's <c>Html</c> folder, the same
/// way it links <c>user</c> to <c>User</c>. Walking up two levels there would land in the folder that
/// holds every room, shared by all of them.</para>
/// </remarks>
public static class ProgramHtmlPaths
{
    /// <summary>
    /// Returns the HTML folder for the program, or null if it can't be worked out from the prefix.
    /// </summary>
    /// <param name="filePathPrefix">The program's file path prefix, e.g. <c>/user/program1/</c>.</param>
    /// <param name="isVirtualControl">True when running on Virtual Control (VC-4).</param>
    public static string GetHtmlRoot(string filePathPrefix, bool isVirtualControl)
    {
        if (string.IsNullOrWhiteSpace(filePathPrefix))
            return null;

        var programDir = new DirectoryInfo(
            filePathPrefix.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));

        // 4-series: {root}/user/programNN -> {root}. VC-4: {room}/User -> {room}.
        var rootDir = isVirtualControl ? programDir.Parent : programDir.Parent?.Parent;

        return rootDir == null ? null : Path.Combine(rootDir.FullName, "html");
    }

    /// <summary>
    /// Returns the folder the Essentials Dev Tools are extracted to and served from, or null if it
    /// can't be worked out from the prefix.
    /// </summary>
    /// <param name="filePathPrefix">The program's file path prefix, e.g. <c>/user/program1/</c>.</param>
    /// <param name="isVirtualControl">True when running on Virtual Control (VC-4).</param>
    public static string GetDevToolsRoot(string filePathPrefix, bool isVirtualControl)
    {
        var htmlRoot = GetHtmlRoot(filePathPrefix, isVirtualControl);
        return htmlRoot == null ? null : Path.Combine(htmlRoot, "debug");
    }
}
