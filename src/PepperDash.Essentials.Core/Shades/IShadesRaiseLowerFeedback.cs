namespace PepperDash.Essentials.Core.Shades
{
    /// <summary>
    /// Requirements for a shade device that provides raising/lowering feedback
    /// </summary>
    public interface IShadesRaiseLowerFeedback
    {
        BoolFeedback ShadeIsLoweringFeedback { get; }
        BoolFeedback ShadeIsRaisingFeedback { get; }
    }
}