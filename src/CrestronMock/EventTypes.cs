using System;

namespace Crestron.SimplSharp
{
  /// <summary>Mock eProgramStatusEventType enumeration</summary>
  public enum eProgramStatusEventType
  {
    /// <summary>Program stopping</summary>
    eProgramStopping = 0,
    /// <summary>Program started</summary>
    eProgramStarted = 1,
    /// <summary>Program paused</summary>
    eProgramPaused = 2,
    /// <summary>Program resumed</summary>
    eProgramResumed = 3
  }

  /// <summary>Mock EthernetEventArgs class</summary>
  public class EthernetEventArgs : EventArgs
  {
    /// <summary>Gets the Ethernet adapter that triggered the event</summary>
    public int EthernetAdapter { get; private set; }

    /// <summary>Gets the link status</summary>
    public bool LinkUp { get; private set; }

    /// <summary>Gets the speed in Mbps</summary>
    public int Speed { get; private set; }

    /// <summary>Gets whether it's full duplex</summary>
    public bool FullDuplex { get; private set; }

    /// <summary>Gets the ethernet event type</summary>
    public eEthernetEventType EthernetEventType { get; private set; }

    /// <summary>Initializes a new instance of EthernetEventArgs</summary>
    /// <param name="adapter">Ethernet adapter number</param>
    /// <param name="linkUp">Link status</param>
    /// <param name="speed">Speed in Mbps</param>
    /// <param name="fullDuplex">Full duplex status</param>
    public EthernetEventArgs(int adapter, bool linkUp, int speed, bool fullDuplex)
    {
      EthernetAdapter = adapter;
      LinkUp = linkUp;
      Speed = speed;
      FullDuplex = fullDuplex;
      EthernetEventType = linkUp ? eEthernetEventType.LinkUp : eEthernetEventType.LinkDown;
    }

    /// <summary>Default constructor</summary>
    public EthernetEventArgs() : this(0, false, 0, false)
    {
    }
  }
}

namespace Crestron.SimplSharp.CrestronIO
{
  /// <summary>Mock FileInfo class for basic file operations</summary>
  public class FileInfo
  {
    /// <summary>Gets the full path of the file</summary>
    public string FullName { get; private set; }

    /// <summary>Gets the name of the file</summary>
    public string Name { get; private set; }

    /// <summary>Gets the directory name</summary>
    public string? DirectoryName { get; private set; }

    /// <summary>Gets whether the file exists</summary>
    public bool Exists { get; private set; }

    /// <summary>Gets the length of the file in bytes</summary>
    public long Length { get; private set; }

    /// <summary>Gets the creation time</summary>
    public DateTime CreationTime { get; private set; }

    /// <summary>Gets the last write time</summary>
    public DateTime LastWriteTime { get; private set; }

    /// <summary>Gets the last access time</summary>
    public DateTime LastAccessTime { get; private set; }

    /// <summary>Initializes a new instance of FileInfo</summary>
    /// <param name="fileName">Path to the file</param>
    public FileInfo(string fileName)
    {
      FullName = fileName ?? string.Empty;
      Name = System.IO.Path.GetFileName(fileName) ?? string.Empty;
      DirectoryName = System.IO.Path.GetDirectoryName(fileName);

      // Mock file properties
      Exists = !string.IsNullOrEmpty(fileName);
      Length = 0;
      CreationTime = DateTime.Now;
      LastWriteTime = DateTime.Now;
      LastAccessTime = DateTime.Now;
    }

    /// <summary>Deletes the file</summary>
    public void Delete()
    {
      // Mock implementation - just mark as not existing
      Exists = false;
    }

    /// <summary>Creates a text file or opens an existing one for writing</summary>
    /// <returns>A mock StreamWriter</returns>
    public System.IO.StreamWriter CreateText()
    {
      var stream = new System.IO.MemoryStream();
      return new System.IO.StreamWriter(stream);
    }

    /// <summary>Opens an existing file for reading</summary>
    /// <returns>A mock FileStream</returns>
    public System.IO.FileStream OpenRead()
    {
      // Mock implementation - return a memory stream wrapped as FileStream
      return new System.IO.FileStream(FullName, System.IO.FileMode.Open, System.IO.FileAccess.Read);
    }
  }
}


