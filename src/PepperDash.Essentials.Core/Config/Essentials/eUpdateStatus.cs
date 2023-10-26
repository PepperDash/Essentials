namespace PepperDash.Essentials.Core.Config
{
    public enum eUpdateStatus
    {
        UpdateStarted,
        ConfigFileReceived,
        ArchivingConfigs,
        DeletingLocalConfig,
        WritingConfigFile,
        RestartingProgram,
        UpdateSucceeded,
        UpdateFailed
    }
}