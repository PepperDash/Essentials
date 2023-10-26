extern alias Full;
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharp.Reflection;
using Full::Newtonsoft.Json;

namespace PepperDash.Essentials.Core
{
    public class MethodNameParams
    {
        [JsonIgnore]
        public MethodInfo MethodInfo { get; private set; }

        public string Name { get { return MethodInfo.Name; } }
        public IEnumerable<NameType> Params { get {
            return MethodInfo.GetParameters().Select(p => 
                new NameType { Name = p.Name, Type = p.ParameterType.Name });
        } }

        public MethodNameParams(MethodInfo info)
        {
            MethodInfo = info;
        }
    }
}