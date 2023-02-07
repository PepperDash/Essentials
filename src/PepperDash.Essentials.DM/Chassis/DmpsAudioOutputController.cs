extern alias Full;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Cards;
using Full.Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges;
using PepperDash.Essentials.DM.Config;


namespace PepperDash.Essentials.DM
{
    /// <summary>
    /// Exposes the volume levels for Program, Aux1, Aux2, Codec1, Codec2, and Digital outputs on a DMPS3 chassis
    /// </summary>
    public class DmpsAudioOutputController : EssentialsBridgeableDevice
    {
        public DmpsAudioOutput MasterVolumeLevel { get; private set; }
        public DmpsAudioOutput SourceVolumeLevel { get; private set; }
        public DmpsAudioOutput MicsMasterVolumeLevel { get; private set; }
        public DmpsAudioOutput Codec1VolumeLevel { get; private set; }
        public DmpsAudioOutput Codec2VolumeLevel { get; private set; }

        public DmpsAudioOutputController(string key, string name, DMOutput card, Card.Dmps3DmHdmiAudioOutput.Dmps3AudioOutputStream stream)
            : base(key, name)
        {
            card.BaseDevice.DMOutputChange += new DMOutputEventHandler(BaseDevice_DMOutputChange);
            var output = new Dmps3AudioOutputWithMixerBase(stream);
            MasterVolumeLevel = new DmpsAudioOutputWithMixer(output, eDmpsLevelType.Master);
            SourceVolumeLevel = new DmpsAudioOutput(output, eDmpsLevelType.Source);
        }
        public DmpsAudioOutputController(string key, string name, DMOutput card, Card.Dmps3DmHdmiAudioOutput.Dmps3DmHdmiOutputStream stream)
            : base(key, name)
        {
            card.BaseDevice.DMOutputChange += new DMOutputEventHandler(BaseDevice_DMOutputChange);
            var output = new Dmps3AudioOutputWithMixerBase(stream);
            MasterVolumeLevel = new DmpsAudioOutputWithMixer(output, eDmpsLevelType.Master);
            SourceVolumeLevel = new DmpsAudioOutput(output, eDmpsLevelType.Source);
        }

