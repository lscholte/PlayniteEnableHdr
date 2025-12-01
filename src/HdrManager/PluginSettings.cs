using Playnite.SDK;
using System.Collections.Generic;

namespace HdrManager
{
    public class PluginSettings : ISettings
    {
        private Plugin plugin;

        public PluginSettings()
        {
        }

        public PluginSettings(Plugin plugin)
        {
            this.plugin = plugin;

            var savedSettings = plugin.LoadPluginSettings<PluginSettings>();
            if (savedSettings != null)
            {
                IsPCGamingWikiWarningSuppressed = savedSettings.IsPCGamingWikiWarningSuppressed;
            }
        }

        public bool IsPCGamingWikiWarningSuppressed { get; set; } = false;

        public void BeginEdit()
        {
        }

        public void CancelEdit()
        {
        }

        public void EndEdit()
        {
            plugin.SavePluginSettings(this);
        }

        public bool VerifySettings(out List<string> errors)
        {
            errors = new List<string>();
            return true;
        }
    }
}
