using HdrManager.Localization.Generated;
using Moq;
using NUnit.Framework;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;

namespace HdrManager.Test
{
    [TestFixture]
    public class PluginTest
    {
        private Mock<IPlaynitePathsAPI> mockPlaynitePathsApi;
        private Mock<IResourceProvider> mockResourceProvider;
        private Mock<IPlayniteAPI> mockPlayniteApi;

        private Game gameWithHdrExclusionTag;
        private Game gameWithoutHdrExclusionTag;

        private Game gameWithSystemHdrEnabled;
        private Game gameWithSystemHdrDisabled;

        private Plugin plugin;

        [SetUp]
        public void SetUp()
        {
            mockPlaynitePathsApi = new Mock<IPlaynitePathsAPI>();
            mockPlaynitePathsApi
                .SetupGet(mock => mock.ExtensionsDataPath)
                .Returns("");

            mockResourceProvider = new Mock<IResourceProvider>();
            mockResourceProvider
                .Setup(mock => mock.GetString(It.IsAny<string>()))
                .Returns((string key) => key);

            mockPlayniteApi = new Mock<IPlayniteAPI>();
            mockPlayniteApi
                .SetupGet(mock => mock.Paths)
                .Returns(mockPlaynitePathsApi.Object);
            mockPlayniteApi
                .SetupGet(mock => mock.Resources)
                .Returns(mockResourceProvider.Object);

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

            plugin = new Plugin(mockPlayniteApi.Object);
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
    }
}
