using HdrManager.Extension;
using HdrManager.Localization.Generated;
using Playnite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HdrManager
{
    public class Plugin : Playnite.Plugin
    {
        #region Private Fields

        private readonly IPluginSettingsStoreFactory _settingsStoreFactory;
        private readonly ISystemHdrManagerFactory _systemHdrManagerFactory;

        private IPlayniteApi _playniteApi = null!;
        private IPluginSettingsStore _settingsStore = null!;
        private ISystemHdrManager _systemHdrManager = null!;

        #endregion

        #region Constructors

        public Plugin()
            : this(
                  new DefaultPluginSettingsStoreFactory(),
                  new DefaultSystemHdrManagerFactory())
        {
        }

        public Plugin(
            IPluginSettingsStoreFactory settingsStoreFactory,
            ISystemHdrManagerFactory systemHdrManagerFactory)
        {
            _settingsStoreFactory = settingsStoreFactory
                ?? throw new ArgumentNullException(nameof(settingsStoreFactory));
            _systemHdrManagerFactory = systemHdrManagerFactory
                ?? throw new ArgumentNullException(nameof(systemHdrManagerFactory));
        }

        #endregion

        #region Public Properties

        // TODO: Replace this with the correct plugin ID for Playnite 11 once it's available
        public static string PCGamingWikiPluginId { get; } = "c038558e-427b-4551-be4c-be7009ce5a8d";

        public const string Id = "lscholte.HdrManager";

        public IPluginSettings Settings { get; set; } = null!;

        #endregion

        #region Public Methods

        public override async Task InitializeAsync(InitializeArgs args)
        {
            _playniteApi = args.Api;
            _settingsStore = _settingsStoreFactory.Create(args.Api.UserDataDir);
            _systemHdrManager = _systemHdrManagerFactory.Create(args.Api);

            Settings = await _settingsStore.LoadSettingsAsync();
        }

        public override async Task<Playnite.PluginSettingsHandler?> GetSettingsHandlerAsync(GetSettingsHandlerArgs args)
        {
            await Task.CompletedTask;
            return new PluginSettingsHandler(this);
        }

        public override async Task OnApplicationStartupAsync(OnApplicationStartupArgs args)
        {
            await _systemHdrManager.CreateOrUpdateHdrExclusionTag(_playniteApi.GetLocalizedString(LocalizationKeys.HdrManagerExclusionTag));
            await _systemHdrManager.EnableSystemHdrForManagedGames();
            await ShowPcGamingWikiWarning();
        }

        public override async Task OnLibraryUpdateFinishedAsync(OnLibraryUpdateFinishedArgs args)
        {
            await _systemHdrManager.EnableSystemHdrForManagedGames();
        }

        public override ICollection<MenuItemDescriptor> GetGameMenuItemDescriptors(GetGameMenuItemDescriptorsArgs args)
        {
            return
            [
                new MenuItemDescriptor(
                    LocalizationKeys.ContextMenuSectionHeader,
                    _playniteApi.GetLocalizedString(LocalizationKeys.ContextMenuSectionHeader))
            ];
        }

        public override ICollection<MenuItemImpl>? GetGameMenuItems(GetGameMenuItemsArgs args)
        {
            IEnumerable<MenuItemImpl> GenerateToggleHdrMenuItems(MenuItemImpl.GetChildrenArgs _)
            {
                if (args.Games.All(game => game.EnableSystemHdr))
                {
                    yield return new MenuItemImpl(
                        _playniteApi.GetLocalizedString(LocalizationKeys.ContextMenuDisableHdrSupport),
                        async () => await _systemHdrManager.SetSystemHdrForGames(args.Games, false));
                }
                else
                {
                    yield return new MenuItemImpl(
                        _playniteApi.GetLocalizedString(LocalizationKeys.ContextMenuEnableHdrSupport),
                        async () => await _systemHdrManager.SetSystemHdrForGames(args.Games, true));
                }
            }

            IEnumerable<MenuItemImpl> GenerateToggleExclusionTagMenuItems(MenuItemImpl.GetChildrenArgs _)
            {
                if (args.Games.All(game => game.HasTag(SystemHdrManager.HdrExclusionTagId)))
                {
                    yield return new MenuItemImpl(
                        _playniteApi.GetLocalizedString(LocalizationKeys.ContextMenuRemoveExclusionTag),
                        async () => await _systemHdrManager.RemoveHdrExclusionTagFromGames(args.Games));
                }
                else
                {
                    yield return new MenuItemImpl(
                        _playniteApi.GetLocalizedString(LocalizationKeys.ContextMenuAddExclusionTag),
                        async () =>
                        {
                            await _systemHdrManager.CreateOrUpdateHdrExclusionTag(_playniteApi.GetLocalizedString(LocalizationKeys.HdrManagerExclusionTag));
                            await _systemHdrManager.AddHdrExclusionTagToGames(args.Games);
                        });
                }
            }

            if (args.ItemId == LocalizationKeys.ContextMenuSectionHeader)
            {
                return [
                    new MenuItemImpl(
                        _playniteApi.GetLocalizedString(LocalizationKeys.ContextMenuSectionHeader),
                        childArgs => GenerateToggleHdrMenuItems(childArgs).Concat(GenerateToggleExclusionTagMenuItems(childArgs)))
                ];
            }

            return null;
        }

        public override ICollection<MenuItemDescriptor> GetAppMenuItemDescriptors(GetAppMenuItemDescriptorsArgs args)
        {
            return
            [
                new MenuItemDescriptor(LocalizationKeys.ExtensionMenuRunHdrActivation, _playniteApi.GetLocalizedString(LocalizationKeys.ExtensionMenuRunHdrActivation))
            ];
        }

        public override ICollection<MenuItemImpl>? GetAppMenuItems(GetAppMenuItemsArgs args)
        {
            if (args.ItemId == LocalizationKeys.ExtensionMenuRunHdrActivation)
            {
                return [
                    new MenuItemImpl(
                        _playniteApi.GetLocalizedString(LocalizationKeys.ExtensionMenuRunHdrActivation),
                        async () => await _systemHdrManager.EnableSystemHdrForManagedGames())
                    ];
            }

            return null;
        }

        public async Task SaveSettingsAsync(IPluginSettings settings)
        {
            Settings = settings;
            await _settingsStore.SaveSettingsAsync(settings);
        }

        #endregion

        #region Private Methods

        private async Task ShowPcGamingWikiWarning()
        {
            if (!Settings.IsPCGamingWikiWarningSuppressed &&
                !_playniteApi.Addons.Plugins.Any(plugin => plugin.Id == PCGamingWikiPluginId))
            {
                var okResponse = new MessageBoxResponse(_playniteApi.GetLocalizedString(LocalizationKeys.DialogResponseOk), true, true);
                var suppressWarningResponse = new MessageBoxResponse(_playniteApi.GetLocalizedString(LocalizationKeys.DialogResponseSuppressWarning));

                List<MessageBoxResponse> responses =
                [
                    okResponse,
                    suppressWarningResponse,
                ];

                MessageBoxResponse? response = await _playniteApi.Dialogs.ShowMessageAsync(
                    _playniteApi.GetLocalizedString(LocalizationKeys.PCGamingWikiDialogWarningMessage),
                    "",
                    MessageBoxSeverity.Warning,
                    responses,
                    []);

                if (response == suppressWarningResponse)
                {
                    Settings.IsPCGamingWikiWarningSuppressed = true;
                    await SaveSettingsAsync(Settings);
                }
            }
        }

        #endregion
    }
}