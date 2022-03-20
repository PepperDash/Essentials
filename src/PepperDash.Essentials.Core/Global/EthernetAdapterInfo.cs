using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace PepperDash.Essentials.Core
{
    public class EthernetAdapterInfo
    {
        public EthernetAdapterType Type { get; set; }
        public bool DhcpIsOn { get; set; }
        public string Hostname { get; set; }
        public string MacAddress { get; set; }
        public string IpAddress { get; set; }
        public string Subnet { get; set; }
        public string Gateway { get; set; }
        public string Dns1 { get; set; }
        public string Dns2 { get; set; }
        public string Dns3 { get; set; }
        public string Domain { get; set; }
    }
}