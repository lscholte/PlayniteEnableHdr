using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace HdrManager
{
    [INotifyPropertyChanged]
    public partial class PluginSettingsHandler : Playnite.PluginSettingsHandler
    {
        private readonly Plugin _plugin;

        [ObservableProperty] private IPluginSettings _settings;

        public PluginSettingsHandler(Plugin plugin)
        {
            _plugin = plugin ?? throw new ArgumentNullException(nameof(plugin));
            Settings = plugin.Settings;
        }

        public override FrameworkElement GetEditView(GetSettingsViewArgs args)
        {
            return new PluginSettingsView { DataContext = this };
        }

        public override async Task BeginEditAsync(BeginEditArgs args)
        {
            await Task.CompletedTask;
        }

        public override async Task CancelEditAsync(CancelEditArgs args)
        {
            await Task.CompletedTask;
        }

        public override async Task EndEditAsync(EndEditArgs args)
        {
            await _plugin.SaveSettingsAsync(Settings);
        }

        public override async Task<ICollection<string>> VerifySettingsAsync(VerifySettingsArgs args)
        {
            await Task.CompletedTask;
            return Array.Empty<string>();
        }
    }
}
