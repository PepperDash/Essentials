using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace PepperDash.Essentials.Devices.Common.DSP
{
	public abstract class TesiraForteControlPoint : DspControlPoint
	{
        public string Key { get; protected set; }

		public string InstanceTag { get; set; }
        public int Index1 { get; private set; }
        public int Index2 { get; private set; }
		public BiampTesiraForteDsp Parent { get; private set; }

        public bool IsSubscribed { get; protected set; }

		protected TesiraForteControlPoint(string id, int index1, int index2,  BiampTesiraForteDsp parent)
		{
			InstanceTag = id;
			Index1 = index1;
            Index2 = index2;
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
		public virtual void SendFullCommand(string command, string attributeCode, string value)
		{
			// Command Format: InstanceTag get/set/toggle/increment/decrement/subscribe/unsubscribe attributeCode [index] [value]
			// Ex: "RoomLevel set level 1.00"

            string cmd;

            if (attributeCode == "level" || attributeCode == "mute" || attributeCode == "minLevel" || attributeCode == "maxLevel" || attributeCode == "label" || attributeCode == "rampInterval" || attributeCode == "rampStep")
            {
                //Command requires Index

                if (String.IsNullOrEmpty(value))
                {
                    // format command without value
                    cmd = string.Format("{0} {1} {2} {3}", InstanceTag, command, attributeCode, Index1);
                }
                else
                {
                    // format commadn with value
                    cmd = string.Format("{0} {1} {2} {3} {4}", InstanceTag, command, attributeCode, Index1, value);
                }

            }
            else
            {
                //Command does not require Index

                if (String.IsNullOrEmpty(value))
                {
                    cmd = string.Format("{0} {1} {2} {3}", InstanceTag, command, attributeCode, value);
                }
                else
                {
                    cmd = string.Format("{0} {1} {2}", InstanceTag, command, attributeCode);
                }
            }

            if (command == "get")
            {
                // This command will generate a return value response so it needs to be queued
                Parent.EnqueueCommand(new BiampTesiraForteDsp.QueuedCommand{ Command = cmd, AttributeCode = attributeCode, ControlPoint = this });
            }
            else
            {
                // This command will generate a simple "+OK" response and doesn't need to be queued
                Parent.SendLine(cmd);
            }

		}

        virtual public void ParseGetMessage(string attributeCode, string message)
        {
        }



        public virtual void SendSubscriptionCommand(string customName, string attributeCode, int responseRate)
        {
            // Subscription string format: InstanceTag subscribe attributeCode Index1 customName responseRate
            // Ex: "RoomLevel subscribe level 1 MyRoomLevel 500"

            string cmd;

            if (responseRate > 0)
            {
                cmd = string.Format("{0} subscribe {1} {2} {3} {4}", InstanceTag, attributeCode, Index1, customName, responseRate);
            }
            else
            {
                cmd = string.Format("{0} subscribe {1} {2} {3}", InstanceTag, attributeCode, Index1, customName);
            }

            Parent.SendLine(cmd);
        }

        
	}
}