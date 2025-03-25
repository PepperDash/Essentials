using Crestron.SimplSharpPro.EthernetCommunication;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using System;

namespace PepperDash.Essentials.Room.MobileControl
{
    /// <summary>
    /// Represents a generic device connection through to and EISC for SIMPL01
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

        public void ChannelUp(bool pressRelease)
        {
            _eisc.SetBool(1111, pressRelease);
        }

        public void ChannelDown(bool pressRelease)
        {
            _eisc.SetBool(1111, pressRelease);
        }

        public void LastChannel(bool pressRelease)
        {
            _eisc.SetBool(1111, pressRelease);
        }

        public void Guide(bool pressRelease)
        {
            _eisc.SetBool(1111, pressRelease);
        }

        public void Info(bool pressRelease)
        {
            _eisc.SetBool(1111, pressRelease);
        }

        public void Exit(bool pressRelease)
        {
            _eisc.SetBool(1111, pressRelease);
        }

        #endregion

        #region INumericKeypad Members

        public void Digit0(bool pressRelease)
        {
            _eisc.SetBool(1111, pressRelease);
        }

        public void Digit1(bool pressRelease)
        {
            _eisc.SetBool(1111, pressRelease);
        }

        public void Digit2(bool pressRelease)
        {
            _eisc.SetBool(1111, pressRelease);
        }

        public void Digit3(bool pressRelease)
        {
            _eisc.SetBool(1111, pressRelease);
        }

        public void Digit4(bool pressRelease)
        {
            _eisc.SetBool(1111, pressRelease);
        }

        public void Digit5(bool pressRelease)
        {
            _eisc.SetBool(1111, pressRelease);
        }

        public void Digit6(bool pressRelease)
        {
            _eisc.SetBool(1111, pressRelease);
        }

        public void Digit7(bool pressRelease)
        {
            _eisc.SetBool(1111, pressRelease);
        }

        public void Digit8(bool pressRelease)
        {
            _eisc.SetBool(1111, pressRelease);
        }

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

        public void KeypadAccessoryButton2(bool pressRelease)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}