namespace PepperDash.Essentials.Core.Lighting
{
    /// <summary>
    /// Requiremnts for controlling a lighting load
    /// </summary>
    public interface ILightingLoad
    {
        void SetLoadLevel(int level);
        void Raise();
        void Lower();

        IntFeedback LoadLevelFeedback { get; }
        BoolFeedback LoadIsOnFeedback { get; }
    }
}