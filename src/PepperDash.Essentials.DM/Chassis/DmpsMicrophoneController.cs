using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges;


namespace PepperDash.Essentials.DM
{
    /// <summary>
    /// Exposes the volume levels for microphones DMPS3 chassis
    /// </summary>
    public class DmpsMicrophoneController
    {
        private Dictionary<uint, DmpsMicrophone> Mics;

        public DmpsMicrophoneController(CrestronControlSystem dmps)
        {
            Debug.Console(2, "Creating Dmps Microphone Controller");
            Mics = new Dictionary<uint,DmpsMicrophone>();

            foreach (var mic in dmps.Microphones)
            {
                Debug.Console(0, "Dmps Microphone Controller Adding Mic: {0} Name: {1}", mic.ID, mic.Name);
                var dmpsMic = new DmpsMicrophone("processor-microphone" + mic.ID, mic.Name, mic);

                DeviceManager.AddDevice(dmpsMic);
                Mics.Add(mic.ID, dmpsMic);
            }

            dmps.MicrophoneChange += new MicrophoneChangeEventHandler(Dmps_MicrophoneChange);
        }

        void Dmps_MicrophoneChange(MicrophoneBase mic, GenericEventArgs args)
        {
            if (args.EventId == MicrophoneEventIds.VuFeedBackEventId)
                return;

            Debug.Console(2, "Dmps Microphone Controller Index: {0} EventId: {1}", mic.ID, args.EventId.ToString());

            if(Mics.ContainsKey(mic.ID))
            {
                Mics[mic.ID].Event(args.EventId);
            }
        }
    }

    public class DmpsMicrophone : EssentialsBridgeableDevice, IBasicVolumeWithFeedback
    {
        MicrophoneBase Mic;

        private bool EnableVolumeSend;
        private ushort VolumeLevelInput;
        protected short MinLevel { get; set; }
        protected short MaxLevel { get; set; }

        public BoolFeedback MuteFeedback { get; private set; }
        public IntFeedback VolumeLevelFeedback { get; private set; }
        public IntFeedback VolumeLevelScaledFeedback { get; private set; }
        public StringFeedback NameFeedback { get; private set; }

        Action MuteOnAction;
        Action MuteOffAction;

        public DmpsMicrophone(string key, string name, MicrophoneBase mic) : base(key, name)
        {
            Mic = mic;
            VolumeLevelInput = 0;
            EnableVolumeSend = false;
            MinLevel = 0;
            MaxLevel = 600;
           
            MuteFeedback = new BoolFeedback(new Func<bool>(() => Mic.MuteOnFeedBack.BoolValue));
            VolumeLevelFeedback = new IntFeedback(new Func<int>(() => Mic.GainFeedBack.UShortValue));
            VolumeLevelScaledFeedback = new IntFeedback(new Func<int>(() => ScaleVolumeFeedback(VolumeLevelFeedback.UShortValue)));
            NameFeedback = new StringFeedback(new Func<string>(() => "Microphone " + Mic.ID));
            MuteOnAction = new Action(Mic.MuteOn);
            MuteOffAction = new Action(Mic.MuteOff);

            VolumeLevelFeedback.FireUpdate();
            VolumeLevelScaledFeedback.FireUpdate();
            NameFeedback.FireUpdate();
            MuteFeedback.FireUpdate();
        }

        public override void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
        {
            var joinMap = new DmpsMicrophoneControllerJoinMap(joinStart);

            if (bridge != null)
            {
                bridge.AddJoinMap(Key, joinMap);
            }
            else
            {
                Debug.Console(0, this, "Please update config to use 'eiscapiadvanced' to get all join map features for this device.");
            }

            Debug.Console(1, this, "Linking to Trilist '{0}'", trilist.ID.ToString("X"));
 
            VolumeLevelFeedback.LinkInputSig(trilist.UShortInput[joinMap.MicGain.JoinNumber]);
            VolumeLevelScaledFeedback.LinkInputSig(trilist.UShortInput[joinMap.MicGainScaled.JoinNumber ]);
            MuteFeedback.LinkInputSig(trilist.BooleanInput[joinMap.MicMuteOn.JoinNumber]);
            MuteFeedback.LinkComplementInputSig(trilist.BooleanInput[joinMap.MicMuteOff.JoinNumber]);
            NameFeedback.LinkInputSig(trilist.StringInput[joinMap.MicName.JoinNumber]);

            trilist.SetUShortSigAction(joinMap.MicGain.JoinNumber, SetVolume);
            trilist.SetUShortSigAction(joinMap.MicGainScaled.JoinNumber, SetVolumeScaled);
            trilist.SetBoolSigAction(joinMap.MicGainScaledSend.JoinNumber, SendScaledVolume);
            trilist.SetSigTrueAction(joinMap.MicMuteOn.JoinNumber, MuteOnAction);
            trilist.SetSigTrueAction(joinMap.MicMuteOff.JoinNumber, MuteOffAction);
        }

        public void Event(int id)
        {
            if (id == MicrophoneEventIds.MuteOnFeedBackEventId)
            {
                MuteFeedback.FireUpdate();
            }
            else if (id == MicrophoneEventIds.GainFeedBackEventId)
            {
                VolumeLevelFeedback.FireUpdate();
                VolumeLevelScaledFeedback.FireUpdate();
            }
        }

        public void SetVolumeScaled(ushort level)
        {
            VolumeLevelInput = (ushort)(level * (MaxLevel - MinLevel) / ushort.MaxValue + MinLevel);
            if (EnableVolumeSend == true)
            {
                Mic.Gain.UShortValue = VolumeLevelInput;
            }
        }

        public ushort ScaleVolumeFeedback(ushort level)
        {
            short signedLevel = (short)level;
            return (ushort)((signedLevel - MinLevel) * ushort.MaxValue / (MaxLevel - MinLevel));
        }

        public void SendScaledVolume(bool pressRelease)
        {
            EnableVolumeSend = pressRelease;
            if (pressRelease == false)
            {
                SetVolumeScaled(VolumeLevelInput);
            }
        }

        #region IBasicVolumeWithFeedback Members

        public void SetVolume(ushort level)
        {
            Mic.Gain.UShortValue = level;
        }

        public void MuteOn()
        {
            MuteOnAction();
        }

        public void MuteOff()
        {
            MuteOffAction();
        }

        #endregion

        #region IBasicVolumeControls Members

        public void VolumeUp(bool pressRelease)
        {
        }

        public void VolumeDown(bool pressRelease)
        {
        }

        public void MuteToggle()
        {
            if (MuteFeedback.BoolValue)
                MuteOff();
            else
                MuteOn();
        }

        #endregion
    }
}