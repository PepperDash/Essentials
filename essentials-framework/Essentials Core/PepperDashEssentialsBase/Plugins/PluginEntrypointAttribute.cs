using System;

namespace PepperDash.Essentials.Core.Plugins
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class PluginEntryPointAttribute : Attribute
    {
        private readonly string _uniqueKey;

        public string UniqueKey {
            get { return _uniqueKey; }
        }

        public PluginEntryPointAttribute(string key)
        {
            _uniqueKey = key;
        }
    }
}