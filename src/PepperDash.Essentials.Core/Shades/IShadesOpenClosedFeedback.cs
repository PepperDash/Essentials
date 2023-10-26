namespace PepperDash.Essentials.Core.Shades
{
    /// <summary>
    /// Requirements for a shade/scene that is open or closed
    /// </summary>
    public interface IShadesOpenClosedFeedback: IShadesOpenCloseStop
    {
        BoolFeedback ShadeIsOpenFeedback { get; }
        BoolFeedback ShadeIsClosedFeedback { get; }
    }
}