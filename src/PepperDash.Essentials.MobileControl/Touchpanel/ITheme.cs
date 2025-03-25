using PepperDash.Core;
using PepperDash.Essentials.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PepperDash.Essentials.Touchpanel
{
    public interface ITheme:IKeyed
    { 
        string Theme { get; }

        void UpdateTheme(string theme);
    }
}
