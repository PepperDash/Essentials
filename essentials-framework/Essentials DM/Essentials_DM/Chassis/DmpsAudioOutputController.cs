using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Cards;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges;
using PepperDash.Essentials.DM.Config;


namespace PepperDash.Essentials.DM
{
    /// <summary>
    /// Exposes the volume levels for Program, Aux1 or Aux2 outputs on a DMPS3 chassis
    /// </summary>
    public class DmpsAudioOutputController : EssentialsBridgeableDevice
    {
        Card.Dmps3OutputBase OutputCard;

        public DmpsAudioOutput MasterVolumeLevel { get; private set; }
        public DmpsAudioOutput SourceVolumeLevel { get; private set; }
        public DmpsAudioOutput MicsMasterVolumeLevel { get; private set; }
        public DmpsAudioOutput Codec1VolumeLevel { get; private set; }
        public DmpsAudioOutput Codec2VolumeLevel { get; private set; }

        public DmpsAudioOutputController(string key, string name, Card.Dmps3OutputBase card)
            : base(key, name)
        {
            OutputCard = card;

            OutputCard.BaseDevice.DMOutputChange += new DMOutputEventHandler(BaseDevice_DMOutputChange);

            if (card is Card.Dmps3ProgramOutput)
            {
                MasterVolumeLevel = new DmpsAudioOutput(card, eDmpsLevelType.Master)
                {
                    Mixer = (card as Card.Dmps3ProgramOutput).OutputMixer
                };
                SourceVolumeLevel = new DmpsAudioOutput(card, eDmpsLevelType.Source);
                MicsMasterVolumeLevel = new DmpsAudioOutput(card, eDmpsLevelType.MicsMaster);
                Codec1VolumeLevel = new DmpsAudioOutput(card, eDmpsLevelType.Codec1);
                Codec2VolumeLevel = new DmpsAudioOutput(card, eDmpsLevelType.Codec2);
            }
            else if (card is Card.Dmps3Aux1Output)
            {
                MasterVolumeLevel = new DmpsAudioOutput(card, eDmpsLevelType.Master)
                {
                    Mixer = (card as Card.Dmps3Aux1Output).OutputMixer
                };
                SourceVolumeLevel = new DmpsAudioOutput(card, eDmpsLevelType.Source);
                MicsMasterVolumeLevel = new DmpsAudioOutput(card, eDmpsLevelType.MicsMaster);
                Codec2VolumeLevel = new DmpsAudioOutput(card, eDmpsLevelType.Codec2);
            }
            else if (card is Card.Dmps3Aux2Output)
            {
                MasterVolumeLevel = new DmpsAudioOutput(card, eDmpsLevelType.Master)
                {
                    Mixer = (card as Card.Dmps3Aux2Output).OutputMixer
                };
                SourceVolumeLevel = new DmpsAudioOutput(card, eDmpsLevelType.Source);
                MicsMasterVolumeLevel = new DmpsAudioOutput(card, eDmpsLevelType.MicsMaster);
                Codec1VolumeLevel = new DmpsAudioOutput(card, eDmpsLevelType.Codec1);
            }
            else //Digital Outputs
            {
                MasterVolumeLevel = new DmpsAudioOutput(card, eDmpsLevelType.Master);
                SourceVolumeLevel = new DmpsAudioOutput(card, eDmpsLevelType.Source);
                MicsMasterVolumeLevel = new DmpsAudioOutput(card, eDmpsLevelType.MicsMaster);
            }
        }

        void BaseDevice_DMOutputChange(Switch device, DMOutputEventArgs args)
        {
            switch (args.EventId)
            {
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
                    if (MasterVolumeLevel != null)
                    {
                        MasterVolumeLevel.GetVolumeMin();
                    }
                    break;
                }
                case DMOutputEventIds.MaxVolumeFeedBackEventId:
                {
                    Debug.Console(2, this, "MaxVolumeFeedBackEventId: {0}", args.Index);
                    if (MasterVolumeLevel != null)
                    {
                        MasterVolumeLevel.GetVolumeMax();
                    }
                    break;
                }
            }
        }

