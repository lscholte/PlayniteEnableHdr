using CommunityToolkit.Mvvm.ComponentModel;

namespace HdrManager
{
    public partial class PluginSettings : ObservableObject, IPluginSettings
    {
        [ObservableProperty] private bool _isPCGamingWikiWarningSuppressed;
    }
}