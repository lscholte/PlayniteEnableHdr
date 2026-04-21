using Microsoft.Extensions.DependencyInjection;
using Playnite;
using System;

namespace HdrManager
{
    public class PluginServiceProviderFactory : IPluginServiceProviderFactory
    {
        public IServiceProvider CreateServiceProvider(IPlayniteApi playniteApi)
        {
            ArgumentNullException.ThrowIfNull(playniteApi);

            var services = new ServiceCollection();
            services.AddSingleton<IPluginSettingsStore>(new PluginSettingsStore(playniteApi.UserDataDir));
            services.AddSingleton<ISystemHdrManager>(new SystemHdrManager(playniteApi));
            return services.BuildServiceProvider();
        }
    }
}