        public override void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
        {
            var joinMap = new DmpsAudioOutputControllerJoinMap(joinStart);

            var joinMapSerialized = JoinMapHelper.GetSerializedJoinMapForDevice(joinMapKey);

            if (!string.IsNullOrEmpty(joinMapSerialized))
                joinMap = JsonConvert.DeserializeObject<DmpsAudioOutputControllerJoinMap>(joinMapSerialized);

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
            
            trilist.SetUShortSigAction(volumeLevelJoin, output.SetVolume);
            output.VolumeLevelFeedback.LinkInputSig(trilist.UShortInput[volumeLevelJoin]);

            trilist.SetUShortSigAction(volumeLevelScaledJoin, output.SetVolumeScaled);
            output.VolumeLevelScaledFeedback.LinkInputSig(trilist.UShortInput[volumeLevelScaledJoin]);

            trilist.SetSigTrueAction(muteOnJoin, output.MuteOn);
            output.MuteFeedback.LinkInputSig(trilist.BooleanInput[muteOnJoin]);
            trilist.SetSigTrueAction(muteOffJoin, output.MuteOff);
            output.MuteFeedback.LinkComplementInputSig(trilist.BooleanInput[muteOffJoin]);

            trilist.SetBoolSigAction(volumeUpJoin, output.VolumeUp);
            trilist.SetBoolSigAction(volumeDownJoin, output.VolumeDown);
        }
    }

    public class DmpsAudioOutput : IBasicVolumeWithFeedback
    {
        Card.Dmps3OutputBase Output;
        eDmpsLevelType Type;
        UShortInputSig Level;

        public short MinLevel { get; private set; }
        public short MaxLevel { get; private set; }

        public CrestronControlSystem.Dmps3OutputMixerWithMonoAndStereo Mixer { get; set; }
        public BoolFeedback MuteFeedback { get; private set; }
        public IntFeedback VolumeLevelFeedback { get; private set; }
        public IntFeedback VolumeLevelScaledFeedback { get; private set; }

        Action MuteOnAction;
        Action MuteOffAction;
        Action<bool> VolumeUpAction;
        Action<bool> VolumeDownAction;
 
        public DmpsAudioOutput(Card.Dmps3OutputBase output, eDmpsLevelType type)
        {
            Output = output;
            Type = type;
            MinLevel = -800;
            MaxLevel = 100;

            switch (type)
            {
                case eDmpsLevelType.Master:
                    {
                        Level = output.MasterVolume;

                        MuteFeedback = new BoolFeedback( new Func<bool> (() => Output.MasterMuteOnFeedBack.BoolValue));
                        VolumeLevelFeedback = new IntFeedback(new Func<int>(() => Output.MasterVolumeFeedBack.UShortValue));
                        MuteOnAction = new Action(Output.MasterMuteOn);
                        MuteOffAction = new Action(Output.MasterMuteOff);
                        VolumeUpAction = new Action<bool>((b) => Output.MasterVolumeUp.BoolValue = b);
                        VolumeDownAction = new Action<bool>((b) => Output.MasterVolumeDown.BoolValue = b);

                        break;
                    }
                case eDmpsLevelType.MicsMaster:
                    {
                        Level = output.MicMasterLevel;

                        MuteFeedback = new BoolFeedback(new Func<bool>(() => Output.MicMasterMuteOnFeedBack.BoolValue));
                        VolumeLevelFeedback = new IntFeedback(new Func<int>(() => Output.MicMasterLevelFeedBack.UShortValue));
                        MuteOnAction = new Action(Output.MicMasterMuteOn);
                        MuteOffAction = new Action(Output.MicMasterMuteOff);
                        VolumeUpAction = new Action<bool>((b) => Output.MicMasterLevelUp.BoolValue = b);
                        VolumeDownAction = new Action<bool>((b) => Output.MicMasterLevelDown.BoolValue = b);

                        break;
                    }
                case eDmpsLevelType.Source:
                    {
                        Level = output.SourceLevel;

                        MuteFeedback = new BoolFeedback(new Func<bool>(() => Output.SourceMuteOnFeedBack.BoolValue));
                        VolumeLevelFeedback = new IntFeedback(new Func<int>(() => Output.SourceLevelFeedBack.UShortValue));
                        MuteOnAction = new Action(Output.SourceMuteOn);
                        MuteOffAction = new Action(Output.SourceMuteOff);
                        VolumeUpAction = new Action<bool>((b) => Output.SourceLevelUp.BoolValue = b);
                        VolumeDownAction = new Action<bool>((b) => Output.SourceLevelDown.BoolValue = b);
                        break;
                    }
                case eDmpsLevelType.Codec1:
                    {
                        var programOutput = output as Card.Dmps3ProgramOutput;

                        if (programOutput != null)
                        {
                            Level = programOutput.Codec1Level;

                            MuteFeedback = new BoolFeedback(new Func<bool>(() => programOutput.CodecMute1OnFeedback.BoolValue));
                            VolumeLevelFeedback = new IntFeedback(new Func<int>(() => programOutput.Codec1LevelFeedback.UShortValue));
                            MuteOnAction = new Action(programOutput.Codec1MuteOn);
                            MuteOffAction = new Action(programOutput.Codec1MuteOff);
                            VolumeUpAction = new Action<bool>((b) => programOutput.Codec1LevelUp.BoolValue = b);
                            VolumeDownAction = new Action<bool>((b) => programOutput.Codec1LevelDown.BoolValue = b);

                        }
                        else
                        {
                            var auxOutput = output as Card.Dmps3Aux2Output;

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
                        var programOutput = output as Card.Dmps3ProgramOutput;

                        if (programOutput != null)
                        {
                            Level = programOutput.Codec2Level;

                            MuteFeedback = new BoolFeedback(new Func<bool>(() => programOutput.CodecMute1OnFeedback.BoolValue));
                            VolumeLevelFeedback = new IntFeedback(new Func<int>(() => programOutput.Codec2LevelFeedback.UShortValue));
                            MuteOnAction = new Action(programOutput.Codec2MuteOn);
                            MuteOffAction = new Action(programOutput.Codec2MuteOff);
                            VolumeUpAction = new Action<bool>((b) => programOutput.Codec2LevelUp.BoolValue = b);
                            VolumeDownAction = new Action<bool>((b) => programOutput.Codec2LevelDown.BoolValue = b);

                        }
                        else
                        {
                            var auxOutput = output as Card.Dmps3Aux1Output;

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
            }
        }

        public void SetVolumeScaled(ushort level)
        {
            Debug.Console(2, Debug.ErrorLogLevel.None, "Scaling DMPS volume:{0} level:{1} min:{2} max:{3}", Output.Name, level.ToString(), MinLevel.ToString(), MaxLevel.ToString());
            Level.UShortValue = (ushort)(level * (MaxLevel - MinLevel) / ushort.MaxValue + MinLevel);
        }

        public ushort ScaleVolumeFeedback(ushort level)
        {
            short signedLevel = (short)level;
            Debug.Console(2, Debug.ErrorLogLevel.None, "Scaling DMPS volume:{0} feedback:{1} min:{2} max:{3}", Output.Name, signedLevel.ToString(), MinLevel.ToString(), MaxLevel.ToString());
            return (ushort)((signedLevel - MinLevel) * ushort.MaxValue / (MaxLevel - MinLevel));
        }

        public void GetVolumeMin()
        {
            if (Mixer != null)
            {
                MinLevel = (short)Mixer.MinVolumeFeedback.UShortValue;
                Debug.Console(2, Debug.ErrorLogLevel.None, "DMPS set {0} min level:{1}", Output.Name, MinLevel);
                if (VolumeLevelScaledFeedback != null)
                {
                    VolumeLevelScaledFeedback.FireUpdate();
                }
            }
        }

        public void GetVolumeMax()
        {
            if (Mixer != null)
            {
                MaxLevel = (short)Mixer.MaxVolumeFeedback.UShortValue;
                Debug.Console(2, Debug.ErrorLogLevel.None, "DMPS set {0} max level:{1}", Output.Name, MaxLevel);
                if (VolumeLevelScaledFeedback != null)
                {
                    VolumeLevelScaledFeedback.FireUpdate();
                }
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