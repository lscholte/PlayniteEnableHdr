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
        private Mock<IResourceProvider> mockResourceProvider;
        private Mock<IDialogsFactory> mockDialogsFactory;
        private Mock<IAddons> mockAddons;
        private Mock<IPlayniteAPI> mockPlayniteApi;

        private Mock<IPluginSettings> mockPluginSettings;
        private Mock<ISystemHdrManager> mockSystemHdrManager;

        private Game gameWithHdrExclusionTag;
        private Game gameWithoutHdrExclusionTag;

        private Game gameWithSystemHdrEnabled;
        private Game gameWithSystemHdrDisabled;

        private Plugin plugin;

        [SetUp]
        public void SetUp()
        {
            mockResourceProvider = new Mock<IResourceProvider>();
            mockResourceProvider
                .Setup(mock => mock.GetString(It.IsAny<string>()))
                .Returns((string key) => key);

            mockDialogsFactory = new Mock<IDialogsFactory>();

            mockAddons = new Mock<IAddons>();

            mockPlayniteApi = new Mock<IPlayniteAPI>();
            mockPlayniteApi
                .SetupGet(mock => mock.Resources)
                .Returns(mockResourceProvider.Object);
            mockPlayniteApi
                .SetupGet(mock => mock.Dialogs)
                .Returns(mockDialogsFactory.Object);
            mockPlayniteApi
                .SetupGet(mock => mock.Addons)
                .Returns(mockAddons.Object);

            mockPluginSettings = new Mock<IPluginSettings>();
            mockPluginSettings.SetupAllProperties();

            mockSystemHdrManager = new Mock<ISystemHdrManager>();

            gameWithoutHdrExclusionTag = new Game
            {
                TagIds = new List<Guid>()
            };

            gameWithHdrExclusionTag = new Game
            {
                TagIds = new List<Guid>()
                {
                    SystemHdrManager.HdrExclusionTagId
                }
            };

            gameWithSystemHdrEnabled = new Game
            {
                EnableSystemHdr = true
            };

            gameWithSystemHdrDisabled = new Game
            {
                EnableSystemHdr = false
            };

            plugin = new Plugin(mockPlayniteApi.Object, mockPluginSettings.Object, mockSystemHdrManager.Object);
        }

        [TearDown]
        public void TearDown()
        {
            plugin.Dispose();
        }

            [Test]
        public void GetGameMenuItems_SelectedSingleGameWithoutHdrExclusionTag_HasAddHdrExclusionMenuItem()
        {
            var games = new List<Game> { gameWithoutHdrExclusionTag };

            var menuItemsArgs = new GetGameMenuItemsArgs
            {
                Games = games
            };

            IEnumerable<GameMenuItem> menuItems = plugin.GetGameMenuItems(menuItemsArgs);

            Assert.That(menuItems, Has.One.Matches<GameMenuItem>(item => item.Description == LocalizationKeys.ContextMenuAddExclusionTag));
            Assert.That(menuItems, Has.None.Matches<GameMenuItem>(item => item.Description == LocalizationKeys.ContextMenuRemoveExclusionTag));
        }

        [Test]
        public void GetGameMenuItems_SelectedSingleGameWithHdrExclusionTag_HasRemoveHdrExclusionMenuItem()
        {
            var games = new List<Game> { gameWithHdrExclusionTag };

            var menuItemsArgs = new GetGameMenuItemsArgs
            {
                Games = games
            };

            IEnumerable<GameMenuItem> menuItems = plugin.GetGameMenuItems(menuItemsArgs);

            Assert.That(menuItems, Has.None.Matches<GameMenuItem>(item => item.Description == LocalizationKeys.ContextMenuAddExclusionTag));
            Assert.That(menuItems, Has.One.Matches<GameMenuItem>(item => item.Description == LocalizationKeys.ContextMenuRemoveExclusionTag));
        }

        [Test]
        public void GetGameMenuItems_MultipleGames_MixedExclusionTags_HasAddHdrExclusionMenuItem()
        {
            var games = new List<Game> { gameWithoutHdrExclusionTag, gameWithHdrExclusionTag };

            var menuItemsArgs = new GetGameMenuItemsArgs
            {
                Games = games
            };

            IEnumerable<GameMenuItem> menuItems = plugin.GetGameMenuItems(menuItemsArgs);

            Assert.That(menuItems, Has.One.Matches<GameMenuItem>(item => item.Description == LocalizationKeys.ContextMenuAddExclusionTag));
            Assert.That(menuItems, Has.None.Matches<GameMenuItem>(item => item.Description == LocalizationKeys.ContextMenuRemoveExclusionTag));         
        }

        [Test]
        public void GetGameMenuItems_SelectedSingleGameWithSystemHdrDisabled_HasEnableSystemHdrMenuItem()
        {
            var games = new List<Game> { gameWithSystemHdrDisabled };

            var menuItemsArgs = new GetGameMenuItemsArgs
            {
                Games = games
            };

            IEnumerable<GameMenuItem> menuItems = plugin.GetGameMenuItems(menuItemsArgs);

            Assert.That(menuItems, Has.One.Matches<GameMenuItem>(item => item.Description == LocalizationKeys.ContextMenuEnableHdrSupport));
            Assert.That(menuItems, Has.None.Matches<GameMenuItem>(item => item.Description == LocalizationKeys.ContextMenuDisableHdrSupport));
        }

        [Test]
        public void GetGameMenuItems_SelectedSingleGameWithSystemHdrEnabled_HasDisableSystemHdrMenuItem()
        {
            var games = new List<Game> { gameWithSystemHdrEnabled };

            var menuItemsArgs = new GetGameMenuItemsArgs
            {
                Games = games
            };

            IEnumerable<GameMenuItem> menuItems = plugin.GetGameMenuItems(menuItemsArgs);

            Assert.That(menuItems, Has.None.Matches<GameMenuItem>(item => item.Description == LocalizationKeys.ContextMenuEnableHdrSupport));
            Assert.That(menuItems, Has.One.Matches<GameMenuItem>(item => item.Description == LocalizationKeys.ContextMenuDisableHdrSupport));
        }

        [Test]
        public void GetGameMenuItems_MultipleGames_MixedSystemHdrStates_HasEnableSystemHdrMenuItem()
        {
            var games = new List<Game> { gameWithSystemHdrDisabled, gameWithSystemHdrEnabled };

            var menuItemsArgs = new GetGameMenuItemsArgs
            {
                Games = games
            };

            IEnumerable<GameMenuItem> menuItems = plugin.GetGameMenuItems(menuItemsArgs);

            Assert.That(menuItems, Has.One.Matches<GameMenuItem>(item => item.Description == LocalizationKeys.ContextMenuEnableHdrSupport));
            Assert.That(menuItems, Has.None.Matches<GameMenuItem>(item => item.Description == LocalizationKeys.ContextMenuDisableHdrSupport));
        }

        [Test]
        public void OnApplicationStarted_PcGamingWikiNotInstalled_WarningDialogIsShown()
        {
            var mockPlugin = new Mock<Playnite.SDK.Plugins.Plugin>(mockPlayniteApi.Object);
            mockPlugin
                .SetupGet(mock => mock.Id)
                .Returns(Guid.NewGuid());

            mockAddons
                .SetupGet(mock => mock.Plugins)
                .Returns([mockPlugin.Object]);
            var applicationStartedArgs = new OnApplicationStartedEventArgs();

            plugin.OnApplicationStarted(applicationStartedArgs);

            mockDialogsFactory.Verify(
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
            mockPluginSettings
                .SetupGet(mock => mock.IsPCGamingWikiWarningSuppressed)
                .Returns(true);

            var mockPlugin = new Mock<Playnite.SDK.Plugins.Plugin>(mockPlayniteApi.Object);
            mockPlugin
                .SetupGet(mock => mock.Id)
                .Returns(Guid.NewGuid());

            mockAddons
                .SetupGet(mock => mock.Plugins)
                .Returns([mockPlugin.Object]);
            var applicationStartedArgs = new OnApplicationStartedEventArgs();

            plugin.OnApplicationStarted(applicationStartedArgs);

            mockDialogsFactory.Verify(
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
            var mockPlugin = new Mock<Playnite.SDK.Plugins.Plugin>(mockPlayniteApi.Object);
            mockPlugin
                .SetupGet(mock => mock.Id)
                .Returns(Guid.NewGuid());

            mockAddons
                .SetupGet(mock => mock.Plugins)
                .Returns([mockPlugin.Object]);

            mockDialogsFactory
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
            plugin.OnApplicationStarted(applicationStartedArgs);

            var settings = plugin.GetSettings(false) as IPluginSettings;

            Assert.That(settings, Is.Not.Null);
            Assert.That(settings.IsPCGamingWikiWarningSuppressed, Is.True);
        }

        [Test]
        public void OnApplicationStarted_PcGamingWikiNotInstalledDialog_OKClicked_SettingIsNotSaved()
        {
            var mockPlugin = new Mock<Playnite.SDK.Plugins.Plugin>(mockPlayniteApi.Object);
            mockPlugin
                .SetupGet(mock => mock.Id)
                .Returns(Guid.NewGuid());

            mockAddons
                .SetupGet(mock => mock.Plugins)
                .Returns([mockPlugin.Object]);

            mockDialogsFactory
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
            plugin.OnApplicationStarted(applicationStartedArgs);

            var settings = plugin.GetSettings(false) as IPluginSettings;

            Assert.That(settings, Is.Not.Null);
            Assert.That(settings.IsPCGamingWikiWarningSuppressed, Is.False);
        }

        [Test]
        public void OnApplicationStarted_PcGamingWikiInstalled_WarningDialogIsNotShown()
        {
            var mockPlugin = new Mock<Playnite.SDK.Plugins.Plugin>(mockPlayniteApi.Object);
            mockPlugin
                .SetupGet(mock => mock.Id)
                .Returns(Plugin.PCGamingWikiPluginId);

            mockAddons
                .SetupGet(mock => mock.Plugins)
                .Returns([mockPlugin.Object]);
            var applicationStartedArgs = new OnApplicationStartedEventArgs();

            plugin.OnApplicationStarted(applicationStartedArgs);

            mockDialogsFactory.Verify(
                mock => mock.ShowMessage(
                    LocalizationKeys.PCGamingWikiDialogWarningMessage,
                    It.IsAny<string>(),
                    MessageBoxImage.Warning,
                    It.IsAny<List<MessageBoxOption>>()),
                Times.Never);
        }
    }
}
