using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Cards;

using PepperDash.Core;
using PepperDash.Essentials.Core;


namespace PepperDash.Essentials.DM
{
    /// <summary>
    /// 
    /// </summary>
    public class DmpsDigitalOutputController : Device, IRoutingNumeric, IHasFeedback
    {
        public Card.Dmps3OutputBase OutputCard { get; protected set; }

        public RoutingInputPort None { get; protected set; }
        public RoutingInputPort DigitalMix1 { get; protected set; }
        public RoutingInputPort DigitalMix2 { get; protected set; }
        public RoutingInputPort AudioFollowsVideo { get; protected set; }

        public RoutingOutputPort DigitalAudioOut { get; protected set; }

        public IntFeedback AudioSourceNumericFeedback { get; protected set; }

        /// <summary>
        /// Returns a list containing the Outputs that we want to expose.
        /// </summary>
        public FeedbackCollection<Feedback> Feedbacks { get; private set; }

        public virtual RoutingPortCollection<RoutingInputPort> InputPorts
        {
            get
            {
                return new RoutingPortCollection<RoutingInputPort> 
				{ 
                    None,
					DigitalMix1,
                    DigitalMix2,
					AudioFollowsVideo
				};
            }
        }

        public RoutingPortCollection<RoutingOutputPort> OutputPorts
        {
            get
            {
                return new RoutingPortCollection<RoutingOutputPort> { DigitalAudioOut };
            }
        }

        public DmpsDigitalOutputController(string key, string name, Card.Dmps3OutputBase outputCard)
            : base(key, name)
        {
            Feedbacks = new FeedbackCollection<Feedback>();
            OutputCard = outputCard;

            if (outputCard is Card.Dmps3DmOutputBackend)
            {
                AudioSourceNumericFeedback = new IntFeedback(() =>
                {
                    return (int)(outputCard as Card.Dmps3DmOutputBackend).AudioOutSourceDeviceFeedback;
                });
                DigitalAudioOut = new RoutingOutputPort(DmPortName.DmOut + OutputCard.Number, eRoutingSignalType.Audio, eRoutingPortConnectionType.DmCat, null, this);
            }

            else if (outputCard is Card.Dmps3HdmiOutputBackend)
            {
                AudioSourceNumericFeedback = new IntFeedback(() =>
                {
                    return (int)(outputCard as Card.Dmps3HdmiOutputBackend).AudioOutSourceDeviceFeedback;
                });
                DigitalAudioOut = new RoutingOutputPort(DmPortName.HdmiOut + OutputCard.Number, eRoutingSignalType.Audio, eRoutingPortConnectionType.Hdmi, null, this);
            }
            else
            {
                return;
            }

            None = new RoutingInputPort("None", eRoutingSignalType.Audio, eRoutingPortConnectionType.DigitalAudio,
                eDmps34KAudioOutSourceDevice.NoRoute, this);
            DigitalMix1 = new RoutingInputPort("DigitalMix1", eRoutingSignalType.Audio, eRoutingPortConnectionType.DigitalAudio,
                eDmps34KAudioOutSourceDevice.DigitalMixer1, this);
            DigitalMix2 = new RoutingInputPort("DigitalMix2", eRoutingSignalType.Audio, eRoutingPortConnectionType.DigitalAudio,
                eDmps34KAudioOutSourceDevice.DigitalMixer2, this);
            AudioFollowsVideo = new RoutingInputPort("AudioFollowsVideo", eRoutingSignalType.Audio, eRoutingPortConnectionType.DigitalAudio,
                eDmps34KAudioOutSourceDevice.AudioFollowsVideo, this);
            
            AddToFeedbackList(AudioSourceNumericFeedback);
        }

        /// <summary>
        /// Adds feedback(s) to the list
        /// </summary>
        /// <param name="newFbs"></param>
        public void AddToFeedbackList(params Feedback[] newFbs)
        {
            foreach (var f in newFbs)
            {
                if (f != null)
                {
                    if (!Feedbacks.Contains(f))
                    {
                        Feedbacks.Add(f);
                    }
                }
            }
        }

        public virtual void ExecuteNumericSwitch(ushort input, ushort output, eRoutingSignalType type)
        {
            Debug.Console(2, this, "Executing Numeric Switch to input {0}.", input);

            switch (input)
            {
                case 0:
                    {
                        ExecuteSwitch(None.Selector, null, type);
                        break;
                    }
                case 1:
                    {
                        ExecuteSwitch(DigitalMix1.Selector, null, type);
                        break;
                    }
                case 2:
                    {
                        ExecuteSwitch(DigitalMix2.Selector, null, type);
                        break;
                    }
                case 3:
                    {
                        ExecuteSwitch(AudioFollowsVideo.Selector, null, type);
                        break;
                    }
            }

        }

        #region IRouting Members

        public void ExecuteSwitch(object inputSelector, object outputSelector, eRoutingSignalType signalType)
        {
            if ((signalType | eRoutingSignalType.Audio) == eRoutingSignalType.Audio)
            {
                if (OutputCard is Card.Dmps3DmOutputBackend)
                {
                    (OutputCard as Card.Dmps3DmOutputBackend).AudioOutSourceDevice = (eDmps34KAudioOutSourceDevice)inputSelector;
                }
                else if (OutputCard is Card.Dmps3HdmiOutputBackend)
                {
                    (OutputCard as Card.Dmps3HdmiOutputBackend).AudioOutSourceDevice = (eDmps34KAudioOutSourceDevice)inputSelector;
                }
            }
        }

        #endregion
    }
}