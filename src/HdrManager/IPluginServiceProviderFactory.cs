using Playnite;
using System;

namespace HdrManager
{
    public interface IPluginServiceProviderFactory
    {
        IServiceProvider CreateServiceProvider(IPlayniteApi playniteApi);
    }
}
