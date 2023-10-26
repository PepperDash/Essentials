extern alias Full;
using System;
using Crestron.SimplSharp.Reflection;
using Full::Newtonsoft.Json;

namespace PepperDash.Essentials.Core
{
    public class PropertyNameType
    {
        object Parent;

        [JsonIgnore]
        public PropertyInfo PropInfo { get; private set; }
        public string Name { get { return PropInfo.Name; } }
        public string Type { get { return PropInfo.PropertyType.Name; } }
        public string Value { get 
        {
            if (PropInfo.CanRead)
            {
                try
                {
                    return PropInfo.GetValue(Parent, null).ToString();
                }
                catch (Exception)
                {
                    return null;
                }
            }
            else
                return null;    
        } }

        public bool CanRead { get { return PropInfo.CanRead; } }
        public bool CanWrite { get { return PropInfo.CanWrite; } }


        public PropertyNameType(PropertyInfo info, object parent)
        {
            PropInfo = info;
            Parent   = parent;
        }
    }
}