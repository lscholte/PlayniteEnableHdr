using HdrManager.Localization.Generated;
using Moq;
using NUnit.Framework;
using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace HdrManager.Test
{
    [TestFixture]
    public class PluginTest
    {
        private Mock<IResourceProvider> _mockResourceProvider;
        private Mock<IDialogsFactory> _mockDialogsFactory;
        private Mock<IAddons> _mockAddons;
        private Mock<IPlayniteAPI> _mockPlayniteApi;

        private Mock<IPluginSettings> _mockPluginSettings;
        private Mock<ISystemHdrManager> _mockSystemHdrManager;

        private Game _gameWithHdrExclusionTag;
        private Game _gameWithoutHdrExclusionTag;

        private Game _gameWithSystemHdrEnabled;
        private Game _gameWithSystemHdrDisabled;

        private Plugin _plugin;

        [SetUp]
        public void SetUp()
        {
            _mockResourceProvider = new Mock<IResourceProvider>();
            _mockResourceProvider
                .Setup(mock => mock.GetString(It.IsAny<string>()))
                .Returns((string key) => key);

            _mockDialogsFactory = new Mock<IDialogsFactory>();

            _mockAddons = new Mock<IAddons>();

            _mockPlayniteApi = new Mock<IPlayniteAPI>();
            _mockPlayniteApi
                .SetupGet(mock => mock.Resources)
                .Returns(_mockResourceProvider.Object);
            _mockPlayniteApi
                .SetupGet(mock => mock.Dialogs)
                .Returns(_mockDialogsFactory.Object);
            _mockPlayniteApi
                .SetupGet(mock => mock.Addons)
                .Returns(_mockAddons.Object);

            _mockPluginSettings = new Mock<IPluginSettings>();
            _mockPluginSettings.SetupAllProperties();

            _mockSystemHdrManager = new Mock<ISystemHdrManager>();

            _gameWithoutHdrExclusionTag = new Game
            {
                TagIds = new List<Guid>()
            };

            _gameWithHdrExclusionTag = new Game
            {
                TagIds = new List<Guid>()
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

            _plugin = new Plugin(_mockPlayniteApi.Object, _mockPluginSettings.Object, _mockSystemHdrManager.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _plugin.Dispose();
        }

        [Test]
        public void GetGameMenuItems_SelectedSingleGameWithoutHdrExclusionTag_HasAddHdrExclusionMenuItem()
        {
            var games = new List<Game> { _gameWithoutHdrExclusionTag };

            var menuItemsArgs = new GetGameMenuItemsArgs
            {
                Games = games
            };

            IEnumerable<GameMenuItem> menuItems = _plugin.GetGameMenuItems(menuItemsArgs);

            Assert.That(menuItems, Has.One.Matches<GameMenuItem>(item => item.Description == LocalizationKeys.ContextMenuAddExclusionTag));
            Assert.That(menuItems, Has.None.Matches<GameMenuItem>(item => item.Description == LocalizationKeys.ContextMenuRemoveExclusionTag));
        }

        [Test]
        public void GetGameMenuItems_SelectedSingleGameWithHdrExclusionTag_HasRemoveHdrExclusionMenuItem()
        {
            var games = new List<Game> { _gameWithHdrExclusionTag };

            var menuItemsArgs = new GetGameMenuItemsArgs
            {
                Games = games
            };

            IEnumerable<GameMenuItem> menuItems = _plugin.GetGameMenuItems(menuItemsArgs);

            Assert.That(menuItems, Has.None.Matches<GameMenuItem>(item => item.Description == LocalizationKeys.ContextMenuAddExclusionTag));
            Assert.That(menuItems, Has.One.Matches<GameMenuItem>(item => item.Description == LocalizationKeys.ContextMenuRemoveExclusionTag));
        }

        [Test]
        public void GetGameMenuItems_MultipleGames_MixedExclusionTags_HasAddHdrExclusionMenuItem()
        {
            var games = new List<Game> { _gameWithoutHdrExclusionTag, _gameWithHdrExclusionTag };

            var menuItemsArgs = new GetGameMenuItemsArgs
            {
                Games = games
            };

            IEnumerable<GameMenuItem> menuItems = _plugin.GetGameMenuItems(menuItemsArgs);

            Assert.That(menuItems, Has.One.Matches<GameMenuItem>(item => item.Description == LocalizationKeys.ContextMenuAddExclusionTag));
            Assert.That(menuItems, Has.None.Matches<GameMenuItem>(item => item.Description == LocalizationKeys.ContextMenuRemoveExclusionTag));
        }

        [Test]
        public void GetGameMenuItems_SelectedSingleGameWithSystemHdrDisabled_HasEnableSystemHdrMenuItem()
        {
            var games = new List<Game> { _gameWithSystemHdrDisabled };

            var menuItemsArgs = new GetGameMenuItemsArgs
            {
                Games = games
            };

            IEnumerable<GameMenuItem> menuItems = _plugin.GetGameMenuItems(menuItemsArgs);

            Assert.That(menuItems, Has.One.Matches<GameMenuItem>(item => item.Description == LocalizationKeys.ContextMenuEnableHdrSupport));
            Assert.That(menuItems, Has.None.Matches<GameMenuItem>(item => item.Description == LocalizationKeys.ContextMenuDisableHdrSupport));
        }

        [Test]
        public void GetGameMenuItems_SelectedSingleGameWithSystemHdrEnabled_HasDisableSystemHdrMenuItem()
        {
            var games = new List<Game> { _gameWithSystemHdrEnabled };

            var menuItemsArgs = new GetGameMenuItemsArgs
            {
                Games = games
            };

            IEnumerable<GameMenuItem> menuItems = _plugin.GetGameMenuItems(menuItemsArgs);

            Assert.That(menuItems, Has.None.Matches<GameMenuItem>(item => item.Description == LocalizationKeys.ContextMenuEnableHdrSupport));
            Assert.That(menuItems, Has.One.Matches<GameMenuItem>(item => item.Description == LocalizationKeys.ContextMenuDisableHdrSupport));
        }

        [Test]
        public void GetGameMenuItems_MultipleGames_MixedSystemHdrStates_HasEnableSystemHdrMenuItem()
        {
            var games = new List<Game> { _gameWithSystemHdrDisabled, _gameWithSystemHdrEnabled };

            var menuItemsArgs = new GetGameMenuItemsArgs
            {
                Games = games
            };

            IEnumerable<GameMenuItem> menuItems = _plugin.GetGameMenuItems(menuItemsArgs);

            Assert.That(menuItems, Has.One.Matches<GameMenuItem>(item => item.Description == LocalizationKeys.ContextMenuEnableHdrSupport));
            Assert.That(menuItems, Has.None.Matches<GameMenuItem>(item => item.Description == LocalizationKeys.ContextMenuDisableHdrSupport));
        }

        [Test]
        public void OnApplicationStarted_PcGamingWikiNotInstalled_WarningDialogIsShown()
        {
            var mockPlugin = new Mock<Playnite.SDK.Plugins.Plugin>(_mockPlayniteApi.Object);
            mockPlugin
                .SetupGet(mock => mock.Id)
                .Returns(Guid.NewGuid());

            _mockAddons
                .SetupGet(mock => mock.Plugins)
                .Returns([mockPlugin.Object]);
            var applicationStartedArgs = new OnApplicationStartedEventArgs();

            _plugin.OnApplicationStarted(applicationStartedArgs);

            _mockDialogsFactory.Verify(
                mock => mock.ShowMessage(
                    LocalizationKeys.PCGamingWikiDialogWarningMessage,
                    It.IsAny<string>(),
                    MessageBoxImage.Warning,
                    It.IsAny<List<MessageBoxOption>>()),
                Times.Once);
        }

        [Test]
        public void OnApplicationStarted_PcGamingWikiNotInstalled_WarningSupressed_WarningDialogIsNotShown()
        {
            _mockPluginSettings
                .SetupGet(mock => mock.IsPCGamingWikiWarningSuppressed)
                .Returns(true);

            var mockPlugin = new Mock<Playnite.SDK.Plugins.Plugin>(_mockPlayniteApi.Object);
            mockPlugin
                .SetupGet(mock => mock.Id)
                .Returns(Guid.NewGuid());

            _mockAddons
                .SetupGet(mock => mock.Plugins)
                .Returns([mockPlugin.Object]);
            var applicationStartedArgs = new OnApplicationStartedEventArgs();

            _plugin.OnApplicationStarted(applicationStartedArgs);

            _mockDialogsFactory.Verify(
                mock => mock.ShowMessage(
                    LocalizationKeys.PCGamingWikiDialogWarningMessage,
                    It.IsAny<string>(),
                    MessageBoxImage.Warning,
                    It.IsAny<List<MessageBoxOption>>()),
                Times.Never);
        }

        [Test]
        public void OnApplicationStarted_PcGamingWikiNotInstalledDialog_SuppressWarningClicked_SettingIsSaved()
        {
            var mockPlugin = new Mock<Playnite.SDK.Plugins.Plugin>(_mockPlayniteApi.Object);
            mockPlugin
                .SetupGet(mock => mock.Id)
                .Returns(Guid.NewGuid());

            _mockAddons
                .SetupGet(mock => mock.Plugins)
                .Returns([mockPlugin.Object]);

            _mockDialogsFactory
                .Setup(
                    mock => mock.ShowMessage(
                        LocalizationKeys.PCGamingWikiDialogWarningMessage,
                        It.IsAny<string>(),
                        It.IsAny<MessageBoxImage>(),
                        It.IsAny<List<MessageBoxOption>>()))
                .Returns((string _, string _, MessageBoxImage _, List<MessageBoxOption> options) =>
                {
                    return options.FirstOrDefault(o => o.Title == LocalizationKeys.DialogResponseSuppressWarning);
                });

            var applicationStartedArgs = new OnApplicationStartedEventArgs();
            _plugin.OnApplicationStarted(applicationStartedArgs);

            var settings = _plugin.GetSettings(false) as IPluginSettings;

            Assert.That(settings, Is.Not.Null);
            Assert.That(settings.IsPCGamingWikiWarningSuppressed, Is.True);
        }

        [Test]
        public void OnApplicationStarted_PcGamingWikiNotInstalledDialog_OKClicked_SettingIsNotSaved()
        {
            var mockPlugin = new Mock<Playnite.SDK.Plugins.Plugin>(_mockPlayniteApi.Object);
            mockPlugin
                .SetupGet(mock => mock.Id)
                .Returns(Guid.NewGuid());

            _mockAddons
                .SetupGet(mock => mock.Plugins)
                .Returns([mockPlugin.Object]);

            _mockDialogsFactory
                .Setup(
                    mock => mock.ShowMessage(
                        LocalizationKeys.PCGamingWikiDialogWarningMessage,
                        It.IsAny<string>(),
                        It.IsAny<MessageBoxImage>(),
                        It.IsAny<List<MessageBoxOption>>()))
                .Returns((string _, string _, MessageBoxImage _, List<MessageBoxOption> options) =>
                {
                    return options.FirstOrDefault(o => o.Title == LocalizationKeys.DialogResponseOK);
                });

            var applicationStartedArgs = new OnApplicationStartedEventArgs();
            _plugin.OnApplicationStarted(applicationStartedArgs);

            var settings = _plugin.GetSettings(false) as IPluginSettings;

            Assert.That(settings, Is.Not.Null);
            Assert.That(settings.IsPCGamingWikiWarningSuppressed, Is.False);
        }

        [Test]
        public void OnApplicationStarted_PcGamingWikiInstalled_WarningDialogIsNotShown()
        {
            var mockPlugin = new Mock<Playnite.SDK.Plugins.Plugin>(_mockPlayniteApi.Object);
            mockPlugin
                .SetupGet(mock => mock.Id)
                .Returns(Plugin.PCGamingWikiPluginId);

            _mockAddons
                .SetupGet(mock => mock.Plugins)
                .Returns([mockPlugin.Object]);
            var applicationStartedArgs = new OnApplicationStartedEventArgs();

            _plugin.OnApplicationStarted(applicationStartedArgs);

            _mockDialogsFactory.Verify(
                mock => mock.ShowMessage(
                    LocalizationKeys.PCGamingWikiDialogWarningMessage,
                    It.IsAny<string>(),
                    MessageBoxImage.Warning,
                    It.IsAny<List<MessageBoxOption>>()),
                Times.Never);
        }
    }
}
