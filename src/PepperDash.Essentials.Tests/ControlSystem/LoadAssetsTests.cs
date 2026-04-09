using System.IO.Compression;
using FluentAssertions;
using Xunit;

namespace PepperDash.Essentials.Tests.ControlSystem;

/// <summary>
/// Tests for <see cref="AssetLoader.Load"/>.
/// <see cref="AssetLoader"/> is the <c>System.IO</c>-based implementation that backs
/// <see cref="AssetLoader.Load"/>. Tests run against a
/// temporary directory tree so no Crestron runtime is required.
/// Debug is initialised with fakes via TestInitializer.
/// </summary>
public sealed class LoadAssetsTests : IDisposable
{
    // ---------------------------------------------------------------------------
    // Fixture: each test gets an isolated temp directory tree
    //
    //   _rootDir/
    //     appdir/          ← applicationDirectoryPath (where the loader scans)
    //     user/
    //       program1/      ← filePathPrefix (where assets land)
    // ---------------------------------------------------------------------------

    private readonly string _rootDir;
    private readonly string _appDir;
    private readonly string _filePathPrefix;

    public LoadAssetsTests()
    {
        _rootDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        _appDir = Path.Combine(_rootDir, "appdir");
        _filePathPrefix = Path.Combine(_rootDir, "user", "program1") + Path.DirectorySeparatorChar;

        Directory.CreateDirectory(_appDir);
        Directory.CreateDirectory(_filePathPrefix);
    }

    public void Dispose()
    {
        if (Directory.Exists(_rootDir))
            Directory.Delete(_rootDir, recursive: true);
    }

    // ---------------------------------------------------------------------------
    // Helpers
    // ---------------------------------------------------------------------------

