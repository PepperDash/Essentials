using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Devices.Common.Codec;

namespace PepperDash.Essentials.Devices.Common.AudioCodec
{
    public class MockAC : AudioCodecBase
    {
        public MockAC(string key, string name, MockAcPropertiesConfig props)
            : base(key, name)
        {
            CodecInfo = new MockAudioCodecInfo();

            CodecInfo.PhoneNumber = props.PhoneNumber;
        }

        public override void Dial(string number)
        {
            if (!IsInCall)
            {
                Debug.Console(1, this, "Dial: {0}", number);
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
                Debug.Console(1, this, "Already in call.  Cannot dial new call.");
            }
        }

        public override void EndCall(CodecActiveCallItem call)
        {
            Debug.Console(1, this, "EndCall");
            ActiveCalls.Remove(call);
            SetNewCallStatusAndFireCallStatusChange(eCodecCallStatus.Disconnected, call);
        }

        public override void EndAllCalls()
        {
            Debug.Console(1, this, "EndAllCalls");
            for (int i = ActiveCalls.Count - 1; i >= 0; i--)
            {
                var call = ActiveCalls[i];
                ActiveCalls.Remove(call);
                SetNewCallStatusAndFireCallStatusChange(eCodecCallStatus.Disconnected, call);
            }
        }

        public override void AcceptCall(CodecActiveCallItem call)
        {
            Debug.Console(1, this, "AcceptCall");
            SetNewCallStatusAndFireCallStatusChange(eCodecCallStatus.Connecting, call);
        }

        public override void RejectCall(CodecActiveCallItem call)
        {
            Debug.Console(1, this, "RejectCall");
            ActiveCalls.Remove(call);
            SetNewCallStatusAndFireCallStatusChange(eCodecCallStatus.Disconnected, call);
        }

        public override void SendDtmf(string s)
        {
            Debug.Console(1, this, "BEEP BOOP SendDTMF: {0}", s);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        public void TestIncomingAudioCall(string number)
        {
            Debug.Console(1, this, "TestIncomingAudioCall from {0}", number);
            var call = new CodecActiveCallItem() { Name = number, Id = number, Number = number, Type = eCodecCallType.Audio, Direction = eCodecCallDirection.Incoming };
            ActiveCalls.Add(call);
            SetNewCallStatusAndFireCallStatusChange(eCodecCallStatus.Ringing, call);
        }

    }

    public class MockAudioCodecInfo : AudioCodecInfo
    {
        string _phoneNumber;

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

    public class MockACFactory : EssentialsDeviceFactory<MockAC>
    {
        public MockACFactory()
        {
            TypeNames = new List<string>() { "mockac" };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "Factory Attempting to create new MockAc Device");
            var props = Newtonsoft.Json.JsonConvert.DeserializeObject<AudioCodec.MockAcPropertiesConfig>(dc.Properties.ToString());
            return new AudioCodec.MockAC(dc.Key, dc.Name, props);
        }
    }

}