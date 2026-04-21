using HdrManager.Localization.Generated;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Playnite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Playnite.Plugin;

namespace HdrManager.Test
{
    [TestFixture]
    public class PluginTest
    {
        private readonly Mock<IDialogs> _mockDialogs;
        private readonly Mock<IAddonsApi> _mockAddons;
        private readonly Mock<IPlayniteApi> _mockPlayniteApi;

        private readonly Mock<IPluginSettingsStore> _mockPluginSettingsStore;
        private readonly Mock<IPluginSettings> _mockPluginSettings;
        private readonly Mock<ISystemHdrManager> _mockSystemHdrManager;

        private readonly Mock<IServiceProvider> _mockServiceProvider;
        private readonly Mock<IPluginServiceProviderFactory> _mockServiceProviderFactory;

        private readonly Game _gameWithHdrExclusionTag;
        private readonly Game _gameWithoutHdrExclusionTag;

        private readonly Game _gameWithSystemHdrEnabled;
        private readonly Game _gameWithSystemHdrDisabled;

        private readonly Plugin _plugin;

        public PluginTest()
        {
            _mockDialogs = new Mock<IDialogs>();

            _mockAddons = new Mock<IAddonsApi>();

            _mockPlayniteApi = new Mock<IPlayniteApi>();
            _mockPlayniteApi
                .SetupGet(mock => mock.Dialogs)
                .Returns(_mockDialogs.Object);
            _mockPlayniteApi
                .SetupGet(mock => mock.Addons)
                .Returns(_mockAddons.Object);
            _mockPlayniteApi
                .Setup(mock => mock.GetLocalizedString(It.IsAny<string>()))
                .Returns((string key) => key);

            _mockPluginSettings = new Mock<IPluginSettings>();
            _mockPluginSettings.SetupAllProperties();

            _mockPluginSettingsStore = new Mock<IPluginSettingsStore>();
            _mockPluginSettingsStore
                .Setup(mock => mock.LoadSettingsAsync())
                .ReturnsAsync(_mockPluginSettings.Object);

            _mockSystemHdrManager = new Mock<ISystemHdrManager>();

            _mockServiceProvider = new Mock<IServiceProvider>();
            _mockServiceProvider
                .Setup(mock => mock.GetService(typeof(IPluginSettingsStore)))
                .Returns(_mockPluginSettingsStore.Object);
            _mockServiceProvider
                .Setup(mock => mock.GetService(typeof(ISystemHdrManager)))
                .Returns(_mockSystemHdrManager.Object);

            _mockServiceProviderFactory = new Mock<IPluginServiceProviderFactory>();
            _mockServiceProviderFactory
                .Setup(mock => mock.CreateServiceProvider(_mockPlayniteApi.Object))
                .Returns(_mockServiceProvider.Object);

            _gameWithoutHdrExclusionTag = new Game
            {
                TagIds = new HashSet<string>()
            };

            _gameWithHdrExclusionTag = new Game
            {
                TagIds = new HashSet<string>()
                {
                    SystemHdrManager.HdrExclusionTagId
                }
            };

            _gameWithSystemHdrEnabled = new Game
            {
                EnableSystemHdr = true
            };

            _gameWithSystemHdrDisabled = new Game
            {
                EnableSystemHdr = false
            };

            _plugin = new Plugin(_mockServiceProviderFactory.Object);
        }

        [SetUp]
        public async Task SetUp()
        {
            var initializeArgs = new Plugin.InitializeArgs(_mockPlayniteApi.Object, "");
            await _plugin.InitializeAsync(initializeArgs);
        }

        [Test]
        public void Constructor_NullServiceProviderFactory_ThrowsArgumentNullException()
        {
            Assert.That(() => new Plugin(null!), Throws.ArgumentNullException);
        }