        public DmpsAudioOutputController(string key, string name, Card.Dmps3OutputBase card)
            : base(key, name)
        {
            card.BaseDevice.DMOutputChange += new DMOutputEventHandler(BaseDevice_DMOutputChange);

            if (card is Card.Dmps3ProgramOutput)
            {
                var programOutput = card as Card.Dmps3ProgramOutput;
                var output = new Dmps3AudioOutputWithMixerBase(card, programOutput.OutputMixer);
                MasterVolumeLevel = new DmpsAudioOutputWithMixerAndEq(output, eDmpsLevelType.Master, programOutput.OutputEqualizer);
                SourceVolumeLevel = new DmpsAudioOutput(output, eDmpsLevelType.Source);
                MicsMasterVolumeLevel = new DmpsAudioOutput(output, eDmpsLevelType.MicsMaster);
                Codec1VolumeLevel = new DmpsAudioOutput(output, eDmpsLevelType.Codec1);
                Codec2VolumeLevel = new DmpsAudioOutput(output, eDmpsLevelType.Codec2);
            }
            else if (card is Card.Dmps3Aux1Output)
            {
                var auxOutput = card as Card.Dmps3Aux1Output;
                var output = new Dmps3AudioOutputWithMixerBase(card, auxOutput.OutputMixer);
                MasterVolumeLevel = new DmpsAudioOutputWithMixerAndEq(output, eDmpsLevelType.Master, auxOutput.OutputEqualizer);
                SourceVolumeLevel = new DmpsAudioOutput(output, eDmpsLevelType.Source);
                MicsMasterVolumeLevel = new DmpsAudioOutput(output, eDmpsLevelType.MicsMaster);
                Codec2VolumeLevel = new DmpsAudioOutput(output, eDmpsLevelType.Codec2);
            }
            else if (card is Card.Dmps3Aux2Output)
            {
                var auxOutput = card as Card.Dmps3Aux2Output;
                var output = new Dmps3AudioOutputWithMixerBase(card, auxOutput.OutputMixer);
                MasterVolumeLevel = new DmpsAudioOutputWithMixerAndEq(output, eDmpsLevelType.Master, auxOutput.OutputEqualizer);
                SourceVolumeLevel = new DmpsAudioOutput(output, eDmpsLevelType.Source);
                MicsMasterVolumeLevel = new DmpsAudioOutput(output, eDmpsLevelType.MicsMaster);
                Codec1VolumeLevel = new DmpsAudioOutput(output, eDmpsLevelType.Codec1);
            }
            else if (card is Card.Dmps3DigitalMixOutput)
            {
                var mixOutput = card as Card.Dmps3DigitalMixOutput;
                var output = new Dmps3AudioOutputWithMixerBase(card, mixOutput.OutputMixer);
                MasterVolumeLevel = new DmpsAudioOutputWithMixer(output, eDmpsLevelType.Master);
                SourceVolumeLevel = new DmpsAudioOutput(output, eDmpsLevelType.Source);
                MicsMasterVolumeLevel = new DmpsAudioOutput(output, eDmpsLevelType.MicsMaster);
            }
            else if (card is Card.Dmps3HdmiOutput)
            {
                var hdmiOutput = card as Card.Dmps3HdmiOutput;
                var output = new Dmps3AudioOutputWithMixerBase(card, hdmiOutput.OutputMixer);
                MasterVolumeLevel = new DmpsAudioOutputWithMixer(output, eDmpsLevelType.Master);
                SourceVolumeLevel = new DmpsAudioOutput(output, eDmpsLevelType.Source);
                MicsMasterVolumeLevel = new DmpsAudioOutput(output, eDmpsLevelType.MicsMaster);
            }
            else if (card is Card.Dmps3DmOutput)
            {
                var dmOutput = card as Card.Dmps3DmOutput;
                var output = new Dmps3AudioOutputWithMixerBase(card, dmOutput.OutputMixer);
                MasterVolumeLevel = new DmpsAudioOutputWithMixer(output, eDmpsLevelType.Master);
                SourceVolumeLevel = new DmpsAudioOutput(output, eDmpsLevelType.Source);
                MicsMasterVolumeLevel = new DmpsAudioOutput(output, eDmpsLevelType.MicsMaster);
            }
        }

