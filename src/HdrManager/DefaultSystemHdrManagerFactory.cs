using Playnite;

namespace HdrManager
{
    public class DefaultSystemHdrManagerFactory : ISystemHdrManagerFactory
    {
        public ISystemHdrManager Create(IPlayniteApi playniteApi) => new SystemHdrManager(playniteApi);
    }
}
