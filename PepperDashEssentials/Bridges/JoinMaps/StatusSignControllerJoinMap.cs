using System.Linq;
using Crestron.SimplSharp.Reflection;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Bridges
{
    public class StatusSignControllerJoinMap:JoinMapBase
    {
        public uint IsOnline { get; set; }
        public uint Name { get; set; }
        public uint RedLed { get; set; }
        public uint GreenLed { get; set; }
        public uint BlueLed { get; set; }
        public uint RedControl { get; set; }
        public uint GreenControl { get; set; }
        public uint BlueControl { get; set; }

        public StatusSignControllerJoinMap()
        {
            //digital
            IsOnline = 1;
            RedControl = 2;
            GreenControl = 3;
            BlueControl = 4;

            //Analog
            RedLed = 2;
            GreenLed = 3;
            BlueLed = 4;

            //string 
            Name = 1;


        }

        public override void OffsetJoinNumbers(uint joinStart)
        {
            var joinOffset = joinStart - 1;
            //digital
            IsOnline += joinOffset;
            RedControl += joinOffset;
            GreenControl += joinOffset;
            BlueControl += joinOffset;

            //Analog
            RedLed += joinOffset;
            GreenLed += joinOffset;
            BlueLed += joinOffset;

            //string 
            Name += joinOffset;
        }
    }
}