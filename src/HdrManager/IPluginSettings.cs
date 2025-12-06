using Playnite.SDK;

namespace HdrManager
{
    public interface IPluginSettings : ISettings
    {
        bool IsPCGamingWikiWarningSuppressed { get; set; }
    }
}
