using HdrManager.Test.Helpers;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Internal;
using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;

namespace HdrManager.Test
{
    [TestFixture]
    public class SystemHdrManagerTest
    {
        private List<Tag> backingTagList;
        private List<Game> backingGameList;
        private List<GameFeature> backingFeatureList;

        private Mock<IItemCollection<Tag>> mockTagCollection;
        private Mock<IItemCollection<Game>> mockGameCollection;
        private Mock<IItemCollection<GameFeature>> mockFeatureCollection;

        private Mock<IGameDatabaseAPI> mockGameDatabaseApi;
        private Mock<IPlayniteAPI> mockPlayniteApi;

        private SystemHdrManager systemHdrManager;

        [SetUp]
        public void SetUp()
        {
            backingTagList = new List<Tag>();
            mockTagCollection = new Mock<IItemCollection<Tag>>();
            mockTagCollection
                .Setup(mock => mock.GetEnumerator())
                .Returns(() => backingTagList.GetEnumerator());

            backingGameList = new List<Game>();
            mockGameCollection = new Mock<IItemCollection<Game>>();
            mockGameCollection
                .Setup(mock => mock.GetEnumerator())
                .Returns(() => backingGameList.GetEnumerator());

            backingFeatureList = new List<GameFeature>();
            mockFeatureCollection = new Mock<IItemCollection<GameFeature>>();
            mockFeatureCollection
                .Setup(mock => mock.GetEnumerator())
                .Returns(() => backingFeatureList.GetEnumerator());

            mockGameDatabaseApi = new Mock<IGameDatabaseAPI>();
            mockGameDatabaseApi
                .SetupGet(mock => mock.Tags)
                .Returns(mockTagCollection.Object);
            mockGameDatabaseApi
                .SetupGet(mock => mock.Games)
                .Returns(mockGameCollection.Object);
            mockGameDatabaseApi
                .SetupGet(mock => mock.Features)
                .Returns(mockFeatureCollection.Object);

            mockPlayniteApi = new Mock<IPlayniteAPI>();
            mockPlayniteApi
                .SetupGet(mock => mock.Database)
                .Returns(mockGameDatabaseApi.Object);

            systemHdrManager = new SystemHdrManager(mockPlayniteApi.Object);
        }

        [TestCase("HDR")]
        [TestCase("HDR Available")]
        public void EnableSystemHdrForManagedGames_GameWithHdrFeature_ShouldHaveEnableSystemHdrTrue(string featureName)
        {
            GameFeature feature = new GameFeature(featureName)
            {
                Id = Guid.NewGuid()
            };

            Game game = new GameBuilder().WithEnableSystemHdr(false).WithFeatureIds(feature.Id).Build();

            backingGameList.Add(game);
            backingFeatureList.Add(feature);

            var libraryUpdatedArgs = new OnLibraryUpdatedEventArgs();

            systemHdrManager.EnableSystemHdrForManagedGames();

            Assert.That(game.EnableSystemHdr, Is.True);
        }

        [TestCase("Miscellaneous")]
        [TestCase("No HDR")]
        public void EnableSystemHdrForManagedGames_GameWithoutHdrFeature_ShouldHaveEnableSystemHdrFalse(string featureName)
        {
            GameFeature feature = new GameFeature(featureName)
            {
                Id = Guid.NewGuid()
            };

            Game game = new GameBuilder().WithEnableSystemHdr(false).WithFeatureIds(feature.Id).Build();

            backingGameList.Add(game);
            backingFeatureList.Add(feature);

            var libraryUpdatedArgs = new OnLibraryUpdatedEventArgs();

            systemHdrManager.EnableSystemHdrForManagedGames();

            Assert.That(game.EnableSystemHdr, Is.False);
        }

