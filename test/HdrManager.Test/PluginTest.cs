using Moq;
using NUnit.Framework;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HdrManager.Test
{
    [TestFixture]
    public class PluginTest
    {
        private const string AddHdrExclusionTagString = "Add HDR Exclusion Tag";
        private const string RemoveHdrExclusionTagString = "Remove HDR Exclusion Tag";
        private const string EnableSystemHdrString = "Enable System HDR";
        private const string DisableSystemHdrString = "Disable System HDR";

        private Mock<IPlaynitePathsAPI> mockPlaynitePathsApi;
        private Mock<IResourceProvider> mockResourceProvider;
        private Mock<IGameDatabaseAPI> mockGameDatabaseApi;
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
                .Setup(mock => mock.GetString("ContextMenuAddExclusionTag"))
                .Returns(AddHdrExclusionTagString);
            mockResourceProvider
                .Setup(mock => mock.GetString("ContextMenuRemoveExclusionTag"))
                .Returns(RemoveHdrExclusionTagString);
            mockResourceProvider
                .Setup(mock => mock.GetString("ContextMenuEnableHdrSupport"))
                .Returns(EnableSystemHdrString);
            mockResourceProvider
                .Setup(mock => mock.GetString("ContextMenuDisableHdrSupport"))
                .Returns(DisableSystemHdrString);

            List<Tag> backingTagList = new List<Tag>();
            Mock<IItemCollection<Tag>> mockTagCollection = new Mock<IItemCollection<Tag>>();
            mockTagCollection
                .Setup(mock => mock.GetEnumerator())
                .Returns(backingTagList.GetEnumerator());

            List<Game> backingGameList = new List<Game>();
            Mock<IItemCollection<Game>> mockGameCollection = new Mock<IItemCollection<Game>>();
            mockGameCollection
                .Setup(mock => mock.GetEnumerator())
                .Returns(backingGameList.GetEnumerator());

            mockGameDatabaseApi = new Mock<IGameDatabaseAPI>();
            mockGameDatabaseApi
                .SetupGet(mock => mock.Tags)
                .Returns(mockTagCollection.Object);
            mockGameDatabaseApi
                .SetupGet(mock => mock.Games)
                .Returns(mockGameCollection.Object);

            mockPlayniteApi = new Mock<IPlayniteAPI>();
            mockPlayniteApi
                .SetupGet(mock => mock.Paths)
                .Returns(mockPlaynitePathsApi.Object);
            mockPlayniteApi
                .SetupGet(mock => mock.Resources)
                .Returns(mockResourceProvider.Object);
            mockPlayniteApi
                .SetupGet(mock => mock.Database)
                .Returns(mockGameDatabaseApi.Object);

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

            Assert.That(menuItems, Has.One.Matches<GameMenuItem>(item => item.Description == AddHdrExclusionTagString));
            Assert.That(menuItems, Has.None.Matches<GameMenuItem>(item => item.Description == RemoveHdrExclusionTagString));

            GameMenuItem menuItem = menuItems.First(item => item.Description == AddHdrExclusionTagString);

            var menuItemActionArgs = new GameMenuItemActionArgs
            {
                Games = games
            };

            menuItem.Action(menuItemActionArgs);

            Assert.That(gameWithoutHdrExclusionTag.TagIds, Does.Contain(SystemHdrManager.HdrExclusionTagId));
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

            Assert.That(menuItems, Has.None.Matches<GameMenuItem>(item => item.Description == AddHdrExclusionTagString));
            Assert.That(menuItems, Has.One.Matches<GameMenuItem>(item => item.Description == RemoveHdrExclusionTagString));

            GameMenuItem menuItem = menuItems.First(item => item.Description == RemoveHdrExclusionTagString);

            var menuItemActionArgs = new GameMenuItemActionArgs
            {
                Games = games
            };

            menuItem.Action(menuItemActionArgs);

            Assert.That(gameWithHdrExclusionTag.TagIds, Does.Not.Contain(SystemHdrManager.HdrExclusionTagId));
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

            Assert.That(menuItems, Has.One.Matches<GameMenuItem>(item => item.Description == AddHdrExclusionTagString));
            Assert.That(menuItems, Has.None.Matches<GameMenuItem>(item => item.Description == RemoveHdrExclusionTagString));

            GameMenuItem menuItem = menuItems.First(item => item.Description == AddHdrExclusionTagString);

            var menuItemActionArgs = new GameMenuItemActionArgs
            {
                Games = games
            };

            menuItem.Action(menuItemActionArgs);

            Assert.That(gameWithoutHdrExclusionTag.TagIds, Does.Contain(SystemHdrManager.HdrExclusionTagId));
            Assert.That(gameWithHdrExclusionTag.TagIds, Does.Contain(SystemHdrManager.HdrExclusionTagId));
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

            Assert.That(menuItems, Has.One.Matches<GameMenuItem>(item => item.Description == EnableSystemHdrString));
            Assert.That(menuItems, Has.None.Matches<GameMenuItem>(item => item.Description == DisableSystemHdrString));

            GameMenuItem menuItem = menuItems.First(item => item.Description == EnableSystemHdrString);

            var menuItemActionArgs = new GameMenuItemActionArgs
            {
                Games = games
            };

            menuItem.Action(menuItemActionArgs);

            Assert.That(gameWithSystemHdrDisabled.EnableSystemHdr, Is.True);
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

            Assert.That(menuItems, Has.None.Matches<GameMenuItem>(item => item.Description == EnableSystemHdrString));
            Assert.That(menuItems, Has.One.Matches<GameMenuItem>(item => item.Description == DisableSystemHdrString));

            GameMenuItem menuItem = menuItems.First(item => item.Description == DisableSystemHdrString);

            var menuItemActionArgs = new GameMenuItemActionArgs
            {
                Games = games
            };

            menuItem.Action(menuItemActionArgs);

            Assert.That(gameWithSystemHdrEnabled.EnableSystemHdr, Is.False);
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

            Assert.That(menuItems, Has.One.Matches<GameMenuItem>(item => item.Description == EnableSystemHdrString));
            Assert.That(menuItems, Has.None.Matches<GameMenuItem>(item => item.Description == DisableSystemHdrString));

            GameMenuItem menuItem = menuItems.First(item => item.Description == EnableSystemHdrString);

            var menuItemActionArgs = new GameMenuItemActionArgs
            {
                Games = games
            };

            menuItem.Action(menuItemActionArgs);

            Assert.That(gameWithSystemHdrDisabled.EnableSystemHdr, Is.True);
            Assert.That(gameWithSystemHdrEnabled.EnableSystemHdr, Is.True);
        }
    }
}
