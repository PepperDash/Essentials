using System;
using System.Linq;
using Crestron.SimplSharp.Reflection;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Bridges
{
    [Obsolete("Please use version PepperDash.Essentials.Core.Bridges")]
    public class C2nRthsControllerJoinMap:JoinMapBase
    {
        public uint IsOnline { get; set; }
        public uint Name { get; set; }
        public uint Temperature { get; set; }
        public uint Humidity { get; set; }
        public uint TemperatureFormat { get; set; }

        public C2nRthsControllerJoinMap()
        {
            //digital
            IsOnline = 1;
            TemperatureFormat = 2;

            //Analog
            Temperature = 2;
            Humidity = 3;

            //serial
            Name = 1;

            
        }

        public override void OffsetJoinNumbers(uint joinStart)
        {
            var joinOffset = joinStart - 1;
            var properties =
                GetType().GetCType().GetProperties().Where(p => p.PropertyType == typeof(uint)).ToList();

            foreach (var propertyInfo in properties)
            {
                propertyInfo.SetValue(this, (uint)propertyInfo.GetValue(this, null) + joinOffset, null);
            }
        }
    }
}