        [TestCase("Miscellaneous")]
        [TestCase("No HDR")]
        public void EnableSystemHdrForManagedGames_GameWithoutHdrFeature_AlreadyHasEnableSystemHdrTrue_ShouldHaveEnableSystemHdrTrue(string featureName)
        {
            GameFeature feature = new GameFeature(featureName)
            {
                Id = Guid.NewGuid()
            };

            Game game = new GameBuilder().WithEnableSystemHdr(true).WithFeatureIds(feature.Id).Build();

            backingGameList.Add(game);
            backingFeatureList.Add(feature);

            var libraryUpdatedArgs = new OnLibraryUpdatedEventArgs();

            systemHdrManager.EnableSystemHdrForManagedGames();

            Assert.That(game.EnableSystemHdr, Is.True);
        }

        [Test]
        public void EnableSystemHdrForManagedGames_ListOfMixedGames_ShouldHaveEnableSystemHdrTrueOnlyForHdrGames()
        {
            GameFeature hdrFeature = new GameFeature("HDR")
            {
                Id = Guid.NewGuid()
            };
            GameFeature nonHdrFeature = new GameFeature("Miscellaneous")
            {
                Id = Guid.NewGuid()
            };

            var hdrGames = new List<Game>
            {
                new GameBuilder().WithName("A").WithEnableSystemHdr(false).WithFeatureIds(hdrFeature.Id).Build(),
                new GameBuilder().WithName("B").WithEnableSystemHdr(false).WithFeatureIds(hdrFeature.Id).Build(),
                new GameBuilder().WithName("C").WithEnableSystemHdr(false).WithFeatureIds(hdrFeature.Id).Build(),
            };
            var nonHdrGames = new List<Game>
            {
                new GameBuilder().WithName("D").WithEnableSystemHdr(false).WithFeatureIds(nonHdrFeature.Id).Build(),
                new GameBuilder().WithName("E").WithEnableSystemHdr(false).WithFeatureIds(nonHdrFeature.Id).Build(),
                new GameBuilder().WithName("F").WithEnableSystemHdr(false).WithFeatureIds(nonHdrFeature.Id).Build(),
            };

            backingGameList.AddRange(hdrGames);
            backingGameList.AddRange(nonHdrGames);

            backingFeatureList.Add(hdrFeature);
            backingFeatureList.Add(nonHdrFeature);

            var libraryUpdatedArgs = new OnLibraryUpdatedEventArgs();

            systemHdrManager.EnableSystemHdrForManagedGames();

            Assert.Multiple(() =>
            {
                foreach (Game game in hdrGames)
                {
                    Assert.That(game.EnableSystemHdr, Is.True, $"Game {game.Name} has EnableSystemHdr set to false but expected true");
                }
                foreach (Game game in nonHdrGames)
                {
                    Assert.That(game.EnableSystemHdr, Is.False, $"Game {game.Name} has EnableSystemHdr set to false but expected true");
                }
            });
        }

        [Test]
        public void EnableSystemHdrForManagedGames_GameWithHdrFeature_AndMiscellaneousTag_ShouldHaveEnableSystemHdrTrue()
        {
            GameFeature feature = new GameFeature("HDR")
            {
                Id = Guid.NewGuid()
            };

            Game game = new GameBuilder().WithEnableSystemHdr(false).WithFeatureIds(feature.Id).WithTagIds(Guid.NewGuid()).Build();

            backingGameList.Add(game);
            backingFeatureList.Add(feature);

            var libraryUpdatedArgs = new OnLibraryUpdatedEventArgs();

            systemHdrManager.EnableSystemHdrForManagedGames();

            Assert.That(game.EnableSystemHdr, Is.True);
        }

