using Playnite;

namespace HdrManager
{
    public interface ISystemHdrManagerFactory
    {
        ISystemHdrManager Create(IPlayniteApi playniteApi);
    }
}
