using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using System.Text.RegularExpressions;


namespace PepperDash.Essentials.Devices.Common.DSP
{

    // QUESTIONS:
    // 
    // When subscribing, just use the Instance ID for Custom Name?

    // Verbose on subscriptions?

    // ! "publishToken":"name" "value":-77.0
    // ! "myLevelName" -77

#warning Working here when set aside for config editor work

    public class TesiraForteLevelControl : TesiraForteControlPoint, IDspLevelControl, IKeyed
    {
        bool _IsMuted;
        ushort _VolumeLevel;

        public BoolFeedback MuteFeedback { get; private set; }

        public IntFeedback VolumeLevelFeedback { get; private set; }


        public bool Enabled { get; set; }
        public string ControlPointTag { get { return base.InstanceTag; } }

        /// <summary>
        /// Used for to identify level subscription values
        /// </summary>
        public string LevelCustomName { get; private set; }

        /// <summary>
        /// Used for to identify mute subscription values
        /// </summary>
        public string MuteCustomName { get; private set; }

        /// <summary>
        /// Minimum fader level
        /// </summary>
        double MinLevel;

        /// <summary>
        /// Maximum fader level
        /// </summary>
        double MaxLevel;

        /// <summary>
        /// Checks if a valid subscription string has been recieved for all subscriptions
        /// </summary>
        public bool IsSubsribed
        {
            get
            {
                bool isSubscribed = false;

                if (HasMute && MuteIsSubscribed)
                    isSubscribed = true;

                if (HasLevel && LevelIsSubscribed)
                    isSubscribed = true;

                return isSubscribed;
            }
        }

        public bool AutomaticUnmuteOnVolumeUp { get; private set; }

        public bool HasMute { get; private set; }

        public bool HasLevel { get; private set; }

        bool MuteIsSubscribed;

        bool LevelIsSubscribed;

        //public TesiraForteLevelControl(string label, string id, int index1, int index2, bool hasMute, bool hasLevel, BiampTesiraForteDsp parent)
        //    : base(id, index1, index2, parent)
        //{
        //    Initialize(label, hasMute, hasLevel);
        //}

        public TesiraForteLevelControl(string key, BiampTesiraForteLevelControlBlockConfig config, BiampTesiraForteDsp parent)
            : base(config.InstanceTag, config.Index1, config.Index2, parent)
        {
            Initialize(key, config.Label, config.HasMute, config.HasLevel);
        }


        /// <summary>
        /// Initializes this attribute based on config values and generates subscriptions commands and adds commands to the parent's queue.
        /// </summary>
        public void Initialize(string key, string label, bool hasMute, bool hasLevel)
        {
            Key = string.Format("{0}--{1}", Parent.Key, key);

            DeviceManager.AddDevice(this);

            Debug.Console(2, this, "Adding LevelControl '{0}'", Key);

            this.IsSubscribed = false;

            MuteFeedback = new BoolFeedback(() => _IsMuted);

            VolumeLevelFeedback = new IntFeedback(() => _VolumeLevel);

            HasMute = hasMute;
            HasLevel = hasLevel;
        }

        public void Subscribe()
        {
            // Do subscriptions and blah blah

            // Subscribe to mute
            if (this.HasMute)
            {
                MuteCustomName = string.Format("{0}~mute{1}", this.InstanceTag, this.Index1);

                SendSubscriptionCommand(MuteCustomName, "mute", 500);
            }

            // Subscribe to level
            if (this.HasLevel)
            {
                LevelCustomName = string.Format("{0}~level{1}", this.InstanceTag, this.Index1);

                SendSubscriptionCommand(LevelCustomName, "level", 250);

                SendFullCommand("get", "minLevel", null);

                SendFullCommand("get", "maxLevel", null);
            }
        }


        /// <summary>
        /// Parses the response from the DspBase
        /// </summary>
        /// <param name="customName"></param>
        /// <param name="value"></param>
        public void ParseSubscriptionMessage(string customName, string value)
        {

            // Check for valid subscription response

            if (this.HasMute && customName == MuteCustomName)
            {
                //if (value.IndexOf("+OK") > -1)
                //{
                //    int pointer = value.IndexOf(" +OK");

                //    MuteIsSubscribed = true;

                //    // Removes the +OK
                //    value = value.Substring(0, value.Length - (value.Length - (pointer - 1)));
                //}

                if (value.IndexOf("true") > -1)
                {
                    _IsMuted = true;
                    MuteIsSubscribed = true;

                }
                else if (value.IndexOf("false") > -1)
                {
                    _IsMuted = false;
                    MuteIsSubscribed = true;
                }

                MuteFeedback.FireUpdate();
            }
            else if (this.HasLevel && customName == LevelCustomName)
            {
                //if (value.IndexOf("+OK") > -1)
                //{
                //    int pointer = value.IndexOf(" +OK");

                //    LevelIsSubscribed = true;

                //}

                var _value = Double.Parse(value);

                _VolumeLevel = (ushort)Scale(_value, MinLevel, MaxLevel, 0, 65535);

                LevelIsSubscribed = true;

                VolumeLevelFeedback.FireUpdate();
            }
            
        }

