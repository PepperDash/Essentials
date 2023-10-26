using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Core;


namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// For rooms with in call feedback
    /// </summary>
    public interface IHasInCallFeedback
    {
        BoolFeedback InCallFeedback { get; }
    }
}