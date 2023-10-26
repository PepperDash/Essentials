namespace PepperDash.Essentials
{
    /// <summary>
    /// For hanging off various common AV things that child drivers might need from a parent AV driver
    /// </summary>
    public interface IAVDriver
    {
        PanelDriverBase Parent { get; }
        JoinedSigInterlock PopupInterlock { get; }
        void ShowNotificationRibbon(string message, int timeout);
        void HideNotificationRibbon();
        void ShowTech();
        uint StartPageVisibleJoin { get; }
    }
}