        [Test]
        public void GetGameMenuItems_SelectedSingleGameWithoutHdrExclusionTag_HasAddHdrExclusionMenuItem()
        {
            var games = new List<Game> { _gameWithoutHdrExclusionTag };
            var menuItemsArgs = new GetGameMenuItemsArgs(LocalizationKeys.ContextMenuSectionHeader, games, GameMenuType.GameDetails);

            IEnumerable<MenuItemImpl>? menuItems = _plugin.GetGameMenuItems(menuItemsArgs);

            Assert.That(menuItems, Has.One.Matches<MenuItemImpl>(item => item.Name == LocalizationKeys.ContextMenuSectionHeader));

            IEnumerable<MenuItemImpl>? subMenuItems = menuItems.First().ChildrenAction?.Invoke(new MenuItemImpl.GetChildrenArgs());

            Assert.That(subMenuItems, Has.Exactly(2).Items);
            Assert.That(subMenuItems, Has.One.Matches<MenuItemImpl>(item => item.Name == LocalizationKeys.ContextMenuAddExclusionTag));
            Assert.That(subMenuItems, Has.None.Matches<MenuItemImpl>(item => item.Name == LocalizationKeys.ContextMenuRemoveExclusionTag));
        }

        [Test]
        public void GetGameMenuItems_SelectedSingleGameWithHdrExclusionTag_HasRemoveHdrExclusionMenuItem()
        {
            var games = new List<Game> { _gameWithHdrExclusionTag };
            var menuItemsArgs = new GetGameMenuItemsArgs(LocalizationKeys.ContextMenuSectionHeader, games, GameMenuType.GameDetails);

            IEnumerable<MenuItemImpl>? menuItems = _plugin.GetGameMenuItems(menuItemsArgs);

            Assert.That(menuItems, Has.One.Matches<MenuItemImpl>(item => item.Name == LocalizationKeys.ContextMenuSectionHeader));

            IEnumerable<MenuItemImpl>? subMenuItems = menuItems.First().ChildrenAction?.Invoke(new MenuItemImpl.GetChildrenArgs());

            Assert.That(subMenuItems, Has.Exactly(2).Items);
            Assert.That(subMenuItems, Has.None.Matches<MenuItemImpl>(item => item.Name == LocalizationKeys.ContextMenuAddExclusionTag));
            Assert.That(subMenuItems, Has.One.Matches<MenuItemImpl>(item => item.Name == LocalizationKeys.ContextMenuRemoveExclusionTag));
        }

        [Test]
        public void GetGameMenuItems_MultipleGames_MixedExclusionTags_HasAddHdrExclusionMenuItem()
        {
            var games = new List<Game> { _gameWithoutHdrExclusionTag, _gameWithHdrExclusionTag };
            var menuItemsArgs = new GetGameMenuItemsArgs(LocalizationKeys.ContextMenuSectionHeader, games, GameMenuType.GameDetails);

            IEnumerable<MenuItemImpl>? menuItems = _plugin.GetGameMenuItems(menuItemsArgs);

            Assert.That(menuItems, Has.One.Matches<MenuItemImpl>(item => item.Name == LocalizationKeys.ContextMenuSectionHeader));

            IEnumerable<MenuItemImpl>? subMenuItems = menuItems.First().ChildrenAction?.Invoke(new MenuItemImpl.GetChildrenArgs());

            Assert.That(subMenuItems, Has.Exactly(2).Items);
            Assert.That(subMenuItems, Has.One.Matches<MenuItemImpl>(item => item.Name == LocalizationKeys.ContextMenuAddExclusionTag));
            Assert.That(subMenuItems, Has.None.Matches<MenuItemImpl>(item => item.Name == LocalizationKeys.ContextMenuRemoveExclusionTag));
        }

        [Test]
        public void GetGameMenuItems_SelectedSingleGameWithSystemHdrDisabled_HasEnableSystemHdrMenuItem()
        {
            var games = new List<Game> { _gameWithSystemHdrDisabled };
            var menuItemsArgs = new GetGameMenuItemsArgs(LocalizationKeys.ContextMenuSectionHeader, games, GameMenuType.GameDetails);

            IEnumerable<MenuItemImpl>? menuItems = _plugin.GetGameMenuItems(menuItemsArgs);

            Assert.That(menuItems, Has.One.Matches<MenuItemImpl>(item => item.Name == LocalizationKeys.ContextMenuSectionHeader));

            IEnumerable<MenuItemImpl>? subMenuItems = menuItems.First().ChildrenAction?.Invoke(new MenuItemImpl.GetChildrenArgs());

            Assert.That(subMenuItems, Has.Exactly(2).Items);
            Assert.That(subMenuItems, Has.One.Matches<MenuItemImpl>(item => item.Name == LocalizationKeys.ContextMenuEnableHdrSupport));
            Assert.That(subMenuItems, Has.None.Matches<MenuItemImpl>(item => item.Name == LocalizationKeys.ContextMenuDisableHdrSupport));
        }

