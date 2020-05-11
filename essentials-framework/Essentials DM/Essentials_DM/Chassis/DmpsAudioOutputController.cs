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
        public DmpsAudioOutput Codec1VolumeLevel { get; private set; }
        public DmpsAudioOutput Codec2VolumeLevel { get; private set; }


        public DmpsAudioOutputController(string key, string name, Card.Dmps3OutputBase card)
            : base(key, name)
        {
            OutputCard = card;

            OutputCard.BaseDevice.DMOutputChange += new DMOutputEventHandler(BaseDevice_DMOutputChange);

            MasterVolumeLevel = new DmpsAudioOutput(card, eDmpsLevelType.Master);
            SourceVolumeLevel = new DmpsAudioOutput(card, eDmpsLevelType.Source);

            if (card is Card.Dmps3ProgramOutput)
            {
                //(card as Card.Dmps3ProgramOutput).OutputMixer.MicLevel
                    //TODO:  Hook up mic levels and mutes
                Codec1VolumeLevel = new DmpsAudioOutput(card, eDmpsLevelType.Codec1);
                Codec2VolumeLevel = new DmpsAudioOutput(card, eDmpsLevelType.Codec2);
            }
            else if (card is Card.Dmps3Aux1Output)
            {
                Codec2VolumeLevel = new DmpsAudioOutput(card, eDmpsLevelType.Codec2);
            }
            else if (card is Card.Dmps3Aux2Output)
            {
                Codec1VolumeLevel = new DmpsAudioOutput(card, eDmpsLevelType.Codec1);
            }
        }

        void BaseDevice_DMOutputChange(Switch device, DMOutputEventArgs args)
        {
            switch (args.EventId)
            {
                case DMOutputEventIds.MasterVolumeFeedBackEventId:
                {
                    MasterVolumeLevel.VolumeLevelFeedback.FireUpdate();
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
                    break;
                }
                case DMOutputEventIds.Codec1LevelFeedBackEventId:
                {
                    if(Codec1VolumeLevel != null)
                        Codec1VolumeLevel.VolumeLevelFeedback.FireUpdate();
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
                        Codec2VolumeLevel.VolumeLevelFeedback.FireUpdate();
                    break;
                }
                case DMOutputEventIds.Codec2MuteOnFeedBackEventId:
                {
                    if (Codec2VolumeLevel != null)
                        Codec2VolumeLevel.MuteFeedback.FireUpdate();
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

            bridge.AddJoinMap(Key, joinMap);

            Debug.Console(1, this, "Linking to Trilist '{0}'", trilist.ID.ToString("X"));

            if (MasterVolumeLevel != null)
            {
                SetUpDmpsAudioOutputJoins(trilist, MasterVolumeLevel, joinMap.MasterVolumeLevel.JoinNumber);
            }

            if (SourceVolumeLevel != null)
            {
                SetUpDmpsAudioOutputJoins(trilist, SourceVolumeLevel, joinMap.SourceVolumeLevel.JoinNumber);
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
            var muteOnJoin = joinStart;
            var muteOffJoin = joinStart + 1;
            var volumeUpJoin = joinStart + 2;
            var volumeDownJoin = joinStart + 3;


            trilist.SetUShortSigAction(volumeLevelJoin, output.SetVolume);
            output.VolumeLevelFeedback.LinkInputSig(trilist.UShortInput[volumeLevelJoin]);

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

        UShortInputSig Level;

        eDmpsLevelType Type;

        public BoolFeedback MuteFeedback { get; private set; }
        public IntFeedback VolumeLevelFeedback { get; private set; }

        Action MuteOnAction;
        Action MuteOffAction;
        Action<bool> VolumeUpAction;
        Action<bool> VolumeDownAction;
 
        public DmpsAudioOutput(Card.Dmps3OutputBase output, eDmpsLevelType type)
        {
            Output = output;

            Type = type;

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