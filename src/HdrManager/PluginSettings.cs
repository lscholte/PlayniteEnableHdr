using System;
using System.Collections.Generic;

namespace HdrManager
{
    public class PluginSettings : IPluginSettings
    {
        private readonly Plugin _plugin;

        [Obsolete("This constructor is only used by Playnite for JSON deserialization. Use the overloads with required parameters instead.")]
        public PluginSettings()
        {
            // Suppress a nullable reference type warning as
            // usages of this constructor are only for deserialization
            // and should not use anything that relies on _plugin being set.
            _plugin = null!;
        }

        public PluginSettings(Plugin plugin)
        {
            _plugin = plugin ?? throw new ArgumentNullException(nameof(plugin));

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
            _plugin.SavePluginSettings(this);
        }

        public bool VerifySettings(out List<string> errors)
        {
            errors = new List<string>();
            return true;
        }
    }
}