    private static byte[] EmptyZip()
    {
        using var ms = new MemoryStream();
        using (var _ = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true)) { }
        return ms.ToArray();
    }

    private static byte[] ZipWithFile(string entryName, string content = "test")
    {
        using var ms = new MemoryStream();
        using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
        {
            var entry = archive.CreateEntry(entryName);
            using var sw = new StreamWriter(entry.Open());
            sw.Write(content);
        }
        return ms.ToArray();
    }

    private static byte[] ZipWithDirectory(string directoryEntryName)
    {
        // Directory entries have a trailing slash and an empty Name
        var normalised = directoryEntryName.TrimEnd('/') + '/';
        using var ms = new MemoryStream();
        using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
        {
            archive.CreateEntry(normalised);
        }
        return ms.ToArray();
    }

    private static byte[] ZipWithTraversalEntry()
    {
        using var ms = new MemoryStream();
        using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
        {
            var entry = archive.CreateEntry("../traversal.txt");
            using var sw = new StreamWriter(entry.Open());
            sw.Write("should not appear");
        }
        return ms.ToArray();
    }

    private void WriteToAppDir(string fileName, byte[] contents) =>
        File.WriteAllBytes(Path.Combine(_appDir, fileName), contents);

    private void WriteTextToAppDir(string fileName, string text) =>
        File.WriteAllText(Path.Combine(_appDir, fileName), text);

    // ---------------------------------------------------------------------------
    // No-op cases — nothing in the application directory
    // ---------------------------------------------------------------------------

    [Fact]
    public void LoadAssets_EmptyApplicationDirectory_DoesNotThrow()
    {
        var act = () => AssetLoader.Load(_appDir, _filePathPrefix);
        act.Should().NotThrow();
    }

    // ---------------------------------------------------------------------------
    // assets*.zip
    // ---------------------------------------------------------------------------

    [Fact]
    public void LoadAssets_MultipleAssetsZips_ThrowsException()
    {
        WriteToAppDir("assets1.zip", EmptyZip());
        WriteToAppDir("assets2.zip", EmptyZip());

        var act = () => AssetLoader.Load(_appDir, _filePathPrefix);
        act.Should().Throw<Exception>().WithMessage("*Multiple assets zip files*");
    }

    [Fact]
    public void LoadAssets_SingleAssetsZip_ExtractsFileToFilePathPrefix()
    {
        WriteToAppDir("assets.zip", ZipWithFile("config.json", "{}"));

        AssetLoader.Load(_appDir, _filePathPrefix);

        File.Exists(Path.Combine(_filePathPrefix, "config.json")).Should().BeTrue();
    }

    [Fact]
    public void LoadAssets_SingleAssetsZip_FileContentsArePreserved()
    {
        const string expected = "{\"key\":\"value\"}";
        WriteToAppDir("assets.zip", ZipWithFile("data.json", expected));

        AssetLoader.Load(_appDir, _filePathPrefix);

        File.ReadAllText(Path.Combine(_filePathPrefix, "data.json")).Should().Be(expected);
    }

    [Fact]
    public void LoadAssets_SingleAssetsZip_ZipIsDeletedAfterExtraction()
    {
        var zipPath = Path.Combine(_appDir, "assets.zip");
        WriteToAppDir("assets.zip", ZipWithFile("file.txt"));

        AssetLoader.Load(_appDir, _filePathPrefix);

        File.Exists(zipPath).Should().BeFalse("assets zip should be cleaned up after extraction");
    }

    [Fact]
    public void LoadAssets_AssetsZipWithDirectoryEntry_CreatesDirectory()
    {
        WriteToAppDir("assets.zip", ZipWithDirectory("subdir/"));

        AssetLoader.Load(_appDir, _filePathPrefix);

        Directory.Exists(Path.Combine(_filePathPrefix, "subdir/")).Should().BeTrue();
    }

    [Fact]
    public void LoadAssets_AssetsZipWithPathTraversal_ThrowsInvalidOperationException()
    {
        WriteToAppDir("assets.zip", ZipWithTraversalEntry());

        var act = () => AssetLoader.Load(_appDir, _filePathPrefix);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*trying to extract outside of the target directory*");
    }

    // ---------------------------------------------------------------------------
    // htmlassets*.zip
    // ---------------------------------------------------------------------------

    [Fact]
    public void LoadAssets_MultipleHtmlAssetsZips_ThrowsException()
    {
        WriteToAppDir("htmlassets1.zip", EmptyZip());
        WriteToAppDir("htmlassets2.zip", EmptyZip());

        var act = () => AssetLoader.Load(_appDir, _filePathPrefix);
        act.Should().Throw<Exception>().WithMessage("*Multiple htmlassets zip files*");
    }

    [Fact]
    public void LoadAssets_SingleHtmlAssetsZip_ExtractsToHtmlDirectory()
    {
        // htmlDir = rootDir/html
        WriteToAppDir("htmlassets.zip", ZipWithFile("index.html", "<html/>"));

        AssetLoader.Load(_appDir, _filePathPrefix);

        var expectedHtmlDir = Path.Combine(_rootDir, "html");
        File.Exists(Path.Combine(expectedHtmlDir, "index.html")).Should().BeTrue();
    }

    [Fact]
    public void LoadAssets_SingleHtmlAssetsZip_ZipIsDeletedAfterExtraction()
    {
        var zipPath = Path.Combine(_appDir, "htmlassets.zip");
        WriteToAppDir("htmlassets.zip", ZipWithFile("page.html"));

        AssetLoader.Load(_appDir, _filePathPrefix);

        File.Exists(zipPath).Should().BeFalse();
    }

    [Fact]
    public void LoadAssets_HtmlAssetsZipWithPathTraversal_ThrowsInvalidOperationException()
    {
        WriteToAppDir("htmlassets.zip", ZipWithTraversalEntry());

        var act = () => AssetLoader.Load(_appDir, _filePathPrefix);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*trying to extract outside of the target directory*");
    }

    [Fact]
    public void LoadAssets_HtmlAssetsZip_ShallowFilePathPrefixThrowsWhenRootCannotBeDetermined()
    {
        // A path that is exactly ONE level below the filesystem root has no grandparent,
        // so rootDir (programDir.Parent.Parent) is null and the guard should throw.
        // DirectoryInfo works with non-existent paths, so we don't create the directory.
        var filesystemRoot = Path.GetPathRoot(Path.GetTempPath())!;
        var shallowPrefix = Path.Combine(filesystemRoot, "pepperDashTestShallow") + Path.DirectorySeparatorChar;
        WriteToAppDir("htmlassets.zip", ZipWithFile("page.html"));

        var act = () => AssetLoader.Load(_appDir, shallowPrefix);
        act.Should().Throw<Exception>().WithMessage("*Unable to determine root directory for html extraction*");
    }

    // ---------------------------------------------------------------------------
    // essentials-devtools*.zip
    // ---------------------------------------------------------------------------

    [Fact]
    public void LoadAssets_MultipleDevToolsZips_ThrowsException()
    {
        WriteToAppDir("essentials-devtools1.zip", EmptyZip());
        WriteToAppDir("essentials-devtools2.zip", EmptyZip());

        var act = () => AssetLoader.Load(_appDir, _filePathPrefix);
        act.Should().Throw<Exception>().WithMessage("*Multiple essentials-devtools zip files*");
    }

    [Fact]
    public void LoadAssets_SingleDevToolsZip_ExtractsToHtmlDebugDirectory()
    {
        // debugDir = rootDir/html/debug
        WriteToAppDir("essentials-devtools.zip", ZipWithFile("app.js", "console.log('hi');"));

        AssetLoader.Load(_appDir, _filePathPrefix);

        var expectedDebugDir = Path.Combine(_rootDir, "html", "debug");
        File.Exists(Path.Combine(expectedDebugDir, "app.js")).Should().BeTrue();
    }

    [Fact]
    public void LoadAssets_SingleDevToolsZip_ZipIsDeletedAfterExtraction()
    {
        var zipPath = Path.Combine(_appDir, "essentials-devtools.zip");
        WriteToAppDir("essentials-devtools.zip", ZipWithFile("tool.js"));

        AssetLoader.Load(_appDir, _filePathPrefix);

        File.Exists(zipPath).Should().BeFalse();
    }

    [Fact]
    public void LoadAssets_DevToolsZipWithPathTraversal_ThrowsInvalidOperationException()
    {
        WriteToAppDir("essentials-devtools.zip", ZipWithTraversalEntry());

        var act = () => AssetLoader.Load(_appDir, _filePathPrefix);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*trying to extract outside of the target directory*");
    }

    // ---------------------------------------------------------------------------
    // *configurationFile*.json
    // ---------------------------------------------------------------------------

    [Fact]
    public void LoadAssets_MultipleJsonConfigFiles_ThrowsException()
    {
        WriteTextToAppDir("abcconfigurationFile1.json", "{}");
        WriteTextToAppDir("abcconfigurationFile2.json", "{}");

        var act = () => AssetLoader.Load(_appDir, _filePathPrefix);
        act.Should().Throw<Exception>().WithMessage("*Multiple configuration files found*");
    }

    [Fact]
    public void LoadAssets_SingleJsonConfigFile_IsMovedToFilePathPrefix()
    {
        const string fileName = "myconfigurationFile.json";
        WriteTextToAppDir(fileName, "{}");

        AssetLoader.Load(_appDir, _filePathPrefix);

        File.Exists(Path.Combine(_appDir, fileName)).Should().BeFalse("source file should be moved, not copied");
        File.Exists(Path.Combine(_filePathPrefix, fileName)).Should().BeTrue("file should exist at the file path prefix");
    }

    [Fact]
    public void LoadAssets_SingleJsonConfigFile_ExistingDestinationIsReplaced()
    {
        const string fileName = "myconfigurationFile.json";
        WriteTextToAppDir(fileName, "new content");

        // Pre-populate the destination with stale content
        File.WriteAllText(Path.Combine(_filePathPrefix, fileName), "old content");

        AssetLoader.Load(_appDir, _filePathPrefix);

        File.ReadAllText(Path.Combine(_filePathPrefix, fileName)).Should().Be("new content");
    }

    [Fact]
    public void LoadAssets_SingleJsonConfigFile_ContentIsPreserved()
    {
        const string content = "{\"devices\":[]}";
        const string fileName = "myconfigurationFile.json";
        WriteTextToAppDir(fileName, content);

        AssetLoader.Load(_appDir, _filePathPrefix);

        File.ReadAllText(Path.Combine(_filePathPrefix, fileName)).Should().Be(content);
    }
}
