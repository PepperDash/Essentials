using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PepperDash.Essentials.Core.DeviceTypeInterfaces
{
    public interface IHasWebView
    {
        void ShowWebView(string url, string mode, string title, string target);
        void HideWebView();
    }
}