        [Test]
        public void GetGameMenuItems_SelectedSingleGameWithSystemHdrEnabled_HasDisableSystemHdrMenuItem()
        {
            var games = new List<Game> { _gameWithSystemHdrEnabled };
            var menuItemsArgs = new GetGameMenuItemsArgs(LocalizationKeys.ContextMenuSectionHeader, games, GameMenuType.GameDetails);

            IEnumerable<MenuItemImpl>? menuItems = _plugin.GetGameMenuItems(menuItemsArgs);

            Assert.That(menuItems, Has.One.Matches<MenuItemImpl>(item => item.Name == LocalizationKeys.ContextMenuSectionHeader));

            IEnumerable<MenuItemImpl>? subMenuItems = menuItems.First().ChildrenAction?.Invoke(new MenuItemImpl.GetChildrenArgs());

            Assert.That(subMenuItems, Has.Exactly(2).Items);
            Assert.That(subMenuItems, Has.None.Matches<MenuItemImpl>(item => item.Name == LocalizationKeys.ContextMenuEnableHdrSupport));
            Assert.That(subMenuItems, Has.One.Matches<MenuItemImpl>(item => item.Name == LocalizationKeys.ContextMenuDisableHdrSupport));
        }

        [Test]
        public void GetGameMenuItems_MultipleGames_MixedSystemHdrStates_HasEnableSystemHdrMenuItem()
        {
            var games = new List<Game> { _gameWithSystemHdrDisabled, _gameWithSystemHdrEnabled };
            var menuItemsArgs = new GetGameMenuItemsArgs(LocalizationKeys.ContextMenuSectionHeader, games, GameMenuType.GameDetails);

            IEnumerable<MenuItemImpl>? menuItems = _plugin.GetGameMenuItems(menuItemsArgs);

            Assert.That(menuItems, Has.One.Matches<MenuItemImpl>(item => item.Name == LocalizationKeys.ContextMenuSectionHeader));

            IEnumerable<MenuItemImpl>? subMenuItems = menuItems.First().ChildrenAction?.Invoke(new MenuItemImpl.GetChildrenArgs());

            Assert.That(subMenuItems, Has.Exactly(2).Items);
            Assert.That(subMenuItems, Has.One.Matches<MenuItemImpl>(item => item.Name == LocalizationKeys.ContextMenuEnableHdrSupport));
            Assert.That(subMenuItems, Has.None.Matches<MenuItemImpl>(item => item.Name == LocalizationKeys.ContextMenuDisableHdrSupport));
        }

        [Test]
        public async Task OnApplicationStarted_PcGamingWikiNotInstalled_WarningDialogIsShown()
        {
            var mockPlugin = new Mock<IAddonsApi.IPluginInfo>();
            mockPlugin
                .SetupGet(mock => mock.Id)
                .Returns("Mock Plugin");

            _mockAddons
                .SetupGet(mock => mock.Plugins)
                .Returns([mockPlugin.Object]);

            var applicationStartupArgs = new Plugin.OnApplicationStartupArgs();

            await _plugin.OnApplicationStartupAsync(applicationStartupArgs);

            _mockDialogs.Verify(
               mock => mock.ShowMessageAsync(
                   LocalizationKeys.PCGamingWikiDialogWarningMessage,
                   It.IsAny<string>(),
                   MessageBoxSeverity.Warning,
                   It.IsAny<List<MessageBoxResponse>>(),
                   It.IsAny<List<MessageBoxOption>>()),
               Times.Once);
        }

        [Test]
        public async Task OnApplicationStarted_PcGamingWikiNotInstalled_WarningSupressed_WarningDialogIsNotShown()
        {
            _mockPluginSettings
                .SetupGet(mock => mock.IsPCGamingWikiWarningSuppressed)
                .Returns(true);

            var mockPlugin = new Mock<IAddonsApi.IPluginInfo>();
            mockPlugin
                .SetupGet(mock => mock.Id)
                .Returns("Mock Plugin");

            _mockAddons
                .SetupGet(mock => mock.Plugins)
                .Returns([mockPlugin.Object]);

            var applicationStartupArgs = new Plugin.OnApplicationStartupArgs();

            await _plugin.OnApplicationStartupAsync(applicationStartupArgs);

            _mockDialogs.Verify(
                mock => mock.ShowMessageAsync(
                    LocalizationKeys.PCGamingWikiDialogWarningMessage,
                    It.IsAny<string>(),
                    MessageBoxSeverity.Warning,
                    It.IsAny<List<MessageBoxResponse>>(),
                    It.IsAny<List<MessageBoxOption>>()),
                Times.Never);
        }

