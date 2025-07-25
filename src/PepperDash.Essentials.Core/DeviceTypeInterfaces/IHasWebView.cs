using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PepperDash.Essentials.Core.DeviceTypeInterfaces
{
    /// <summary>
    /// Defines the contract for IHasWebView
    /// </summary>
    public interface IHasWebView
    {
        bool WebviewIsVisible { get; }
        void ShowWebView(string url, string mode, string title, string target);
        void HideWebView();
        event EventHandler<WebViewStatusChangedEventArgs> WebViewStatusChanged;
    }

    /// <summary>
    /// Represents a WebViewStatusChangedEventArgs
    /// </summary>
    public class WebViewStatusChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the Status
        /// </summary>
        public string Status { get; }

        public WebViewStatusChangedEventArgs(string status)
        {
            Status = status;
        }
    }
}