        [Test]
        public void EnableSystemHdrForManagedGames_GameWithHdrFeature_AndHdrExclusionTag_ShouldHaveEnableSystemHdrFalse()
        {
            GameFeature feature = new GameFeature("HDR")
            {
                Id = Guid.NewGuid()
            };

            Game game = new GameBuilder().WithEnableSystemHdr(false).WithFeatureIds(feature.Id).WithTagIds(SystemHdrManager.HdrExclusionTagId).Build();

            backingGameList.Add(game);
            backingFeatureList.Add(feature);

            var libraryUpdatedArgs = new OnLibraryUpdatedEventArgs();

            systemHdrManager.EnableSystemHdrForManagedGames();

            Assert.That(game.EnableSystemHdr, Is.False);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void SetSystemHdrForGames_NoGames_DoesNotUpdateGameDatabase(bool enableSystemHdr)
        {
            var games = new List<Game>();

            systemHdrManager.SetSystemHdrForGames(games, enableSystemHdr);

            mockGameCollection.Verify(mock => mock.Update(It.IsAny<Game>()), Times.Never);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void SetSystemHdrForGames_SetsEnableSystemHdrOnEachGame(bool enableSystemHdr)
        {
            var games = new List<Game>
            {
                new GameBuilder().WithName("A").WithEnableSystemHdr(enableSystemHdr).Build(),
                new GameBuilder().WithName("B").WithEnableSystemHdr(!enableSystemHdr).Build(),
            };

            systemHdrManager.SetSystemHdrForGames(games, enableSystemHdr);

            Assert.Multiple(() =>
            {
                foreach (Game game in games)
                {
                    Assert.That(game.EnableSystemHdr, Is.EqualTo(enableSystemHdr), $"Game {game.Name} has EnableSystemHdr set to {!enableSystemHdr} but expected {enableSystemHdr}");
                }
                mockGameCollection.Verify(mock => mock.Update(It.IsAny<Game>()), Times.Exactly(games.Count));
            });
        }

        [TestCase(true)]
        [TestCase(false)]
        public void AddHdrExclusionTagToGames_NoGames_DoesNotUpdateGameDatabase(bool enableSystemHdr)
        {
            var games = new List<Game>();

            systemHdrManager.AddHdrExclusionTagToGames(games);

            mockGameCollection.Verify(mock => mock.Update(It.IsAny<Game>()), Times.Never);
        }

        [Test]
        public void AddHdrExclusionTagToGames_UpdatesGameDatabase()
        {
            var games = new List<Game>
            {
                new GameBuilder().WithTagIds(SystemHdrManager.HdrExclusionTagId).Build(),
                new GameBuilder().WithTagIds(Guid.NewGuid()).Build(),
                new GameBuilder().WithTagIds(Guid.NewGuid(), SystemHdrManager.HdrExclusionTagId).Build(),
                new GameBuilder().Build()
            };

            systemHdrManager.AddHdrExclusionTagToGames(games);

            mockGameCollection.Verify(mock => mock.Update(It.IsAny<Game>()), Times.Exactly(games.Count));
        }

        [Test]
        public void AddHdrExclusionTagToGames_DoesNotDuplicateHdrExclusionTag()
        {
            Game game = new GameBuilder().WithTagIds(SystemHdrManager.HdrExclusionTagId).Build();

            systemHdrManager.AddHdrExclusionTagToGames(new List<Game> { game });

            Assert.That(game.TagIds, Does.Contain(SystemHdrManager.HdrExclusionTagId));
            Assert.That(game.TagIds, Has.One.Items);
        }

        [Test]
        public void AddHdrExclusionTagToGames_DoesNotRemoveOtherTags()
        {
            Guid tagIdA = Guid.NewGuid();
            Guid tagIdB = Guid.NewGuid();

            Game game = new GameBuilder().WithTagIds(tagIdA, tagIdB, SystemHdrManager.HdrExclusionTagId).Build();

            systemHdrManager.AddHdrExclusionTagToGames(new List<Game> { game });

            Assert.That(game.TagIds, Does.Contain(SystemHdrManager.HdrExclusionTagId));
            Assert.That(game.TagIds, Does.Contain(tagIdA));
            Assert.That(game.TagIds, Does.Contain(tagIdB));
            Assert.That(game.TagIds, Has.Exactly(3).Items);
        }

        [Test]
        public void AddHdrExclusionTagToGames_HandlesEmptyTagIdsOnGame()
        {
            Game game = new GameBuilder().WithTagIds().Build();

            systemHdrManager.AddHdrExclusionTagToGames(new List<Game> { game });

            Assert.That(game.TagIds, Does.Contain(SystemHdrManager.HdrExclusionTagId));
            Assert.That(game.TagIds, Has.One.Items);
        }

        [Test]
        public void AddHdrExclusionTagToGames_HandlesNullTagIdsOnGame()
        {
            Game game = new GameBuilder().Build();

            systemHdrManager.AddHdrExclusionTagToGames(new List<Game> { game });

            Assert.That(game.TagIds, Does.Contain(SystemHdrManager.HdrExclusionTagId));
            Assert.That(game.TagIds, Has.One.Items);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void RemoveHdrExclusionTagFromGames_NoGames_DoesNotUpdateGameDatabase(bool enableSystemHdr)
        {
            var games = new List<Game>();

            systemHdrManager.RemoveHdrExclusionTagFromGames(games);

            mockGameCollection.Verify(mock => mock.Update(It.IsAny<Game>()), Times.Never);
        }

        [Test]
        public void RemoveHdrExclusionTagFromGames_UpdatesGameDatabase()
        {
            var games = new List<Game>
            {
                new GameBuilder().WithTagIds(SystemHdrManager.HdrExclusionTagId).Build(),
                new GameBuilder().WithTagIds(Guid.NewGuid()).Build(),
                new GameBuilder().WithTagIds(Guid.NewGuid(), SystemHdrManager.HdrExclusionTagId).Build(),
                new GameBuilder().Build()
            };

            systemHdrManager.RemoveHdrExclusionTagFromGames(games);

            mockGameCollection.Verify(mock => mock.Update(It.IsAny<Game>()), Times.Exactly(games.Count));
        }

        [Test]
        public void RemoveHdrExclusionTagFromGames_DoesNotRemoveOtherTags()
        {
            Guid tagIdA = Guid.NewGuid();
            Guid tagIdB = Guid.NewGuid();

            Game game = new GameBuilder().WithTagIds(tagIdA, tagIdB, SystemHdrManager.HdrExclusionTagId).Build();

            systemHdrManager.RemoveHdrExclusionTagFromGames(new List<Game> { game });

            Assert.That(game.TagIds, Does.Contain(tagIdA));
            Assert.That(game.TagIds, Does.Contain(tagIdB));
            Assert.That(game.TagIds, Has.Exactly(2).Items);
        }

        [Test]
        public void RemoveHdrExclusionTagFromGames_HandlesEmptyTagIdsOnGame()
        {
            Game game = new GameBuilder().WithTagIds().Build();

            systemHdrManager.RemoveHdrExclusionTagFromGames(new List<Game> { game });

            Assert.That(game.TagIds, Is.Null.Or.Empty);
        }

        [Test]
        public void RemoveHdrExclusionTagFromGames_HandlesNullTagIdsOnGame()
        {
            Game game = new GameBuilder().Build();

            systemHdrManager.RemoveHdrExclusionTagFromGames(new List<Game> { game });

            Assert.That(game.TagIds, Is.Null.Or.Empty);
        }

        [Test]
        public void CreateOrUpdateHdrExclusionTag_CreatesNewTag()
        {
            string expectedTagName = "HDR Exclusion Tag";

            Tag tag = systemHdrManager.CreateOrUpdateHdrExclusionTag(expectedTagName);

            Assert.That(tag.Name, Is.EqualTo(expectedTagName));
            Assert.That(tag.Id, Is.EqualTo(SystemHdrManager.HdrExclusionTagId));
        }

        [Test]
        public void CreateOrUpdateHdrExclusionTag_UpdatesExistingTag()
        {
            Tag existingTag = new Tag("Old Name")
            {
                Id = SystemHdrManager.HdrExclusionTagId
            };

            backingTagList.Add(existingTag);

            string expectedTagName = "New Name";
            Tag tag = systemHdrManager.CreateOrUpdateHdrExclusionTag(expectedTagName);

            Assert.That(tag, Is.SameAs(existingTag));
            Assert.That(tag.Name, Is.EqualTo(expectedTagName));
            Assert.That(tag.Id, Is.EqualTo(SystemHdrManager.HdrExclusionTagId));
        }
    }
}
