using HdrManager.Extension;
using HdrManager.Localization.Generated;
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

        private readonly IPluginSettings _pluginSettings;
        private readonly ISystemHdrManager _systemHdrManager;

        #endregion

        #region Constructors

        public Plugin(IPlayniteAPI api)
            : this(api, null, null)
        {
        }

        public Plugin(IPlayniteAPI api, IPluginSettings? pluginSettings, ISystemHdrManager? systemHdrManager)
            : base(api)
        {
            Properties = new GenericPluginProperties()
            {
                HasSettings = true
            };
            _pluginSettings = pluginSettings ?? new PluginSettings(this);
            _systemHdrManager = systemHdrManager ?? new SystemHdrManager(api);
        }

        #endregion

        #region Public Properties

        public static Guid PCGamingWikiPluginId { get; } = Guid.Parse("c038558e-427b-4551-be4c-be7009ce5a8d");

        public override Guid Id { get; } = Guid.Parse("b73b5b49-acdf-4da4-a2cc-b91d34d57c9a");

        #endregion

        #region Public Methods

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return _pluginSettings;
        }

        public override UserControl GetSettingsView(bool firstRunView)
        {
            return new PluginSettingsView();
        }

        public override void OnApplicationStarted(OnApplicationStartedEventArgs args)
        {
            _systemHdrManager.CreateOrUpdateHdrExclusionTag(PlayniteApi.Resources.GetString(LocalizationKeys.HdrManagerExclusionTag));
            _systemHdrManager.EnableSystemHdrForManagedGames();
            ShowPcGamingWikiWarning();
        }

        public override void OnLibraryUpdated(OnLibraryUpdatedEventArgs args)
        {
            _systemHdrManager.EnableSystemHdrForManagedGames();
        }

        public override IEnumerable<GameMenuItem> GetGameMenuItems(GetGameMenuItemsArgs args)
        {
            if (args.Games.All(game => game.HasTag(SystemHdrManager.HdrExclusionTagId)))
            {
                yield return new GameMenuItem
                {
                    Description = PlayniteApi.Resources.GetString(LocalizationKeys.ContextMenuRemoveExclusionTag),
                    MenuSection = PlayniteApi.Resources.GetString(LocalizationKeys.ContextMenuSectionHeader),
                    Action = (a) => _systemHdrManager.RemoveHdrExclusionTagFromGames(a.Games)
                };
            }
            else
            {
                yield return new GameMenuItem
                {
                    Description = PlayniteApi.Resources.GetString(LocalizationKeys.ContextMenuAddExclusionTag),
                    MenuSection = PlayniteApi.Resources.GetString(LocalizationKeys.ContextMenuSectionHeader),
                    Action = (a) =>
                    {
                        _systemHdrManager.CreateOrUpdateHdrExclusionTag(PlayniteApi.Resources.GetString(LocalizationKeys.HdrManagerExclusionTag));
                        _systemHdrManager.AddHdrExclusionTagToGames(a.Games);
                    }
                };
            }

            if (args.Games.All(game => game.EnableSystemHdr))
            {
                yield return new GameMenuItem
                {
                    Description = PlayniteApi.Resources.GetString(LocalizationKeys.ContextMenuDisableHdrSupport),
                    MenuSection = PlayniteApi.Resources.GetString(LocalizationKeys.ContextMenuSectionHeader),
                    Action = (a) => _systemHdrManager.SetSystemHdrForGames(a.Games, false)
                };
            }
            else
            {
                yield return new GameMenuItem
                {
                    Description = PlayniteApi.Resources.GetString(LocalizationKeys.ContextMenuEnableHdrSupport),
                    MenuSection = PlayniteApi.Resources.GetString(LocalizationKeys.ContextMenuSectionHeader),
                    Action = (a) => _systemHdrManager.SetSystemHdrForGames(a.Games, true)
                };
            }
        }

        public override IEnumerable<MainMenuItem> GetMainMenuItems(GetMainMenuItemsArgs args)
        {
            yield return new MainMenuItem
            {
                Description = PlayniteApi.Resources.GetString(LocalizationKeys.ExtensionMenuRunHdrActivation),
                MenuSection = "@",
                Action = _ => _systemHdrManager.EnableSystemHdrForManagedGames()
            };
        }

        #endregion

        #region Private Methods

        private void ShowPcGamingWikiWarning()
        {
            if (!_pluginSettings.IsPCGamingWikiWarningSuppressed &&
                !PlayniteApi.Addons.Plugins.Any(plugin => plugin.Id == PCGamingWikiPluginId))
            {
                var okResponse = new MessageBoxOption(PlayniteApi.Resources.GetString(LocalizationKeys.DialogResponseOK), true, true);
                var suppressWarningResponse = new MessageBoxOption(PlayniteApi.Resources.GetString(LocalizationKeys.DialogResponseSuppressWarning));

                List<MessageBoxOption> options = new List<MessageBoxOption>()
                {
                    okResponse,
                    suppressWarningResponse
                };

                MessageBoxOption response = PlayniteApi.Dialogs.ShowMessage(
                    PlayniteApi.Resources.GetString(LocalizationKeys.PCGamingWikiDialogWarningMessage),
                    "",
                    MessageBoxImage.Warning,
                    options);
                if (response == suppressWarningResponse)
                {
                    _pluginSettings.BeginEdit();
                    _pluginSettings.IsPCGamingWikiWarningSuppressed = true;
                    _pluginSettings.EndEdit();
                }
            }
        }

        #endregion
    }
}