extern alias Full;
using System.Text;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Crestron.SimplSharp;

using PepperDash.Core;
using Full.Newtonsoft.Json.Linq;

namespace PepperDash.Essentials.Devices.Common.VideoCodec.ZoomRoom
{
    public abstract class NotifiableObject : INotifyPropertyChanged
	{
		#region INotifyPropertyChanged Members

		public event PropertyChangedEventHandler PropertyChanged;

		protected void NotifyPropertyChanged(string propertyName)
		{
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
            else
            {
                Debug.Console(2, "PropertyChanged event is NULL");
            }
		}

		#endregion
	}
}