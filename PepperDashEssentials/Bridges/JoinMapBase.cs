using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace PepperDash.Essentials.Bridges
{
    public static class JoinMapHelper
    {
        /// <summary>
        /// Attempts to get the join map from config
        /// </summary>
        /// <param name="joinMapKey"></param>
        /// <returns></returns>
        public static JoinMapBase GetJoinMapForDevice(string joinMapKey)
        {
            if (!string.IsNullOrEmpty(joinMapKey))
                return null;

            // FUTURE TODO: Get the join map from the ConfigReader.ConfigObject 

            return null;
        }
    }


    public abstract class JoinMapBase
    {
        /// <summary>
        /// Modifies all the join numbers by adding the offset.  This should never be called twice
        /// </summary>
        /// <param name="joinStart"></param>
        public abstract void OffsetJoinNumbers(uint joinStart);


    }


}