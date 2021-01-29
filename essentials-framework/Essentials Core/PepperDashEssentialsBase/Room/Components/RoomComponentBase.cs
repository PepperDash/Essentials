using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Essentials.Core.Interfaces.Components;

namespace PepperDash.Essentials.Core.Room.Components
{
    /// <summary>
    /// The base class from which Room Components should be derived
    /// </summary>
    public abstract class RoomComponentBase : IRoomComponent
    {
        private string _componentKey;

        /// <summary>
        /// The key of the component, which is composed of the parent room key, plus the specific component key
        /// </summary>
        public string Key {
            get
            {
                return Parent.Key + "-" + _componentKey;
            }
        }

        public IComponentRoom Parent { get; private set; }

        public RoomComponentBase(string key, IComponentRoom parent)
        {
            _componentKey = key;
            Parent = parent;
        }

    }


}