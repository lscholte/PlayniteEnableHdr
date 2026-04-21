namespace HdrManager
{
    public interface IPluginSettingsStoreFactory
    {
        IPluginSettingsStore Create(string settingsDirectory);
    }
}