        void BaseDevice_DMOutputChange(Switch device, DMOutputEventArgs args)
        {
            Debug.Console(2, this, "Dmps Audio Controller Event Output: {0} EventId: {1}", args.Number, args.EventId.ToString());
            switch (args.EventId)
            {
                case DMOutputEventIds.OutputVuFeedBackEventId:
                    {
                        //Frequently called event that isn't needed
                        return;
                    }
                case DMOutputEventIds.MasterVolumeFeedBackEventId:
                    {
                        MasterVolumeLevel.VolumeLevelFeedback.FireUpdate();
                        MasterVolumeLevel.VolumeLevelScaledFeedback.FireUpdate();
                        break;
                    }
                case DMOutputEventIds.MasterMuteOnFeedBackEventId:
                    {
                        MasterVolumeLevel.MuteFeedback.FireUpdate();
                        break;
                    }
                case DMOutputEventIds.SourceLevelFeedBackEventId:
                    {
                        SourceVolumeLevel.VolumeLevelFeedback.FireUpdate();
                        SourceVolumeLevel.VolumeLevelScaledFeedback.FireUpdate();
                        break;
                    }
                case DMOutputEventIds.SourceMuteOnFeedBackEventId:
                    {
                        SourceVolumeLevel.MuteFeedback.FireUpdate();
                        break;
                    }
                case DMOutputEventIds.MicMasterLevelFeedBackEventId:
                    {
                        MicsMasterVolumeLevel.VolumeLevelFeedback.FireUpdate();
                        MicsMasterVolumeLevel.VolumeLevelScaledFeedback.FireUpdate();
                        break;
                    }
                case DMOutputEventIds.MicMasterMuteOnFeedBackEventId:
                    {
                        MicsMasterVolumeLevel.MuteFeedback.FireUpdate();
                        break;
                    }
                case DMOutputEventIds.Codec1LevelFeedBackEventId:
                    {
                        if (Codec1VolumeLevel != null)
                        {
                            Codec1VolumeLevel.VolumeLevelFeedback.FireUpdate();
                            Codec1VolumeLevel.VolumeLevelScaledFeedback.FireUpdate();
                        }
                        break;
                    }
                case DMOutputEventIds.Codec1MuteOnFeedBackEventId:
                    {
                        if (Codec1VolumeLevel != null)
                            Codec1VolumeLevel.MuteFeedback.FireUpdate();
                        break;
                    }
                case DMOutputEventIds.Codec2LevelFeedBackEventId:
                    {
                        if (Codec2VolumeLevel != null)
                        {
                            Codec2VolumeLevel.VolumeLevelFeedback.FireUpdate();
                            Codec2VolumeLevel.VolumeLevelScaledFeedback.FireUpdate();
                        }
                        break;
                    }
                case DMOutputEventIds.Codec2MuteOnFeedBackEventId:
                    {
                        if (Codec2VolumeLevel != null)
                            Codec2VolumeLevel.MuteFeedback.FireUpdate();
                        break;
                    }
                case DMOutputEventIds.MinVolumeFeedBackEventId:
                    {
                        Debug.Console(2, this, "MinVolumeFeedBackEventId: {0}", args.Index);
                        var level = MasterVolumeLevel as DmpsAudioOutputWithMixer;
                        if (level != null)
                        {
                            level.GetVolumeMin();
                        }
                        break;
                    }
                case DMOutputEventIds.MaxVolumeFeedBackEventId:
                    {
                        Debug.Console(2, this, "MaxVolumeFeedBackEventId: {0}", args.Index);
                        var level = MasterVolumeLevel as DmpsAudioOutputWithMixer;
                        if (level != null)
                        {
                            level.GetVolumeMax();
                        }
                        break;
                    }
            }
        }

        public override void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
        {
            var joinMap = new DmpsAudioOutputControllerJoinMap(joinStart);

            if (bridge != null)
            {
                bridge.AddJoinMap(Key, joinMap);
            }
            else
            {
                Debug.Console(0, this, "Please update config to use 'eiscapiadvanced' to get all join map features for this device.");
            }

            Debug.Console(1, this, "Linking to Trilist '{0}'", trilist.ID.ToString("X"));

            if (MasterVolumeLevel != null)
            {
                SetUpDmpsAudioOutputJoins(trilist, MasterVolumeLevel, joinMap.MasterVolumeLevel.JoinNumber);
                var mixer = MasterVolumeLevel as DmpsAudioOutputWithMixer;
                if (mixer != null)
                {
                    trilist.SetUShortSigAction(joinMap.MixerPresetRecall.JoinNumber, mixer.RecallPreset);
                }
                var eq = MasterVolumeLevel as DmpsAudioOutputWithMixerAndEq;
                if (eq != null)
                {
                    trilist.SetUShortSigAction(joinMap.MixerEqPresetRecall.JoinNumber, eq.RecallEqPreset);
                }
            }

            if (SourceVolumeLevel != null)
            {
                SetUpDmpsAudioOutputJoins(trilist, SourceVolumeLevel, joinMap.SourceVolumeLevel.JoinNumber);
            }

            if (MicsMasterVolumeLevel != null)
            {
                SetUpDmpsAudioOutputJoins(trilist, MicsMasterVolumeLevel, joinMap.MicsMasterVolumeLevel.JoinNumber);
            }

            if (Codec1VolumeLevel != null)
            {
                SetUpDmpsAudioOutputJoins(trilist, Codec1VolumeLevel, joinMap.Codec1VolumeLevel.JoinNumber);
            }

            if (Codec2VolumeLevel != null)
            {
                SetUpDmpsAudioOutputJoins(trilist, Codec2VolumeLevel, joinMap.Codec2VolumeLevel.JoinNumber);
            }
        }

