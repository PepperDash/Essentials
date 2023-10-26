using System;
using Crestron.SimplSharp.Reflection;
using PepperDash.Core;

namespace PepperDash.Essentials.Core
{
    [AttributeUsage(AttributeTargets.All)]
    public class JoinNameAttribute : CAttribute
    {
        private string _Name;

        public JoinNameAttribute(string name)
        {
            Debug.Console(2, "Setting Attribute Name: {0}", name);
            _Name = name;
        }

        public string Name
        {
            get { return _Name; }
        }
    }
}