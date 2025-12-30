using HdrManager.Test.Helper;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Internal;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;

namespace HdrManager.Test
{
    [TestFixture]
    public class SystemHdrManagerTest
    {
        private readonly List<Tag> _backingTagList;
        private readonly List<Game> _backingGameList;
        private readonly List<GameFeature> _backingFeatureList;

        private readonly Mock<IItemCollection<Tag>> _mockTagCollection;
        private readonly Mock<IItemCollection<Game>> _mockGameCollection;
        private readonly Mock<IItemCollection<GameFeature>> _mockFeatureCollection;

        private readonly Mock<IGameDatabaseAPI> _mockGameDatabaseApi;
        private readonly Mock<IPlayniteAPI> _mockPlayniteApi;

        private readonly SystemHdrManager _systemHdrManager;

        public SystemHdrManagerTest()
        {
            _backingTagList = new List<Tag>();
            _mockTagCollection = new Mock<IItemCollection<Tag>>();
            _mockTagCollection
                .Setup(mock => mock.GetEnumerator())
                .Returns(() => _backingTagList.GetEnumerator());

            _backingGameList = new List<Game>();
            _mockGameCollection = new Mock<IItemCollection<Game>>();
            _mockGameCollection
                .Setup(mock => mock.GetEnumerator())
                .Returns(() => _backingGameList.GetEnumerator());

            _backingFeatureList = new List<GameFeature>();
            _mockFeatureCollection = new Mock<IItemCollection<GameFeature>>();
            _mockFeatureCollection
                .Setup(mock => mock.GetEnumerator())
                .Returns(() => _backingFeatureList.GetEnumerator());

            _mockGameDatabaseApi = new Mock<IGameDatabaseAPI>();
            _mockGameDatabaseApi
                .SetupGet(mock => mock.Tags)
                .Returns(_mockTagCollection.Object);
            _mockGameDatabaseApi
                .SetupGet(mock => mock.Games)
                .Returns(_mockGameCollection.Object);
            _mockGameDatabaseApi
                .SetupGet(mock => mock.Features)
                .Returns(_mockFeatureCollection.Object);

            _mockPlayniteApi = new Mock<IPlayniteAPI>();
            _mockPlayniteApi
                .SetupGet(mock => mock.Database)
                .Returns(_mockGameDatabaseApi.Object);

            _systemHdrManager = new SystemHdrManager(_mockPlayniteApi.Object);
        }

        [TestCase("HDR")]
        [TestCase("HDR Available")]
        [TestCase("Video: HDR")]
        [TestCase("Allows HDR")]
        [TestCase("Supports HDR")]
        [TestCase("High Dynamic Range")]
        [TestCase("High Dynamic Range Supported")]
        [TestCase("Has HDR")]
        [TestCase("hdr")]
        [TestCase("hdr supported")]
        [TestCase("HDR!")]
        [TestCase("\tHDR  ")]
        [TestCase("H D R")]
        [TestCase("HDR Note")]
        public void EnableSystemHdrForManagedGames_GameWithHdrFeature_ShouldHaveEnableSystemHdrTrue(string featureName)
        {
            GameFeature feature = new GameFeature(featureName)
            {
                Id = Guid.NewGuid()
            };

            Game game = new GameBuilder().WithEnableSystemHdr(false).WithFeatureIds(feature.Id).Build();

            _backingGameList.Add(game);
            _backingFeatureList.Add(feature);

            _systemHdrManager.EnableSystemHdrForManagedGames();

            Assert.That(game.EnableSystemHdr, Is.True);
        }

