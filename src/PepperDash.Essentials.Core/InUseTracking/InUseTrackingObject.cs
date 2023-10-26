namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Wrapper for label/object pair representing in-use status.  Allows the same object to
    /// register for in-use with different roles.
    /// </summary>
    public class InUseTrackingObject
    {
        public string Label { get; private set; }
        public object User { get; private set; }

        public InUseTrackingObject(object user, string label)
        {
            User  = user;
            Label = label;
        }
    }
}