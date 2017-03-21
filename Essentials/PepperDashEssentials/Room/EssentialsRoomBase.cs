using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Core;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials
{
    /// <summary>
    /// 
    /// </summary>
    public interface IHasCurrentSourceInfoChange
    {
        event SourceInfoChangeHandler CurrentSingleSourceChange;
    }


    /// <summary>
    /// 
    /// </summary>
    public class EssentialsRoomBase : Device
    {
        public EssentialsRoomBase(string key, string name) : base(key, name)
        {

        }
    }
}