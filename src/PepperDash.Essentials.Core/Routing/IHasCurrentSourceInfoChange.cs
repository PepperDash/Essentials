namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// For rooms with a single presentation source, change event
    /// </summary>
    public interface IHasCurrentSourceInfoChange
    {
        string CurrentSourceInfoKey { get; set; }
        SourceListItem CurrentSourceInfo { get; set; }
        event SourceInfoChangeHandler CurrentSourceChange;
    }
}