using System;
using System.Collections.Generic;
using System.Linq;

namespace PepperDash.Essentials.Devices.Common.VideoCodec.Interfaces
{
    public class CodecParticipants
    {
        private List<Participant> _currentParticipants;

        public List<Participant> CurrentParticipants
        {
            get { return _currentParticipants; }
            set
            {
                _currentParticipants = value;
                OnParticipantsChanged();
            }
        }

        public Participant Host
        {
            get
            {
                return _currentParticipants.FirstOrDefault(p => p.IsHost);
            }
        }

        public event EventHandler<EventArgs> ParticipantsListHasChanged;

        public CodecParticipants()
        {
            _currentParticipants = new List<Participant>();
        }

        public void OnParticipantsChanged()
        {
            var handler = ParticipantsListHasChanged;

            if (handler == null) return;

            handler(this, new EventArgs());
        }
    }
}