        static void SetUpDmpsAudioOutputJoins(BasicTriList trilist, DmpsAudioOutput output, uint joinStart)
        {
            var volumeLevelJoin = joinStart;
            var volumeLevelScaledJoin = joinStart + 1;            
            var muteOnJoin = joinStart;
            var muteOffJoin = joinStart + 1;
            var volumeUpJoin = joinStart + 2;
            var volumeDownJoin = joinStart + 3;
            var sendScaledVolumeJoin = joinStart + 4;

            output.VolumeLevelFeedback.LinkInputSig(trilist.UShortInput[volumeLevelJoin]);
            output.VolumeLevelScaledFeedback.LinkInputSig(trilist.UShortInput[volumeLevelScaledJoin]);

            trilist.SetSigTrueAction(muteOnJoin, output.MuteOn);
            output.MuteFeedback.LinkInputSig(trilist.BooleanInput[muteOnJoin]);
            trilist.SetSigTrueAction(muteOffJoin, output.MuteOff);
            output.MuteFeedback.LinkComplementInputSig(trilist.BooleanInput[muteOffJoin]);

            trilist.SetBoolSigAction(volumeUpJoin, output.VolumeUp);
            trilist.SetBoolSigAction(volumeDownJoin, output.VolumeDown);
            trilist.SetBoolSigAction(sendScaledVolumeJoin, output.SendScaledVolume);
            trilist.SetUShortSigAction(volumeLevelJoin, output.SetVolume);
            trilist.SetUShortSigAction(volumeLevelScaledJoin, output.SetVolumeScaled);
        }
    }

    public class DmpsAudioOutputWithMixerAndEq : DmpsAudioOutputWithMixer
    {
        private CrestronControlSystem.Dmps3OutputEqualizer Eq;
        public DmpsAudioOutputWithMixerAndEq(Dmps3AudioOutputWithMixerBase output, eDmpsLevelType type, CrestronControlSystem.Dmps3OutputEqualizer eq)
            : base(output, type)
        {
            Eq = eq;
        }

        public void RecallEqPreset(ushort preset)
        {
            Eq.PresetNumber.UShortValue = preset;
            Eq.RecallPreset();
        }
    }

    public class DmpsAudioOutputWithMixer : DmpsAudioOutput
    {
        Dmps3AudioOutputWithMixerBase Output;

        public DmpsAudioOutputWithMixer(Dmps3AudioOutputWithMixerBase output, eDmpsLevelType type)
            : base(output, type)
        {
            Output = output;
            GetVolumeMax();
            GetVolumeMin();
        }

        public void GetVolumeMin()
        {
            MinLevel = (short)Output.MinVolumeFeedback.UShortValue;
            if (VolumeLevelScaledFeedback != null)
            {
                VolumeLevelScaledFeedback.FireUpdate();
            }
        }

        public void GetVolumeMax()
        {
            MaxLevel = (short)Output.MaxVolumeFeedback.UShortValue;
            if (VolumeLevelScaledFeedback != null)
            {
                VolumeLevelScaledFeedback.FireUpdate();
            }
        }

        public void RecallPreset(ushort preset)
        {
            Output.PresetNumber.UShortValue = preset;
            Output.RecallPreset();

            if (!Global.ControlSystemIsDmps4k3xxType)
            {
                //Recall startup volume for main volume level as DMPS3(non-4K) presets don't affect the main volume
                RecallStartupVolume();
            }
        }

        public void RecallStartupVolume()
        {
            ushort startupVol = Output.StartupVolumeFeedback.UShortValue;
            //Reset startup vol due to bug on DMPS3 where getting the value from above method clears the startup volume
            Output.StartupVolume.UShortValue = startupVol;
            Debug.Console(1, "DMPS Recalling Startup Volume {0}", startupVol);
            SetVolume(startupVol);
            MuteOff();
        }
    }