        [TestCase("Miscellaneous")]
        [TestCase("No HDR")]
        [TestCase("Not HDR")]
        [TestCase("HDR Disabled")]
        [TestCase("Without HDR")]
        [TestCase("Disable HDR")]
        [TestCase("Disabled HDR")]
        [TestCase("phraseWithHdrInside")]
        [TestCase("hdr not supported")]
        [TestCase("")]
        [TestCase("      ")]
        [TestCase("\tNo HDR  ")]
        [TestCase("HD")]
        [TestCase("HD-R")]
        [TestCase("Has_Hdr")]
        public void EnableSystemHdrForManagedGames_GameWithoutHdrFeature_ShouldHaveEnableSystemHdrFalse(string featureName)
        {
            GameFeature feature = new GameFeature(featureName)
            {
                Id = Guid.NewGuid()
            };

            Game game = new GameBuilder().WithEnableSystemHdr(false).WithFeatureIds(feature.Id).Build();

            _backingGameList.Add(game);
            _backingFeatureList.Add(feature);

            _systemHdrManager.EnableSystemHdrForManagedGames();

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

            _backingGameList.Add(game);
            _backingFeatureList.Add(feature);

            _systemHdrManager.EnableSystemHdrForManagedGames();

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

            _backingGameList.AddRange(hdrGames);
            _backingGameList.AddRange(nonHdrGames);

            _backingFeatureList.Add(hdrFeature);
            _backingFeatureList.Add(nonHdrFeature);

            _systemHdrManager.EnableSystemHdrForManagedGames();

            using (Assert.EnterMultipleScope())
            {
                foreach (Game game in hdrGames)
                {
                    Assert.That(game.EnableSystemHdr, Is.True, $"Game {game.Name} has EnableSystemHdr set to false but expected true");
                }
                foreach (Game game in nonHdrGames)
                {
                    Assert.That(game.EnableSystemHdr, Is.False, $"Game {game.Name} has EnableSystemHdr set to false but expected true");
                }
            }
        }