        /// <summary>
        /// Parses a non subscription response
        /// </summary>
        /// <param name="attributeCode">The attribute code of the command</param>
        /// <param name="message">The message to parse</param>
        public override void ParseGetMessage(string attributeCode, string message)
        {
            try
            {
                // Parse an "+OK" message
                string pattern = @"\+OK ""value"":(.*)";

                Match match = Regex.Match(message, pattern);

                if (match.Success)
                {

                    string value = match.Groups[1].Value;

                    Debug.Console(1, this, "Response: '{0}' Value: '{1}'", attributeCode, value);

                    if (message.IndexOf("\"value\":") > -1)
                    {
                        switch (attributeCode)
                        {
                            case "minLevel":
                                {
                                    MinLevel = Double.Parse(value);

                                    Debug.Console(1, this, "MinLevel is '{0}'", MinLevel);

                                    break;
                                }
                            case "maxLevel":
                                {
                                    MaxLevel = Double.Parse(value);

                                    Debug.Console(1, this, "MaxLevel is '{0}'", MaxLevel);

                                    break;
                                }
                            default:
                                {
                                    Debug.Console(2, "Response does not match expected attribute codes: '{0}'", message);

                                    break;
                                }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Console(2, "Unable to parse message: '{0}'\n{1}", message, e);
            }

        }

        /// <summary>
        /// Turns the mute off
        /// </summary>
        public void MuteOff()
        {
            SendFullCommand("set", "mute", "false");
        }

        /// <summary>
        /// Turns the mute on
        /// </summary>
        public void MuteOn()
        {
            SendFullCommand("set", "mute", "true");
        }

        /// <summary>
        /// Sets the volume to a specified level
        /// </summary>
        /// <param name="level"></param>
        public void SetVolume(ushort level)
        {
            Debug.Console(1, this, "volume: {0}", level);
            // Unmute volume if new level is higher than existing
            if (level > _VolumeLevel && AutomaticUnmuteOnVolumeUp)
                if(!_IsMuted)
                    MuteOff();

            double volumeLevel = Scale(level, 0, 65535, MinLevel, MaxLevel);

            SendFullCommand("set", "level", string.Format("{0:0.000000}", volumeLevel));          
        }

        /// <summary>
        /// Toggles mute status
        /// </summary>
        public void MuteToggle()
        {
            SendFullCommand("toggle", "mute", "");
        }

        /// <summary>
        /// Decrements volume level
        /// </summary>
        /// <param name="pressRelease"></param>
        public void VolumeDown(bool pressRelease)
        {
            SendFullCommand("decrement", "level", "");
        }

        /// <summary>
        /// Increments volume level
        /// </summary>
        /// <param name="pressRelease"></param>
        public void VolumeUp(bool pressRelease)
        {
            SendFullCommand("increment", "level", "");

            if (AutomaticUnmuteOnVolumeUp)
                if (!_IsMuted)
                    MuteOff();
        }

        ///// <summary>
        ///// Scales the input from the input range to the output range
        ///// </summary>
        ///// <param name="input"></param>
        ///// <param name="inMin"></param>
        ///// <param name="inMax"></param>
        ///// <param name="outMin"></param>
        ///// <param name="outMax"></param>
        ///// <returns></returns>
        //int Scale(int input, int inMin, int inMax, int outMin, int outMax)
        //{
        //    Debug.Console(1, this, "Scaling (int) input '{0}' with min '{1}'/max '{2}' to output range min '{3}'/max '{4}'", input, inMin, inMax, outMin, outMax);

        //    int inputRange = inMax - inMin;

        //    int outputRange = outMax - outMin;

        //    var output = (((input-inMin) * outputRange) / inputRange ) - outMin;

        //    Debug.Console(1, this, "Scaled output '{0}'", output);

        //    return output;
        //}

        /// <summary>
        /// Scales the input from the input range to the output range
        /// </summary>
        /// <param name="input"></param>
        /// <param name="inMin"></param>
        /// <param name="inMax"></param>
        /// <param name="outMin"></param>
        /// <param name="outMax"></param>
        /// <returns></returns>
        double Scale(double input, double inMin, double inMax, double outMin, double outMax)
        {
            Debug.Console(1, this, "Scaling (double) input '{0}' with min '{1}'/max '{2}' to output range min '{3}'/max '{4}'",input ,inMin ,inMax ,outMin, outMax);

            double inputRange = inMax - inMin;

            if (inputRange <= 0)
            {
                throw new ArithmeticException(string.Format("Invalid Input Range '{0}' for Scaling.  Min '{1}' Max '{2}'.", inputRange, inMin, inMax));
            }

            double outputRange = outMax - outMin;

            var output = (((input - inMin) * outputRange) / inputRange) + outMin;

            Debug.Console(1, this, "Scaled output '{0}'", output);

            return output;
        }
    }
}