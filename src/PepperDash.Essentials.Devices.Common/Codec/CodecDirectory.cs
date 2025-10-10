

using System;
using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json;

namespace PepperDash.Essentials.Devices.Common.Codec
{
  /// <summary>
  /// Represents a codec directory
  /// </summary>
  public class CodecDirectory
  {
    /// <summary>
    /// Represents the contents of the directory
    /// We don't want to serialize this for messages to MobileControl.  MC can combine Contacts and Folders to get the same data
    /// </summary>
    [JsonIgnore]
    public List<DirectoryItem> CurrentDirectoryResults { get; private set; }

    /// <summary>
    /// Gets the Contacts in the CurrentDirectoryResults
    /// </summary>
    [JsonProperty("contacts")]
    public List<DirectoryItem> Contacts
    {
      get
      {
        return CurrentDirectoryResults.OfType<DirectoryContact>().Cast<DirectoryItem>().ToList();
      }
    }

    /// <summary>
    /// Gets the Folders in the CurrentDirectoryResults
    /// </summary>
    [JsonProperty("folders")]
    public List<DirectoryItem> Folders
    {
      get
      {
        return CurrentDirectoryResults.OfType<DirectoryFolder>().Cast<DirectoryItem>().ToList();
      }
    }

    /// <summary>
    /// Used to store the ID of the current folder for CurrentDirectoryResults
    /// Gets or sets the ResultsFolderId
    /// </summary>
    [JsonProperty("resultsFolderId")]
    public string ResultsFolderId { get; set; }

    /// <summary>
    /// Constructor for <see cref="CodecDirectory"/>
    /// </summary>
    public CodecDirectory()
    {
      CurrentDirectoryResults = new List<DirectoryItem>();
    }

    /// <summary>
    /// Adds folders to the directory
    /// </summary>
    /// <param name="folders"></param>        
    public void AddFoldersToDirectory(List<DirectoryItem> folders)
    {
      if (folders != null)
        CurrentDirectoryResults.AddRange(folders);

      SortDirectory();
    }

    /// <summary>
    /// Adds contacts to the directory
    /// </summary>
    /// <param name="contacts"></param>        
    public void AddContactsToDirectory(List<DirectoryItem> contacts)
    {
      if (contacts != null)
        CurrentDirectoryResults.AddRange(contacts);

      SortDirectory();
    }

    /// <summary>
    /// Filters the CurrentDirectoryResults by the predicate
    /// </summary>
    /// <param name="predicate"></param>

    public void FilterContacts(Func<DirectoryItem, bool> predicate)
    {
      CurrentDirectoryResults = CurrentDirectoryResults.Where(predicate).ToList();
    }

    /// <summary>
    /// Sorts the DirectoryResults list to display all folders alphabetically, then all contacts alphabetically
    /// </summary>
    private void SortDirectory()
    {
      var sortedFolders = new List<DirectoryItem>();

      sortedFolders.AddRange(CurrentDirectoryResults.Where(f => f is DirectoryFolder));

      sortedFolders.OrderBy(f => f.Name);

      var sortedContacts = new List<DirectoryItem>();

      sortedContacts.AddRange(CurrentDirectoryResults.Where(c => c is DirectoryContact));

      sortedFolders.OrderBy(c => c.Name);

      CurrentDirectoryResults.Clear();

      CurrentDirectoryResults.AddRange(sortedFolders);

      CurrentDirectoryResults.AddRange(sortedContacts);
    }

  }
}