        [Test]
        public void EnableSystemHdrForManagedGames_GameWithHdrFeature_AndMiscellaneousTag_ShouldHaveEnableSystemHdrTrue()
        {
            GameFeature feature = new GameFeature("HDR")
            {
                Id = Guid.NewGuid()
            };

            Game game = new GameBuilder().WithEnableSystemHdr(false).WithFeatureIds(feature.Id).WithTagIds(Guid.NewGuid()).Build();

            _backingGameList.Add(game);
            _backingFeatureList.Add(feature);

            _systemHdrManager.EnableSystemHdrForManagedGames();

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

            _backingGameList.Add(game);
            _backingFeatureList.Add(feature);

            _systemHdrManager.EnableSystemHdrForManagedGames();

            Assert.That(game.EnableSystemHdr, Is.False);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void SetSystemHdrForGames_NoGames_DoesNotUpdateGameDatabase(bool enableSystemHdr)
        {
            var games = new List<Game>();

            _systemHdrManager.SetSystemHdrForGames(games, enableSystemHdr);

            _mockGameCollection.Verify(mock => mock.Update(It.IsAny<Game>()), Times.Never);
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

            _systemHdrManager.SetSystemHdrForGames(games, enableSystemHdr);

            using (Assert.EnterMultipleScope())
            {
                foreach (Game game in games)
                {
                    Assert.That(game.EnableSystemHdr, Is.EqualTo(enableSystemHdr), $"Game {game.Name} has EnableSystemHdr set to {!enableSystemHdr} but expected {enableSystemHdr}");
                }
                _mockGameCollection.Verify(mock => mock.Update(It.IsAny<Game>()), Times.Exactly(games.Count));
            }
        }

        [Test]
        public void AddHdrExclusionTagToGames_NoGames_DoesNotUpdateGameDatabase()
        {
            var games = new List<Game>();

            _systemHdrManager.AddHdrExclusionTagToGames(games);

            _mockGameCollection.Verify(mock => mock.Update(It.IsAny<Game>()), Times.Never);
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

            _systemHdrManager.AddHdrExclusionTagToGames(games);

            _mockGameCollection.Verify(mock => mock.Update(It.IsAny<Game>()), Times.Exactly(games.Count));
        }

        [Test]
        public void AddHdrExclusionTagToGames_DoesNotDuplicateHdrExclusionTag()
        {
            Game game = new GameBuilder().WithTagIds(SystemHdrManager.HdrExclusionTagId).Build();

            _systemHdrManager.AddHdrExclusionTagToGames(new List<Game> { game });

            Assert.That(game.TagIds, Does.Contain(SystemHdrManager.HdrExclusionTagId));
            Assert.That(game.TagIds, Has.One.Items);
        }

        [Test]
        public void AddHdrExclusionTagToGames_DoesNotRemoveOtherTags()
        {
            Guid tagIdA = Guid.NewGuid();
            Guid tagIdB = Guid.NewGuid();

            Game game = new GameBuilder().WithTagIds(tagIdA, tagIdB, SystemHdrManager.HdrExclusionTagId).Build();

            _systemHdrManager.AddHdrExclusionTagToGames(new List<Game> { game });

            Assert.That(game.TagIds, Does.Contain(SystemHdrManager.HdrExclusionTagId));
            Assert.That(game.TagIds, Does.Contain(tagIdA));
            Assert.That(game.TagIds, Does.Contain(tagIdB));
            Assert.That(game.TagIds, Has.Exactly(3).Items);
        }

        [Test]
        public void AddHdrExclusionTagToGames_HandlesEmptyTagIdsOnGame()
        {
            Game game = new GameBuilder().WithTagIds().Build();

            _systemHdrManager.AddHdrExclusionTagToGames(new List<Game> { game });

            Assert.That(game.TagIds, Does.Contain(SystemHdrManager.HdrExclusionTagId));
            Assert.That(game.TagIds, Has.One.Items);
        }

        [Test]
        public void AddHdrExclusionTagToGames_HandlesNullTagIdsOnGame()
        {
            Game game = new GameBuilder().Build();

            _systemHdrManager.AddHdrExclusionTagToGames(new List<Game> { game });

            Assert.That(game.TagIds, Does.Contain(SystemHdrManager.HdrExclusionTagId));
            Assert.That(game.TagIds, Has.One.Items);
        }

        [Test]
        public void RemoveHdrExclusionTagFromGames_NoGames_DoesNotUpdateGameDatabase()
        {
            var games = new List<Game>();

            _systemHdrManager.RemoveHdrExclusionTagFromGames(games);

            _mockGameCollection.Verify(mock => mock.Update(It.IsAny<Game>()), Times.Never);
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

            _systemHdrManager.RemoveHdrExclusionTagFromGames(games);

            _mockGameCollection.Verify(mock => mock.Update(It.IsAny<Game>()), Times.Exactly(games.Count));
        }

        [Test]
        public void RemoveHdrExclusionTagFromGames_DoesNotRemoveOtherTags()
        {
            Guid tagIdA = Guid.NewGuid();
            Guid tagIdB = Guid.NewGuid();

            Game game = new GameBuilder().WithTagIds(tagIdA, tagIdB, SystemHdrManager.HdrExclusionTagId).Build();

            _systemHdrManager.RemoveHdrExclusionTagFromGames(new List<Game> { game });

            Assert.That(game.TagIds, Does.Contain(tagIdA));
            Assert.That(game.TagIds, Does.Contain(tagIdB));
            Assert.That(game.TagIds, Has.Exactly(2).Items);
        }

        [Test]
        public void RemoveHdrExclusionTagFromGames_HandlesEmptyTagIdsOnGame()
        {
            Game game = new GameBuilder().WithTagIds().Build();

            _systemHdrManager.RemoveHdrExclusionTagFromGames(new List<Game> { game });

            Assert.That(game.TagIds, Is.Null.Or.Empty);
        }

        [Test]
        public void RemoveHdrExclusionTagFromGames_HandlesNullTagIdsOnGame()
        {
            Game game = new GameBuilder().Build();

            _systemHdrManager.RemoveHdrExclusionTagFromGames(new List<Game> { game });

            Assert.That(game.TagIds, Is.Null.Or.Empty);
        }

        [Test]
        public void CreateOrUpdateHdrExclusionTag_CreatesNewTag()
        {
            string expectedTagName = "HDR Exclusion Tag";

            Tag tag = _systemHdrManager.CreateOrUpdateHdrExclusionTag(expectedTagName);

            Assert.That(tag, Is.Not.Null);
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

            _backingTagList.Add(existingTag);

            string expectedTagName = "New Name";
            Tag tag = _systemHdrManager.CreateOrUpdateHdrExclusionTag(expectedTagName);

            Assert.That(tag, Is.SameAs(existingTag));
            Assert.That(tag.Name, Is.EqualTo(expectedTagName));
            Assert.That(tag.Id, Is.EqualTo(SystemHdrManager.HdrExclusionTagId));
        }
    }
}
