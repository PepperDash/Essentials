using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DeviceSupport;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Devices.Common;

using Newtonsoft.Json;

namespace PepperDash.Essentials.Bridges
{
    public static class IRSetTopBoxBaseApiExtensions
    {
        public static void LinkToApi(this PepperDash.Essentials.Devices.Common.IRSetTopBoxBase stbDevice, BasicTriList trilist, uint joinStart, string joinMapKey)
        {
            SetTopBoxControllerJoinMap joinMap = new SetTopBoxControllerJoinMap();
            var joinMapSerialized = JoinMapHelper.GetSerializedJoinMapForDevice(joinMapKey);

            if (!string.IsNullOrEmpty(joinMapSerialized))
                joinMap = JsonConvert.DeserializeObject<SetTopBoxControllerJoinMap>(joinMapSerialized);

            joinMap.OffsetJoinNumbers(joinStart);

            Debug.Console(1, "Linking to Trilist '{0}'", trilist.ID.ToString("X"));
            Debug.Console(0, "Linking to Display: {0}", stbDevice.Name);

            trilist.StringInput[joinMap.Name].StringValue = stbDevice.Name;

            var stbBase = stbDevice as ISetTopBoxControls;
            if (stbBase != null)
            {
                trilist.BooleanInput[joinMap.HasDpad].BoolValue = stbBase.HasDpad;
                trilist.BooleanInput[joinMap.HasNumeric].BoolValue = stbBase.HasNumeric;
                trilist.BooleanInput[joinMap.HasDvr].BoolValue = stbBase.HasDvr;
                trilist.BooleanInput[joinMap.HasPresets].BoolValue = stbBase.HasPresets;

                trilist.SetBoolSigAction(joinMap.DvrList, (b) => stbBase.DvrList(b));
                trilist.SetBoolSigAction(joinMap.Replay, (b) => stbBase.Replay(b));

                trilist.SetStringSigAction(joinMap.LoadPresets, (s) => stbBase.LoadPresets(s));
            }

            var stbPower = stbDevice as IPower;
            if (stbPower != null)
            {
                trilist.SetSigTrueAction(joinMap.PowerOn, () => stbPower.PowerOn());
                trilist.SetSigTrueAction(joinMap.PowerOff, () => stbPower.PowerOff());
                trilist.SetSigTrueAction(joinMap.PowerToggle, () => stbPower.PowerToggle());

            }

            var stbDPad = stbDevice as IDPad;
            if (stbDPad != null)
            {
                trilist.SetBoolSigAction(joinMap.Up, (b) => stbDPad.Up(b));
                trilist.SetBoolSigAction(joinMap.Down, (b) => stbDPad.Down(b));
                trilist.SetBoolSigAction(joinMap.Left, (b) => stbDPad.Left(b));
                trilist.SetBoolSigAction(joinMap.Right, (b) => stbDPad.Right(b));
                trilist.SetBoolSigAction(joinMap.Select, (b) => stbDPad.Select(b));
                trilist.SetBoolSigAction(joinMap.Menu, (b) => stbDPad.Menu(b));
                trilist.SetBoolSigAction(joinMap.Exit, (b) => stbDPad.Exit(b));
            }

            var stbChannel = stbDevice as IChannel;
            if (stbChannel != null)
            {
                trilist.SetBoolSigAction(joinMap.ChannelUp, (b) => stbChannel.ChannelUp(b));
                trilist.SetBoolSigAction(joinMap.ChannelDown, (b) => stbChannel.ChannelDown(b));
                trilist.SetBoolSigAction(joinMap.LastChannel, (b) => stbChannel.LastChannel(b));
                trilist.SetBoolSigAction(joinMap.Guide, (b) => stbChannel.Guide(b));
                trilist.SetBoolSigAction(joinMap.Info, (b) => stbChannel.Info(b));
                trilist.SetBoolSigAction(joinMap.Exit, (b) => stbChannel.Exit(b));
            }

            var stbColor = stbDevice as IColor;
            if (stbColor != null)
            {
                trilist.SetBoolSigAction(joinMap.Red, (b) => stbColor.Red(b));
                trilist.SetBoolSigAction(joinMap.Green, (b) => stbColor.Green(b));
                trilist.SetBoolSigAction(joinMap.Yellow, (b) => stbColor.Yellow(b));
                trilist.SetBoolSigAction(joinMap.Blue, (b) => stbColor.Blue(b));
            }

            var stbKeypad = stbDevice as ISetTopBoxNumericKeypad;
            if (stbKeypad != null)
            {
                trilist.StringInput[joinMap.KeypadAccessoryButton1Label].StringValue = stbKeypad.KeypadAccessoryButton1Label;
                trilist.StringInput[joinMap.KeypadAccessoryButton2Label].StringValue = stbKeypad.KeypadAccessoryButton2Label;

                trilist.BooleanInput[joinMap.HasKeypadAccessoryButton1].BoolValue = stbKeypad.HasKeypadAccessoryButton1;
                trilist.BooleanInput[joinMap.HasKeypadAccessoryButton2].BoolValue = stbKeypad.HasKeypadAccessoryButton2;

                trilist.SetBoolSigAction(joinMap.Digit0, (b) => stbKeypad.Digit0(b));
                trilist.SetBoolSigAction(joinMap.Digit1, (b) => stbKeypad.Digit1(b));
                trilist.SetBoolSigAction(joinMap.Digit2, (b) => stbKeypad.Digit2(b));
                trilist.SetBoolSigAction(joinMap.Digit3, (b) => stbKeypad.Digit3(b));
                trilist.SetBoolSigAction(joinMap.Digit4, (b) => stbKeypad.Digit4(b));
                trilist.SetBoolSigAction(joinMap.Digit5, (b) => stbKeypad.Digit5(b));
                trilist.SetBoolSigAction(joinMap.Digit6, (b) => stbKeypad.Digit6(b));
                trilist.SetBoolSigAction(joinMap.Digit7, (b) => stbKeypad.Digit7(b));
                trilist.SetBoolSigAction(joinMap.Digit8, (b) => stbKeypad.Digit8(b));
                trilist.SetBoolSigAction(joinMap.Digit9, (b) => stbKeypad.Digit9(b));
                trilist.SetBoolSigAction(joinMap.KeypadAccessoryButton1Press, (b) => stbKeypad.KeypadAccessoryButton1(b));
                trilist.SetBoolSigAction(joinMap.KeypadAccessoryButton2Press, (b) => stbKeypad.KeypadAccessoryButton1(b));
                trilist.SetBoolSigAction(joinMap.Dash, (b) => stbKeypad.Dash(b));
                trilist.SetBoolSigAction(joinMap.KeypadEnter, (b) => stbKeypad.KeypadEnter(b));
            }

            var stbTransport = stbDevice as ITransport;
            if (stbTransport != null)
            {
                trilist.SetBoolSigAction(joinMap.Play, (b) => stbTransport.Play(b));
                trilist.SetBoolSigAction(joinMap.Pause, (b) => stbTransport.Pause(b));
                trilist.SetBoolSigAction(joinMap.Rewind, (b) => stbTransport.Rewind(b));
                trilist.SetBoolSigAction(joinMap.FFwd, (b) => stbTransport.FFwd(b));
                trilist.SetBoolSigAction(joinMap.ChapMinus, (b) => stbTransport.ChapMinus(b));
                trilist.SetBoolSigAction(joinMap.ChapPlus, (b) => stbTransport.ChapPlus(b));
                trilist.SetBoolSigAction(joinMap.Stop, (b) => stbTransport.Stop(b));
                trilist.SetBoolSigAction(joinMap.Record, (b) => stbTransport.Record(b));

            }

        }
    }
}