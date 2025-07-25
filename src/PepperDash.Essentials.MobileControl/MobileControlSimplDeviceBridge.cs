using Crestron.SimplSharpPro.EthernetCommunication;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using System;

namespace PepperDash.Essentials.Room.MobileControl
{
    /// <summary>
    /// Represents a MobileControlSimplDeviceBridge
    /// </summary>
    public class MobileControlSimplDeviceBridge : Device, IChannel, INumericKeypad
    {
        /// <summary>
        /// EISC used to talk to Simpl
        /// </summary>
        private readonly ThreeSeriesTcpIpEthernetIntersystemCommunications _eisc;

        public MobileControlSimplDeviceBridge(string key, string name,
            ThreeSeriesTcpIpEthernetIntersystemCommunications eisc)
            : base(key, name)
        {
            _eisc = eisc;
        }

        #region IChannel Members

        /// <summary>
        /// ChannelUp method
        /// </summary>
        public void ChannelUp(bool pressRelease)
        {
            _eisc.SetBool(1111, pressRelease);
        }

        /// <summary>
        /// ChannelDown method
        /// </summary>
        public void ChannelDown(bool pressRelease)
        {
            _eisc.SetBool(1111, pressRelease);
        }

        /// <summary>
        /// LastChannel method
        /// </summary>
        public void LastChannel(bool pressRelease)
        {
            _eisc.SetBool(1111, pressRelease);
        }

        /// <summary>
        /// Guide method
        /// </summary>
        public void Guide(bool pressRelease)
        {
            _eisc.SetBool(1111, pressRelease);
        }

        /// <summary>
        /// Info method
        /// </summary>
        public void Info(bool pressRelease)
        {
            _eisc.SetBool(1111, pressRelease);
        }

        /// <summary>
        /// Exit method
        /// </summary>
        public void Exit(bool pressRelease)
        {
            _eisc.SetBool(1111, pressRelease);
        }

        #endregion

        #region INumericKeypad Members

        /// <summary>
        /// Digit0 method
        /// </summary>
        public void Digit0(bool pressRelease)
        {
            _eisc.SetBool(1111, pressRelease);
        }

        /// <summary>
        /// Digit1 method
        /// </summary>
        public void Digit1(bool pressRelease)
        {
            _eisc.SetBool(1111, pressRelease);
        }

        /// <summary>
        /// Digit2 method
        /// </summary>
        public void Digit2(bool pressRelease)
        {
            _eisc.SetBool(1111, pressRelease);
        }

        /// <summary>
        /// Digit3 method
        /// </summary>
        public void Digit3(bool pressRelease)
        {
            _eisc.SetBool(1111, pressRelease);
        }

        /// <summary>
        /// Digit4 method
        /// </summary>
        public void Digit4(bool pressRelease)
        {
            _eisc.SetBool(1111, pressRelease);
        }

        /// <summary>
        /// Digit5 method
        /// </summary>
        public void Digit5(bool pressRelease)
        {
            _eisc.SetBool(1111, pressRelease);
        }

        /// <summary>
        /// Digit6 method
        /// </summary>
        public void Digit6(bool pressRelease)
        {
            _eisc.SetBool(1111, pressRelease);
        }

        /// <summary>
        /// Digit7 method
        /// </summary>
        public void Digit7(bool pressRelease)
        {
            _eisc.SetBool(1111, pressRelease);
        }

        /// <summary>
        /// Digit8 method
        /// </summary>
        public void Digit8(bool pressRelease)
        {
            _eisc.SetBool(1111, pressRelease);
        }

        /// <summary>
        /// Digit9 method
        /// </summary>
        public void Digit9(bool pressRelease)
        {
            _eisc.SetBool(1111, pressRelease);
        }

        public bool HasKeypadAccessoryButton1
        {
            get { throw new NotImplementedException(); }
        }

        public string KeypadAccessoryButton1Label
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// KeypadAccessoryButton1 method
        /// </summary>
        public void KeypadAccessoryButton1(bool pressRelease)
        {
            throw new NotImplementedException();
        }

        public bool HasKeypadAccessoryButton2
        {
            get { throw new NotImplementedException(); }
        }

        public string KeypadAccessoryButton2Label
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// KeypadAccessoryButton2 method
        /// </summary>
        public void KeypadAccessoryButton2(bool pressRelease)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}