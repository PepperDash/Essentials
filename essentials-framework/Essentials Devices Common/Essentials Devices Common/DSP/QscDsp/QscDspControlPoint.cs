using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace PepperDash.Essentials.Devices.Common.DSP
{
	public abstract class QscDspControlPoint : DspControlPoint
	{
        public string Key { get; protected set; }

		public string LevelInstanceTag { get; set; }
		public string MuteInstanceTag { get; set; }
		public QscDsp Parent { get; private set; }

        public bool IsSubscribed { get; protected set; }

		protected QscDspControlPoint(string levelInstanceTag, string muteInstanceTag, QscDsp parent)
		{
			LevelInstanceTag = levelInstanceTag;
			MuteInstanceTag = muteInstanceTag;
			Parent = parent;
		}

        virtual public void Initialize()
        {
        }

        /// <summary>
        /// Sends a command to the DSP
        /// </summary>
        /// <param name="command">command</param>
        /// <param name="attribute">attribute code</param>
        /// <param name="value">value (use "" if not applicable)</param>
		public virtual void SendFullCommand(string cmd, string instance, string value)
		{
			
			var cmdToSemd = string.Format("{0} {1} {2}", cmd, instance, value);

			Parent.SendLine(cmdToSemd);
		
		}

        virtual public void ParseGetMessage(string attributeCode, string message)
        {
        }



        public virtual void SendSubscriptionCommand(string instanceTag, string changeGroup)
        {
            // Subscription string format: InstanceTag subscribe attributeCode Index1 customName responseRate
            // Ex: "RoomLevel subscribe level 1 MyRoomLevel 500"

            string cmd;

			cmd = string.Format("cga {0} {1}", changeGroup, instanceTag);

            Parent.SendLine(cmd);
        }

        
	}
}