using System;
using System.Collections.Generic;
using System.Linq;

namespace PepperDash.Essentials.Devices.Common.VideoCodec.Interfaces
{
  /// <summary>
  /// Represents a CodecParticipants
  /// </summary>
  public class CodecParticipants
  {
    private List<Participant> _currentParticipants;

    /// <summary>
    /// Gets or sets the CurrentParticipants
    /// </summary>
    public List<Participant> CurrentParticipants
    {
      get { return _currentParticipants; }
      set
      {
        _currentParticipants = value;
        OnParticipantsChanged();
      }
    }

    /// <summary>
    /// Gets the Host participant
    /// </summary>
    public Participant Host
    {
      get
      {
        return _currentParticipants.FirstOrDefault(p => p.IsHost);
      }
    }

    /// <summary>
    /// Event fired when the participants list has changed
    /// </summary>
    public event EventHandler<EventArgs> ParticipantsListHasChanged;

    /// <summary>
    /// Initializes a new instance of the CodecParticipants class
    /// </summary>
    public CodecParticipants()
    {
      _currentParticipants = new List<Participant>();
    }

    /// <summary>
    /// OnParticipantsChanged method
    /// </summary>
    public void OnParticipantsChanged()
    {
      var handler = ParticipantsListHasChanged;

      if (handler == null) return;

      handler(this, new EventArgs());
    }
  }
}