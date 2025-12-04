using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace HdrManager
{
    public class Plugin : GenericPlugin
    {
        #region Private Fields

        private readonly PluginSettings pluginSettings;
        private readonly SystemHdrManager systemHdrManager;

        private readonly Guid PCGamingWikiPluginId = Guid.Parse("c038558e-427b-4551-be4c-be7009ce5a8d");

        #endregion

        #region Constructors

        public Plugin(IPlayniteAPI api) : base(api)
        {
            Properties = new GenericPluginProperties()
            {
                HasSettings = true
            };
            pluginSettings = new PluginSettings(this);
            systemHdrManager = new SystemHdrManager(api);
        }

        #endregion

        #region Public Properties

        public override Guid Id { get; } = Guid.Parse("b73b5b49-acdf-4da4-a2cc-b91d34d57c9a");

        #endregion

        #region Public Methods

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return pluginSettings;
        }

        public override UserControl GetSettingsView(bool firstRunView)
        {
            return new PluginSettingsView();
        }

        public override void OnApplicationStarted(OnApplicationStartedEventArgs args)
        {
            systemHdrManager.CreateOrUpdateHdrExclusionTag(PlayniteApi.Resources.GetString("HdrManagerExclusionTag"));
            systemHdrManager.EnableSystemHdrForManagedGames();
            ShowPcGamingWikiWarning();
        }

        public override void OnLibraryUpdated(OnLibraryUpdatedEventArgs args)
        {
            systemHdrManager.EnableSystemHdrForManagedGames();
        }

        public override IEnumerable<GameMenuItem> GetGameMenuItems(GetGameMenuItemsArgs args)
        {
            if (args.Games.All(game => game.HasHdrExclusionTag(SystemHdrManager.HdrExclusionTagId)))
            {
                yield return new GameMenuItem
                {
                    Description = PlayniteApi.Resources.GetString("ContextMenuRemoveExclusionTag"),
                    MenuSection = PlayniteApi.Resources.GetString("ContextMenuSectionHeader"),
                    Action = (a) => systemHdrManager.RemoveHdrExclusionTagFromGames(a.Games)
                };
            }
            else
            {
                yield return new GameMenuItem
                {
                    Description = PlayniteApi.Resources.GetString("ContextMenuAddExclusionTag"),
                    MenuSection = PlayniteApi.Resources.GetString("ContextMenuSectionHeader"),
                    Action = (a) =>
                    {
                        systemHdrManager.CreateOrUpdateHdrExclusionTag(PlayniteApi.Resources.GetString("HdrManagerExclusionTag"));
                        systemHdrManager.AddHdrExclusionTagToGames(a.Games);
                    }
                };
            }

            if (args.Games.All(game => game.EnableSystemHdr))
            {
                yield return new GameMenuItem
                {
                    Description = PlayniteApi.Resources.GetString("ContextMenuDisableHdrSupport"),
                    MenuSection = PlayniteApi.Resources.GetString("ContextMenuSectionHeader"),
                    Action = (a) => systemHdrManager.SetSystemHdrForGames(a.Games, false)
                };
            }
            else
            {
                yield return new GameMenuItem
                {
                    Description = PlayniteApi.Resources.GetString("ContextMenuEnableHdrSupport"),
                    MenuSection = PlayniteApi.Resources.GetString("ContextMenuSectionHeader"),
                    Action = (a) => systemHdrManager.SetSystemHdrForGames(a.Games, true)
                };
            }
        }

        public override IEnumerable<MainMenuItem> GetMainMenuItems(GetMainMenuItemsArgs args)
        {
            yield return new MainMenuItem
            {
                Description = PlayniteApi.Resources.GetString("ExtensionMenuRunHdrActivation"),
                MenuSection = "@",
                Action = _ => systemHdrManager.EnableSystemHdrForManagedGames()
            };
        }

        #endregion

        #region Private Methods

        private void ShowPcGamingWikiWarning()
        {
            if (!pluginSettings.IsPCGamingWikiWarningSuppressed &&
                !PlayniteApi.Addons.Plugins.Any(plugin => plugin.Id == PCGamingWikiPluginId))
            {
                var okResponse = new MessageBoxOption(PlayniteApi.Resources.GetString("DialogResponseOK"), true, true);
                var suppressWarningResponse = new MessageBoxOption(PlayniteApi.Resources.GetString("DialogResponseSuppressWarning"));

                List<MessageBoxOption> options = new List<MessageBoxOption>()
                {
                    okResponse,
                    suppressWarningResponse
                };

                MessageBoxOption response = PlayniteApi.Dialogs.ShowMessage(
                    PlayniteApi.Resources.GetString("PCGamingWikiDialogWarningMessage"),
                    "",
                    MessageBoxImage.Warning,
                    options);
                if (response == suppressWarningResponse)
                {
                    pluginSettings.IsPCGamingWikiWarningSuppressed = true;
                    SavePluginSettings(pluginSettings);
                }
            }
        }

        #endregion
    }
}