    public class DmpsAudioOutput : IBasicVolumeWithFeedback
    {
        private UShortInputSig Level;
        private bool EnableVolumeSend;
        private ushort VolumeLevelInput;
        protected short MinLevel { get; set; }
        protected short MaxLevel { get; set; }

        public eDmpsLevelType Type { get; private set; }
        public BoolFeedback MuteFeedback { get; private set; }
        public IntFeedback VolumeLevelFeedback { get; private set; }
        public IntFeedback VolumeLevelScaledFeedback { get; private set; }

        Action MuteOnAction;
        Action MuteOffAction;
        Action<bool> VolumeUpAction;
        Action<bool> VolumeDownAction;

        public DmpsAudioOutput(Dmps3AudioOutputBase output, eDmpsLevelType type)
        {
            VolumeLevelInput = 0;
            EnableVolumeSend = false;
            Type = type;
            MinLevel = -800;
            MaxLevel = 100;

            switch (type)
            {
                case eDmpsLevelType.Master:
                    {
                        Level = output.MasterVolume;
                        MuteFeedback = new BoolFeedback(new Func<bool>(() => output.MasterMuteOnFeedBack.BoolValue));
                        VolumeLevelFeedback = new IntFeedback(new Func<int>(() => output.MasterVolumeFeedBack.UShortValue));
                        MuteOnAction = new Action(output.MasterMuteOn);
                        MuteOffAction = new Action(output.MasterMuteOff);
                        VolumeUpAction = new Action<bool>((b) => output.MasterVolumeUp.BoolValue = b);
                        VolumeDownAction = new Action<bool>((b) => output.MasterVolumeDown.BoolValue = b);
                        break;
                    }
                case eDmpsLevelType.MicsMaster:
                    {
                        if (output.Card is Card.Dmps3OutputBase)
                        {
                            var micOutput = output.Card as Card.Dmps3OutputBase;
                            Level = micOutput.MicMasterLevel;
                            MuteFeedback = new BoolFeedback(new Func<bool>(() => micOutput.MicMasterMuteOnFeedBack.BoolValue));
                            VolumeLevelFeedback = new IntFeedback(new Func<int>(() => micOutput.MicMasterLevelFeedBack.UShortValue));
                            MuteOnAction = new Action(micOutput.MicMasterMuteOn);
                            MuteOffAction = new Action(micOutput.MicMasterMuteOff);
                            VolumeUpAction = new Action<bool>((b) => micOutput.MicMasterLevelUp.BoolValue = b);
                            VolumeDownAction = new Action<bool>((b) => micOutput.MicMasterLevelDown.BoolValue = b);
                        }
                        break;
                    }
                case eDmpsLevelType.Source:
                    {
                        Level = output.SourceLevel;
                        MuteFeedback = new BoolFeedback(new Func<bool>(() => output.SourceMuteOnFeedBack.BoolValue));
                        VolumeLevelFeedback = new IntFeedback(new Func<int>(() => output.SourceLevelFeedBack.UShortValue));
                        MuteOnAction = new Action(output.SourceMuteOn);
                        MuteOffAction = new Action(output.SourceMuteOff);
                        VolumeUpAction = new Action<bool>((b) => output.SourceLevelUp.BoolValue = b);
                        VolumeDownAction = new Action<bool>((b) => output.SourceLevelDown.BoolValue = b);
                        break;
                    }
                case eDmpsLevelType.Codec1:
                    {
                        if (output.Card is Card.Dmps3ProgramOutput)
                        {
                            var programOutput = output.Card as Card.Dmps3ProgramOutput;
                            Level = programOutput.Codec1Level;
                            MuteFeedback = new BoolFeedback(new Func<bool>(() => programOutput.CodecMute1OnFeedback.BoolValue));
                            VolumeLevelFeedback = new IntFeedback(new Func<int>(() => programOutput.Codec1LevelFeedback.UShortValue));
                            MuteOnAction = new Action(programOutput.Codec1MuteOn);
                            MuteOffAction = new Action(programOutput.Codec1MuteOff);
                            VolumeUpAction = new Action<bool>((b) => programOutput.Codec1LevelUp.BoolValue = b);
                            VolumeDownAction = new Action<bool>((b) => programOutput.Codec1LevelDown.BoolValue = b);
                        }
                        else if (output.Card is Card.Dmps3Aux2Output)
                        {
                            var auxOutput = output.Card as Card.Dmps3Aux2Output;
                            Level = auxOutput.Codec1Level;
                            MuteFeedback = new BoolFeedback(new Func<bool>(() => auxOutput.CodecMute1OnFeedback.BoolValue));
                            VolumeLevelFeedback = new IntFeedback(new Func<int>(() => auxOutput.Codec1LevelFeedback.UShortValue));
                            MuteOnAction = new Action(auxOutput.Codec1MuteOn);
                            MuteOffAction = new Action(auxOutput.Codec1MuteOff);
                            VolumeUpAction = new Action<bool>((b) => auxOutput.Codec1LevelUp.BoolValue = b);
                            VolumeDownAction = new Action<bool>((b) => auxOutput.Codec1LevelDown.BoolValue = b);
                        }
                        break;
                    }
                case eDmpsLevelType.Codec2:
                    {
                        if (output.Card is Card.Dmps3ProgramOutput)
                        {
                            var programOutput = output.Card as Card.Dmps3ProgramOutput;
                            Level = programOutput.Codec2Level;
                            MuteFeedback = new BoolFeedback(new Func<bool>(() => programOutput.CodecMute1OnFeedback.BoolValue));
                            VolumeLevelFeedback = new IntFeedback(new Func<int>(() => programOutput.Codec2LevelFeedback.UShortValue));
                            MuteOnAction = new Action(programOutput.Codec2MuteOn);
                            MuteOffAction = new Action(programOutput.Codec2MuteOff);
                            VolumeUpAction = new Action<bool>((b) => programOutput.Codec2LevelUp.BoolValue = b);
                            VolumeDownAction = new Action<bool>((b) => programOutput.Codec2LevelDown.BoolValue = b);
                        }
                        else if (output.Card is Card.Dmps3Aux1Output)
                        {
                            var auxOutput = output.Card as Card.Dmps3Aux1Output;

                            Level = auxOutput.Codec2Level;
                            MuteFeedback = new BoolFeedback(new Func<bool>(() => auxOutput.CodecMute2OnFeedback.BoolValue));
                            VolumeLevelFeedback = new IntFeedback(new Func<int>(() => auxOutput.Codec2LevelFeedback.UShortValue));
                            MuteOnAction = new Action(auxOutput.Codec2MuteOn);
                            MuteOffAction = new Action(auxOutput.Codec2MuteOff);
                            VolumeUpAction = new Action<bool>((b) => auxOutput.Codec2LevelUp.BoolValue = b);
                            VolumeDownAction = new Action<bool>((b) => auxOutput.Codec2LevelDown.BoolValue = b);
                        }
                        break;
                    }
            }
            if (VolumeLevelFeedback != null)
            {
                VolumeLevelScaledFeedback = new IntFeedback(new Func<int>(() => ScaleVolumeFeedback(VolumeLevelFeedback.UShortValue)));
                VolumeLevelFeedback.FireUpdate();
                VolumeLevelScaledFeedback.FireUpdate();
            }
        }

