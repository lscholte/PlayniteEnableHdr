using HdrManager.Test.Helper;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Internal;
using Playnite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HdrManager.Test
{
    [TestFixture]
    public class SystemHdrManagerTest
    {
        private readonly List<Tag> _backingTagList;
        private readonly List<Game> _backingGameList;
        private readonly List<Feature> _backingFeatureList;

        private readonly Mock<ILibraryCollection<Tag>> _mockTagCollection;
        private readonly Mock<ILibraryCollection<Game>> _mockGameCollection;
        private readonly Mock<ILibraryCollection<Feature>> _mockFeatureCollection;

        private readonly Mock<ILibraryApi> _mockLibraryApi;
        private readonly Mock<IPlayniteApi> _mockPlayniteApi;

        private readonly SystemHdrManager _systemHdrManager;

        public SystemHdrManagerTest()
        {
            _backingTagList = new List<Tag>();
            _mockTagCollection = new Mock<ILibraryCollection<Tag>>();
            _mockTagCollection
                .Setup(mock => mock.GetEnumerator())
                .Returns(() => _backingTagList.GetEnumerator());

            _backingGameList = new List<Game>();
            _mockGameCollection = new Mock<ILibraryCollection<Game>>();
            _mockGameCollection
                .Setup(mock => mock.GetEnumerator())
                .Returns(() => _backingGameList.GetEnumerator());

            _backingFeatureList = new List<Feature>();
            _mockFeatureCollection = new Mock<ILibraryCollection<Feature>>();
            _mockFeatureCollection
                .Setup(mock => mock.GetEnumerator())
                .Returns(() => _backingFeatureList.GetEnumerator());

            _mockLibraryApi = new Mock<ILibraryApi>();
            _mockLibraryApi
                .SetupGet(mock => mock.Tags)
                .Returns(_mockTagCollection.Object);
            _mockLibraryApi
                .SetupGet(mock => mock.Games)
                .Returns(_mockGameCollection.Object);
            _mockLibraryApi
                .SetupGet(mock => mock.Features)
                .Returns(_mockFeatureCollection.Object);

            _mockPlayniteApi = new Mock<IPlayniteApi>(MockBehavior.Loose);
            _mockPlayniteApi
                .SetupGet(mock => mock.Library)
                .Returns(_mockLibraryApi.Object);

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
        public async Task EnableSystemHdrForManagedGames_GameWithHdrFeature_ShouldHaveEnableSystemHdrTrue(string featureName)
        {
            Feature feature = new Feature(featureName)
            {
                Id = "Feature ID"
            };

            Game game = new GameBuilder().WithEnableSystemHdr(false).WithFeatureIds(feature.Id).Build();

            _backingGameList.Add(game);
            _backingFeatureList.Add(feature);

            await _systemHdrManager.EnableSystemHdrForManagedGames();

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
        public async Task EnableSystemHdrForManagedGames_GameWithoutHdrFeature_ShouldHaveEnableSystemHdrFalse(string featureName)
        {
            Feature feature = new Feature(featureName)
            {
                Id = "Feature ID"
            };

            Game game = new GameBuilder().WithEnableSystemHdr(false).WithFeatureIds(feature.Id).Build();

            _backingGameList.Add(game);
            _backingFeatureList.Add(feature);

            await _systemHdrManager.EnableSystemHdrForManagedGames();

            Assert.That(game.EnableSystemHdr, Is.False);
        }

        [TestCase("Miscellaneous")]
        [TestCase("No HDR")]
        public async Task EnableSystemHdrForManagedGames_GameWithoutHdrFeature_AlreadyHasEnableSystemHdrTrue_ShouldHaveEnableSystemHdrTrue(string featureName)
        {
            Feature feature = new Feature(featureName)
            {
                Id = "Feature ID"
            };

            Game game = new GameBuilder().WithEnableSystemHdr(true).WithFeatureIds(feature.Id).Build();

            _backingGameList.Add(game);
            _backingFeatureList.Add(feature);

            await _systemHdrManager.EnableSystemHdrForManagedGames();

            Assert.That(game.EnableSystemHdr, Is.True);
        }

        [Test]
        public async Task EnableSystemHdrForManagedGames_ListOfMixedGames_ShouldHaveEnableSystemHdrTrueOnlyForHdrGames()
        {
            Feature hdrFeature = new Feature("HDR")
            {
                Id = "Feature ID HDR"
            };
            Feature nonHdrFeature = new Feature("Miscellaneous")
            {
                Id = "Feature ID Miscellaneous"
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

            await _systemHdrManager.EnableSystemHdrForManagedGames();

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
        public async Task EnableSystemHdrForManagedGames_GameWithHdrFeature_AndMiscellaneousTag_ShouldHaveEnableSystemHdrTrue()
        {
            Feature feature = new Feature("HDR")
            {
                Id = "Feature ID"
            };

            Game game = new GameBuilder().WithEnableSystemHdr(false).WithFeatureIds(feature.Id).WithTagIds("Tag A").Build();

            _backingGameList.Add(game);
            _backingFeatureList.Add(feature);

            await _systemHdrManager.EnableSystemHdrForManagedGames();

            Assert.That(game.EnableSystemHdr, Is.True);
        }

        [Test]
        public async Task EnableSystemHdrForManagedGames_GameWithHdrFeature_AndHdrExclusionTag_ShouldHaveEnableSystemHdrFalse()
        {
            Feature feature = new Feature("HDR")
            {
                Id = "Feature ID"
            };

            Game game = new GameBuilder().WithEnableSystemHdr(false).WithFeatureIds(feature.Id).WithTagIds(SystemHdrManager.HdrExclusionTagId).Build();

            _backingGameList.Add(game);
            _backingFeatureList.Add(feature);

            await _systemHdrManager.EnableSystemHdrForManagedGames();

            Assert.That(game.EnableSystemHdr, Is.False);
        }

        [TestCase(true)]
        [TestCase(false)]
        public async Task SetSystemHdrForGames_NoGames_DoesNotUpdateGameDatabase(bool enableSystemHdr)
        {
            var games = new List<Game>();

            await _systemHdrManager.SetSystemHdrForGames(games, enableSystemHdr);

            _mockGameCollection.Verify(mock => mock.MakeBulkChangesAsync(Enumerable.Empty<Game>(), games, Enumerable.Empty<Game>()), Times.Once);
        }

        [TestCase(true)]
        [TestCase(false)]
        public async Task SetSystemHdrForGames_SetsEnableSystemHdrOnEachGame(bool enableSystemHdr)
        {
            var games = new List<Game>
            {
                new GameBuilder().WithName("A").WithEnableSystemHdr(enableSystemHdr).Build(),
                new GameBuilder().WithName("B").WithEnableSystemHdr(!enableSystemHdr).Build(),
            };

            await _systemHdrManager.SetSystemHdrForGames(games, enableSystemHdr);

            using (Assert.EnterMultipleScope())
            {
                foreach (Game game in games)
                {
                    Assert.That(game.EnableSystemHdr, Is.EqualTo(enableSystemHdr), $"Game {game.Name} has EnableSystemHdr set to {!enableSystemHdr} but expected {enableSystemHdr}");
                }
                _mockGameCollection.Verify(mock => mock.MakeBulkChangesAsync(Enumerable.Empty<Game>(), games, Enumerable.Empty<Game>()), Times.Once);
            }
        }

        [Test]
        public async Task AddHdrExclusionTagToGames_NoGames_DoesNotUpdateGameDatabase()
        {
            var games = new List<Game>();

            await _systemHdrManager.AddHdrExclusionTagToGames(games);

            _mockGameCollection.Verify(mock => mock.MakeBulkChangesAsync(Enumerable.Empty<Game>(), games, Enumerable.Empty<Game>()), Times.Once);
        }

        [Test]
        public async Task AddHdrExclusionTagToGames_UpdatesGameDatabase()
        {
            var games = new List<Game>
            {
                new GameBuilder().WithTagIds(SystemHdrManager.HdrExclusionTagId).Build(),
                new GameBuilder().WithTagIds("Tag A").Build(),
                new GameBuilder().WithTagIds("Tag B", SystemHdrManager.HdrExclusionTagId).Build(),
                new GameBuilder().Build()
            };

            await _systemHdrManager.AddHdrExclusionTagToGames(games);

            _mockGameCollection.Verify(mock => mock.MakeBulkChangesAsync(Enumerable.Empty<Game>(), games, Enumerable.Empty<Game>()), Times.Once);
        }

        [Test]
        public async Task AddHdrExclusionTagToGames_DoesNotDuplicateHdrExclusionTag()
        {
            Game game = new GameBuilder().WithTagIds(SystemHdrManager.HdrExclusionTagId).Build();

            await _systemHdrManager.AddHdrExclusionTagToGames([game]);

            Assert.That(game.TagIds, Does.Contain(SystemHdrManager.HdrExclusionTagId));
            Assert.That(game.TagIds, Has.One.Items);
        }

        [Test]
        public async Task AddHdrExclusionTagToGames_DoesNotRemoveOtherTags()
        {
            string tagIdA = "Tag A";
            string tagIdB = "Tag B";

            Game game = new GameBuilder().WithTagIds(tagIdA, tagIdB, SystemHdrManager.HdrExclusionTagId).Build();

            await _systemHdrManager.AddHdrExclusionTagToGames([game]);

            Assert.That(game.TagIds, Does.Contain(SystemHdrManager.HdrExclusionTagId));
            Assert.That(game.TagIds, Does.Contain(tagIdA));
            Assert.That(game.TagIds, Does.Contain(tagIdB));
            Assert.That(game.TagIds, Has.Exactly(3).Items);
        }

        [Test]
        public async Task AddHdrExclusionTagToGames_HandlesEmptyTagIdsOnGame()
        {
            Game game = new GameBuilder().WithTagIds().Build();

            await _systemHdrManager.AddHdrExclusionTagToGames([game]);

            Assert.That(game.TagIds, Does.Contain(SystemHdrManager.HdrExclusionTagId));
            Assert.That(game.TagIds, Has.One.Items);
        }

        [Test]
        public async Task AddHdrExclusionTagToGames_HandlesNullTagIdsOnGame()
        {
            Game game = new GameBuilder().Build();

            await _systemHdrManager.AddHdrExclusionTagToGames([game]);

            Assert.That(game.TagIds, Does.Contain(SystemHdrManager.HdrExclusionTagId));
            Assert.That(game.TagIds, Has.One.Items);
        }

        [Test]
        public async Task RemoveHdrExclusionTagFromGames_NoGames_DoesNotUpdateGameDatabase()
        {
            var games = new List<Game>();

            await _systemHdrManager.RemoveHdrExclusionTagFromGames(games);

            _mockGameCollection.Verify(mock => mock.MakeBulkChangesAsync(Enumerable.Empty<Game>(), games, Enumerable.Empty<Game>()), Times.Once);
        }

        [Test]
        public async Task RemoveHdrExclusionTagFromGames_UpdatesGameDatabase()
        {
            var games = new List<Game>
            {
                new GameBuilder().WithTagIds(SystemHdrManager.HdrExclusionTagId).Build(),
                new GameBuilder().WithTagIds("Tag A").Build(),
                new GameBuilder().WithTagIds("Tag B", SystemHdrManager.HdrExclusionTagId).Build(),
                new GameBuilder().Build()
            };

            await _systemHdrManager.RemoveHdrExclusionTagFromGames(games);

            _mockGameCollection.Verify(mock => mock.MakeBulkChangesAsync(Enumerable.Empty<Game>(), games, Enumerable.Empty<Game>()), Times.Once);
        }

        [Test]
        public async Task RemoveHdrExclusionTagFromGames_DoesNotRemoveOtherTags()
        {
            string tagIdA = "Tag A";
            string tagIdB = "Tag B";

            Game game = new GameBuilder().WithTagIds(tagIdA, tagIdB, SystemHdrManager.HdrExclusionTagId).Build();

            await _systemHdrManager.RemoveHdrExclusionTagFromGames(new List<Game> { game });

            Assert.That(game.TagIds, Does.Contain(tagIdA));
            Assert.That(game.TagIds, Does.Contain(tagIdB));
            Assert.That(game.TagIds, Has.Exactly(2).Items);
        }

        [Test]
        public async Task RemoveHdrExclusionTagFromGames_HandlesEmptyTagIdsOnGame()
        {
            Game game = new GameBuilder().WithTagIds().Build();

            await _systemHdrManager.RemoveHdrExclusionTagFromGames([game]);

            Assert.That(game.TagIds, Is.Null.Or.Empty);
        }

        [Test]
        public async Task RemoveHdrExclusionTagFromGames_HandlesNullTagIdsOnGame()
        {
            Game game = new GameBuilder().Build();

            await _systemHdrManager.RemoveHdrExclusionTagFromGames([game]);

            Assert.That(game.TagIds, Is.Null.Or.Empty);
        }

        [Test]
        public async Task CreateOrUpdateHdrExclusionTag_CreatesNewTag()
        {
            string expectedTagName = "HDR Exclusion Tag";

            Tag tag = await _systemHdrManager.CreateOrUpdateHdrExclusionTag(expectedTagName);

            Assert.That(tag, Is.Not.Null);
            Assert.That(tag.Name, Is.EqualTo(expectedTagName));
            Assert.That(tag.Id, Is.EqualTo(SystemHdrManager.HdrExclusionTagId));
        }

        [Test]
        public async Task CreateOrUpdateHdrExclusionTag_UpdatesExistingTag()
        {
            Tag existingTag = new Tag("Old Name")
            {
                Id = SystemHdrManager.HdrExclusionTagId
            };

            _backingTagList.Add(existingTag);

            string expectedTagName = "New Name";
            Tag tag = await _systemHdrManager.CreateOrUpdateHdrExclusionTag(expectedTagName);

            Assert.That(tag, Is.SameAs(existingTag));
            Assert.That(tag.Name, Is.EqualTo(expectedTagName));
            Assert.That(tag.Id, Is.EqualTo(SystemHdrManager.HdrExclusionTagId));
        }
    }
}
