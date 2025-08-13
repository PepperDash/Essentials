using System;
using System.Collections.Generic;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Devices.Common.Codec;

namespace PepperDash.Essentials.AppServer.Messengers.Tests
{
    /// <summary>
    /// Mock device for testing CallStatusMessenger that implements IHasDialer without VideoCodecBase
    /// </summary>
    public class MockCallStatusDevice : EssentialsDevice, IHasDialer, IHasContentSharing
    {
        public event EventHandler<CodecCallStatusItemChangeEventArgs> CallStatusChange;

        private List<CodecActiveCallItem> _activeCalls = new List<CodecActiveCallItem>();
        private bool _isInCall;
        private bool _sharingContentIsOn;
        private string _sharingSource = "";

        public MockCallStatusDevice(string key, string name) : base(key, name)
        {
            SharingContentIsOnFeedback = new BoolFeedback(key + "-SharingContentIsOnFeedback", () => _sharingContentIsOn);
            SharingSourceFeedback = new StringFeedback(key + "-SharingSourceFeedback", () => _sharingSource);
            AutoShareContentWhileInCall = false;
        }

        public bool IsInCall 
        { 
            get => _isInCall;
            private set
            {
                if (_isInCall != value)
                {
                    _isInCall = value;
                    OnCallStatusChange();
                }
            }
        }

        public List<CodecActiveCallItem> ActiveCalls => _activeCalls;

        public BoolFeedback SharingContentIsOnFeedback { get; private set; }
        public StringFeedback SharingSourceFeedback { get; private set; }
        public bool AutoShareContentWhileInCall { get; private set; }

        public void Dial(string number)
        {
            // Mock implementation
            var call = new CodecActiveCallItem
            {
                Id = Guid.NewGuid().ToString(),
                Name = $"Call to {number}",
                Number = number,
                Status = eCodecCallStatus.Dialing,
                Direction = eCodecCallDirection.Outgoing
            };
            
            _activeCalls.Add(call);
            IsInCall = true;
        }

        public void EndCall(CodecActiveCallItem activeCall)
        {
            if (activeCall != null && _activeCalls.Contains(activeCall))
            {
                _activeCalls.Remove(activeCall);
                IsInCall = _activeCalls.Count > 0;
            }
        }

        public void EndAllCalls()
        {
            _activeCalls.Clear();
            IsInCall = false;
        }

        public void AcceptCall(CodecActiveCallItem item)
        {
            if (item != null)
            {
                item.Status = eCodecCallStatus.Connected;
                IsInCall = true;
            }
        }

        public void RejectCall(CodecActiveCallItem item)
        {
            if (item != null && _activeCalls.Contains(item))
            {
                _activeCalls.Remove(item);
                IsInCall = _activeCalls.Count > 0;
            }
        }

        public void SendDtmf(string digit)
        {
            // Mock implementation - nothing to do
        }

        public void StartSharing()
        {
            _sharingContentIsOn = true;
            _sharingSource = "Local";
            SharingContentIsOnFeedback.FireUpdate();
            SharingSourceFeedback.FireUpdate();
        }

        public void StopSharing()
        {
            _sharingContentIsOn = false;
            _sharingSource = "";
            SharingContentIsOnFeedback.FireUpdate();
            SharingSourceFeedback.FireUpdate();
        }

        private void OnCallStatusChange()
        {
            CallStatusChange?.Invoke(this, new CodecCallStatusItemChangeEventArgs(
                _activeCalls.Count > 0 ? _activeCalls[0] : null));
        }
    }
}