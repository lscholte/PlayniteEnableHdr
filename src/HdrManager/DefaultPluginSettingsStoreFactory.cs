namespace HdrManager
{
    public class DefaultPluginSettingsStoreFactory : IPluginSettingsStoreFactory
    {
        public IPluginSettingsStore Create(string settingsDirectory) => new PluginSettingsStore(settingsDirectory);
    }
}
