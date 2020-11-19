using System;
using System.Collections.Generic;

namespace PepperDash.Essentials.Devices.Common.VideoCodec.Interfaces
{
    public interface IHasParticipants
    {
        CodecParticipants Participants { get; }
    }

    public interface IHasParticipantVideoMute:IHasParticipants
    {
        void MuteVideoForParticipant(int userId);
        void UnmuteVideoForParticipant(int userId);
        void ToggleVideoForParticipant(int userId);
    }

    public interface IHasParticipantAudioMute:IHasParticipantVideoMute
    {
        void MuteAudioForParticipant(int userId);
        void UnmuteAudioForParticipant(int userId);
        void ToggleAudioForParticipant(int userId);
    }

    public class CodecParticipants
    {
        private List<Participant> _currentParticipants;
 
        public List<Participant> CurrentParticipants {
            get { return _currentParticipants; }
            set
            {
                _currentParticipants = value;
                var handler = ParticipantsListHasChanged;

                if(handler == null) return;

                handler(this, new EventArgs());
            }
        }

        public event EventHandler<EventArgs> ParticipantsListHasChanged;

        public CodecParticipants()
        {
            _currentParticipants = new List<Participant>();
        }
    }

    public class Participant
    {
        public bool IsHost { get; set; }
        public string Name { get; set; }
        public bool CanMuteVideo { get; set; }       
        public bool CanUnmuteVideo { get; set; }
        public bool VideoMuteFb { get; set; }
        public bool AudioMuteFb { get; set; }
    }
}