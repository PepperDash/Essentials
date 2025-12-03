using System.Collections.Generic;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Devices.Common.Codec;
using Serilog.Events;

namespace PepperDash.Essentials.Devices.Common.AudioCodec
{
    /// <summary>
    /// Represents a MockAC
    /// </summary>
    public class MockAC : AudioCodecBase
    {
        /// <summary>
        /// Constructor for MockAC
        /// </summary>
        /// <param name="key">Device key</param>
        /// <param name="name">Device name</param>
        /// <param name="props">MockAC properties configuration</param>
        public MockAC(string key, string name, MockAcPropertiesConfig props)
            : base(key, name)
        {
            CodecInfo = new MockAudioCodecInfo();

            CodecInfo.PhoneNumber = props.PhoneNumber;
        }

        /// <summary>
        /// Dial method
        /// </summary>
        /// <inheritdoc />
        public override void Dial(string number)
        {
            if (!IsInCall)
            {
                Debug.LogMessage(LogEventLevel.Debug, this, "Dial: {0}", number);
                var call = new CodecActiveCallItem()
                {
                    Name = "Mock Outgoing Call",
                    Number = number,
                    Type = eCodecCallType.Audio,
                    Status = eCodecCallStatus.Connected,
                    Direction = eCodecCallDirection.Outgoing,
                    Id = "mockAudioCall-1"
                };

                ActiveCalls.Add(call);

                OnCallStatusChange(call);
            }
            else
            {
                Debug.LogMessage(LogEventLevel.Debug, this, "Already in call.  Cannot dial new call.");
            }
        }

        /// <summary>
        /// EndCall method
        /// </summary>
        /// <inheritdoc />
        public override void EndCall(CodecActiveCallItem call)
        {
            Debug.LogMessage(LogEventLevel.Debug, this, "EndCall");
            ActiveCalls.Remove(call);
            SetNewCallStatusAndFireCallStatusChange(eCodecCallStatus.Disconnected, call);
        }

        /// <summary>
        /// EndAllCalls method
        /// </summary>
        /// <inheritdoc />
        public override void EndAllCalls()
        {
            Debug.LogMessage(LogEventLevel.Debug, this, "EndAllCalls");
            for (int i = ActiveCalls.Count - 1; i >= 0; i--)
            {
                var call = ActiveCalls[i];
                ActiveCalls.Remove(call);
                SetNewCallStatusAndFireCallStatusChange(eCodecCallStatus.Disconnected, call);
            }
        }

        /// <summary>
        /// AcceptCall method
        /// </summary>
        /// <inheritdoc />
        public override void AcceptCall(CodecActiveCallItem call)
        {
            Debug.LogMessage(LogEventLevel.Debug, this, "AcceptCall");
            SetNewCallStatusAndFireCallStatusChange(eCodecCallStatus.Connecting, call);
        }

        /// <summary>
        /// RejectCall method
        /// </summary>
        public override void RejectCall(CodecActiveCallItem call)
        {
            Debug.LogMessage(LogEventLevel.Debug, this, "RejectCall");
            ActiveCalls.Remove(call);
            SetNewCallStatusAndFireCallStatusChange(eCodecCallStatus.Disconnected, call);
        }

        /// <summary>
        /// SendDtmf method
        /// </summary>
        /// <inheritdoc />
        public override void SendDtmf(string s)
        {
            Debug.LogMessage(LogEventLevel.Debug, this, "BEEP BOOP SendDTMF: {0}", s);
        }

        /// <summary>
        /// TestIncomingAudioCall method
        /// </summary>
        /// <param name="number">Phone number to call from</param>
        public void TestIncomingAudioCall(string number)
        {
            Debug.LogMessage(LogEventLevel.Debug, this, "TestIncomingAudioCall from {0}", number);
            var call = new CodecActiveCallItem() { Name = number, Id = number, Number = number, Type = eCodecCallType.Audio, Direction = eCodecCallDirection.Incoming };
            ActiveCalls.Add(call);
            SetNewCallStatusAndFireCallStatusChange(eCodecCallStatus.Ringing, call);
        }

    }

    /// <summary>
    /// Represents a MockAudioCodecInfo
    /// </summary>
    public class MockAudioCodecInfo : AudioCodecInfo
    {
        string _phoneNumber;

        /// <inheritdoc />
        public override string PhoneNumber
        {
            get
            {
                return _phoneNumber;
            }
            set
            {
                _phoneNumber = value;
            }
        }
    }

    /// <summary>
    /// Represents a MockACFactory
    /// </summary>
    public class MockACFactory : EssentialsDeviceFactory<MockAC>
    {
        /// <summary>
        /// Constructor for MockACFactory
        /// </summary>
        public MockACFactory()
        {
            TypeNames = new List<string>() { "mockac" };
        }

        /// <summary>
        /// BuildDevice method
        /// </summary>
        /// <inheritdoc />
        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.LogMessage(LogEventLevel.Debug, "Factory Attempting to create new MockAc Device");
            var props = Newtonsoft.Json.JsonConvert.DeserializeObject<AudioCodec.MockAcPropertiesConfig>(dc.Properties.ToString());
            return new AudioCodec.MockAC(dc.Key, dc.Name, props);
        }
    }

}