        public void SetVolumeScaled(ushort level)
        {
            if (ushort.MaxValue + MinLevel != 0)
            {
                VolumeLevelInput = (ushort)(level * (MaxLevel - MinLevel) / ushort.MaxValue + MinLevel);
                if (EnableVolumeSend == true)
                {
                    Level.UShortValue = VolumeLevelInput;
                }
            }
        }

        public ushort ScaleVolumeFeedback(ushort level)
        {
            short signedLevel = (short)level;

            if (MaxLevel - MinLevel != 0)
            {
                return (ushort)((signedLevel - MinLevel) * ushort.MaxValue / (MaxLevel - MinLevel));
            }
            else
                return (ushort)MinLevel;
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
            Level.UShortValue = level;
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
            VolumeUpAction(pressRelease);
        }

        public void VolumeDown(bool pressRelease)
        {
            VolumeDownAction(pressRelease);
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

    public class Dmps3AudioOutputWithMixerBase : Dmps3AudioOutputBase
    {
        public UShortOutputSig MinVolumeFeedback { get; private set; }
        public UShortOutputSig MaxVolumeFeedback { get; private set; }
        public UShortInputSig StartupVolume { get; private set; }
        public UShortOutputSig StartupVolumeFeedback { get; private set; }
        public UShortInputSig PresetNumber { get; private set; }

        public Action RecallPreset { get; private set; }

        public Dmps3AudioOutputWithMixerBase(Card.Dmps3OutputBase card, CrestronControlSystem.Dmps3OutputMixer mixer)
            : base(card)
        {
            MinVolumeFeedback = mixer.MinVolumeFeedback;
            MaxVolumeFeedback = mixer.MaxVolumeFeedback;
            StartupVolume = mixer.StartupVolume;
            StartupVolumeFeedback = mixer.StartupVolumeFeedback;
            PresetNumber = mixer.PresetNumber;

            RecallPreset = new Action(mixer.RecallPreset);
        }

        public Dmps3AudioOutputWithMixerBase(Card.Dmps3OutputBase card, CrestronControlSystem.Dmps3AttachableOutputMixer mixer)
            : base(card)
        {
            MinVolumeFeedback = mixer.MinVolumeFeedback;
            MaxVolumeFeedback = mixer.MaxVolumeFeedback;
            StartupVolume = mixer.StartupVolume;
            StartupVolumeFeedback = mixer.StartupVolumeFeedback;
            PresetNumber = mixer.PresetNumber;

            RecallPreset = new Action(mixer.RecallPreset);
        }

        public Dmps3AudioOutputWithMixerBase(Card.Dmps3DmHdmiAudioOutput.Dmps3AudioOutputStream stream)
            : base(stream)
        {
            var mixer = stream.OutputMixer;
            MinVolumeFeedback = mixer.MinVolumeFeedback;
            MaxVolumeFeedback = mixer.MaxVolumeFeedback;
            StartupVolume = mixer.StartupVolume;
            StartupVolumeFeedback = mixer.StartupVolumeFeedback;
            PresetNumber = stream.PresetNumber;
            RecallPreset = new Action(stream.RecallPreset);
        }

        public Dmps3AudioOutputWithMixerBase(Card.Dmps3DmHdmiAudioOutput.Dmps3DmHdmiOutputStream stream)
            : base(stream)
        {
            var mixer = stream.OutputMixer;
            MinVolumeFeedback = mixer.MinVolumeFeedback;
            MaxVolumeFeedback = mixer.MaxVolumeFeedback;
            StartupVolume = mixer.StartupVolume;
            StartupVolumeFeedback = mixer.StartupVolumeFeedback;
            PresetNumber = stream.PresetNumber;
            RecallPreset = new Action(stream.RecallPreset);
        }
    }
    public class Dmps3AudioOutputBase
    {
        public DMOutput Card { get; private set; }
        public BoolOutputSig MasterMuteOffFeedBack { get; private set; }
        public BoolOutputSig MasterMuteOnFeedBack { get; private set; }
        public UShortInputSig MasterVolume { get; private set; }
        public UShortOutputSig MasterVolumeFeedBack { get; private set; }
        public BoolInputSig MasterVolumeUp { get; private set; }
        public BoolInputSig MasterVolumeDown { get; private set; }
        public BoolOutputSig SourceMuteOffFeedBack { get; private set; }
        public BoolOutputSig SourceMuteOnFeedBack { get; private set; }
        public UShortInputSig SourceLevel { get; private set; }
        public UShortOutputSig SourceLevelFeedBack { get; private set; }
        public BoolInputSig SourceLevelUp { get; private set; }
        public BoolInputSig SourceLevelDown { get; private set; }

        public Action MasterMuteOff { get; private set; }
        public Action MasterMuteOn { get; private set; }
        public Action SourceMuteOff { get; private set; }
        public Action SourceMuteOn { get; private set; }

        public Dmps3AudioOutputBase(Card.Dmps3OutputBase card)
        {
            Card = card;
            MasterMuteOffFeedBack = card.MasterMuteOffFeedBack;
            MasterMuteOnFeedBack = card.MasterMuteOnFeedBack;
            MasterVolume = card.MasterVolume;
            MasterVolumeFeedBack = card.MasterVolumeFeedBack;
            MasterVolumeUp = card.MasterVolumeUp;
            MasterVolumeDown = card.MasterVolumeDown;
            SourceMuteOffFeedBack = card.SourceMuteOffFeedBack;
            SourceMuteOnFeedBack = card.SourceMuteOnFeedBack;
            SourceLevel = card.SourceLevel;
            SourceLevelFeedBack = card.SourceLevelFeedBack;
            SourceLevelUp = card.SourceLevelUp;
            SourceLevelDown = card.SourceLevelDown;

            MasterMuteOff = new Action(card.MasterMuteOff);
            MasterMuteOn = new Action(card.MasterMuteOn);
            SourceMuteOff = new Action(card.SourceMuteOff);
            SourceMuteOn = new Action(card.SourceMuteOn);
        }

        public Dmps3AudioOutputBase(Card.Dmps3DmHdmiAudioOutput.Dmps3AudioOutputStream stream)
        {
            MasterMuteOffFeedBack = stream.MasterMuteOffFeedBack;
            MasterMuteOnFeedBack = stream.MasterMuteOnFeedBack;
            MasterVolume = stream.MasterVolume;
            MasterVolumeFeedBack = stream.MasterVolumeFeedBack;
            MasterVolumeUp = stream.MasterVolumeUp;
            MasterVolumeDown = stream.MasterVolumeDown;
            SourceMuteOffFeedBack = stream.SourceMuteOffFeedBack;
            SourceMuteOnFeedBack = stream.SourceMuteOnFeedBack;
            SourceLevel = stream.SourceLevel;
            SourceLevelFeedBack = stream.SourceLevelFeedBack;
            SourceLevelUp = stream.SourceLevelUp;
            SourceLevelDown = stream.SourceLevelDown;

            MasterMuteOff = new Action(stream.MasterMuteOff);
            MasterMuteOn = new Action(stream.MasterMuteOn);
            SourceMuteOff = new Action(stream.SourceMuteOff);
            SourceMuteOn = new Action(stream.SourceMuteOn);
        }

        public Dmps3AudioOutputBase(Card.Dmps3DmHdmiAudioOutput.Dmps3DmHdmiOutputStream stream)
        {
            MasterMuteOffFeedBack = stream.MasterMuteOffFeedBack;
            MasterMuteOnFeedBack = stream.MasterMuteOnFeedBack;
            MasterVolume = stream.MasterVolume;
            MasterVolumeFeedBack = stream.MasterVolumeFeedBack;
            MasterVolumeUp = stream.MasterVolumeUp;
            MasterVolumeDown = stream.MasterVolumeDown;
            SourceMuteOffFeedBack = stream.SourceMuteOffFeedBack;
            SourceMuteOnFeedBack = stream.SourceMuteOnFeedBack;
            SourceLevel = stream.SourceLevel;
            SourceLevelFeedBack = stream.SourceLevelFeedBack;
            SourceLevelUp = stream.SourceLevelUp;
            SourceLevelDown = stream.SourceLevelDown;

            MasterMuteOff = new Action(stream.MasterMuteOff);
            MasterMuteOn = new Action(stream.MasterMuteOn);
            SourceMuteOff = new Action(stream.SourceMuteOff);
            SourceMuteOn = new Action(stream.SourceMuteOn);
        }
    }

    public enum eDmpsLevelType
    {
        Master,
        Source,
        MicsMaster,
        Codec1,
        Codec2,
        Mic
    }
}