        [Test]
        public async Task OnApplicationStarted_PcGamingWikiNotInstalledDialog_SuppressWarningClicked_SettingIsSaved()
        {
            var mockPlugin = new Mock<IAddonsApi.IPluginInfo>();
            mockPlugin
                .SetupGet(mock => mock.Id)
                .Returns("Mock Plugin");

            _mockAddons
                .SetupGet(mock => mock.Plugins)
                .Returns([mockPlugin.Object]);

            _mockDialogs
                .Setup(
                    mock => mock.ShowMessageAsync(
                        LocalizationKeys.PCGamingWikiDialogWarningMessage,
                        It.IsAny<string>(),
                        MessageBoxSeverity.Warning,
                        It.IsAny<List<MessageBoxResponse>>(),
                        It.IsAny<List<MessageBoxOption>>()))
                .ReturnsAsync((string _, string _, MessageBoxSeverity _, List<MessageBoxResponse> responses, List<MessageBoxOption> _) =>
                {
                    return responses.FirstOrDefault(o => o.Title == LocalizationKeys.DialogResponseSuppressWarning);
                });

            var applicationStartupArgs = new Plugin.OnApplicationStartupArgs();

            await _plugin.OnApplicationStartupAsync(applicationStartupArgs);

            Assert.That(_plugin.Settings, Is.Not.Null);
            Assert.That(_plugin.Settings.IsPCGamingWikiWarningSuppressed, Is.True);
        }

        [Test]
        public async Task OnApplicationStarted_PcGamingWikiNotInstalledDialog_OKClicked_SettingIsNotSaved()
        {
            var mockPlugin = new Mock<IAddonsApi.IPluginInfo>();
            mockPlugin
                .SetupGet(mock => mock.Id)
                .Returns("Mock Plugin");

            _mockAddons
                .SetupGet(mock => mock.Plugins)
                .Returns([mockPlugin.Object]);

            _mockDialogs
                .Setup(
                    mock => mock.ShowMessageAsync(
                        LocalizationKeys.PCGamingWikiDialogWarningMessage,
                        It.IsAny<string>(),
                        MessageBoxSeverity.Warning,
                        It.IsAny<List<MessageBoxResponse>>(),
                        It.IsAny<List<MessageBoxOption>>()))
                .ReturnsAsync((string _, string _, MessageBoxSeverity _, List<MessageBoxResponse> responses, List<MessageBoxOption> _) =>
                {
                    return responses.FirstOrDefault(o => o.Title == LocalizationKeys.DialogResponseOk);
                });

            var applicationStartupArgs = new Plugin.OnApplicationStartupArgs();

            await _plugin.OnApplicationStartupAsync(applicationStartupArgs);

            Assert.That(_plugin.Settings, Is.Not.Null);
            Assert.That(_plugin.Settings.IsPCGamingWikiWarningSuppressed, Is.False);
        }

        [Test]
        public async Task OnApplicationStarted_PcGamingWikiInstalled_WarningDialogIsNotShown()
        {
            var mockPlugin = new Mock<IAddonsApi.IPluginInfo>();
            mockPlugin
                .SetupGet(mock => mock.Id)
                .Returns(Plugin.PCGamingWikiPluginId);

            _mockAddons
                .SetupGet(mock => mock.Plugins)
                .Returns([mockPlugin.Object]);

            var applicationStartupArgs = new Plugin.OnApplicationStartupArgs();

            await _plugin.OnApplicationStartupAsync(applicationStartupArgs);

            _mockDialogs.Verify(
                mock => mock.ShowMessageAsync(
                    LocalizationKeys.PCGamingWikiDialogWarningMessage,
                    It.IsAny<string>(),
                    MessageBoxSeverity.Warning,
                    It.IsAny<List<MessageBoxResponse>>(),
                    It.IsAny<List<MessageBoxOption>>()),
                Times.Never);
        }
    }
}
