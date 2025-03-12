using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PepperDash.Essentials.Core.DeviceTypeInterfaces
{
    public interface IHasOSD
    {
        void ShowOsdMessage(string url, string mobileControlPath, string mode, string title, string target);
        void HideOsdMessage();
    }
}
