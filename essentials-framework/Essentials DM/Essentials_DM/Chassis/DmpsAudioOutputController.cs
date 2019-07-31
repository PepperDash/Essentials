using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;

using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Cards;

using PepperDash.Core;
using PepperDash.Essentials.Core;

using PepperDash.Essentials.DM.Config;

namespace PepperDash.Essentials.DM
{
    public class DmpsAudioOutputController : Device, IHasFeedback
    {
        Card.Dmps3OutputBase Card;

        public DmpsAudioOutput MasterVolumeLevel { get; private set; }
        public DmpsAudioOutput SourceVolumeLevel { get; private set; }
        public DmpsAudioOutput Codec1VolumeLevel { get; private set; }
        public DmpsAudioOutput Codec2VolumeLevel { get; private set; }
        public Dictionary<int, DmpsAudioOutput> MicVolumeLevels { get; private set; }

        public DmpsAudioOutputController(string key, string name, Card.Dmps3OutputBase card)
            : base(key, name)
        {
            Card = card;

            MasterVolumeLevel = new DmpsAudioOutput(card, eDmpsLevelType.Master);
            SourceVolumeLevel = new DmpsAudioOutput(card, eDmpsLevelType.Source);

            if (card is Card.Dmps3ProgramOutput)
            {
                Codec1VolumeLevel = new DmpsAudioOutput(card, eDmpsLevelType.Codec1);
            }
        }

        #region IHasFeedback Members

        public FeedbackCollection<Feedback> Feedbacks
        {
            get { throw new NotImplementedException(); }
        }

        #endregion
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
        Codec2
    }
}