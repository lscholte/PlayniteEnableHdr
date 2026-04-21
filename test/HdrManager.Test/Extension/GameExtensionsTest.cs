using HdrManager.Extension;
using NUnit.Framework;
using Playnite;
using System.Collections.Generic;

namespace HdrManager.Test.Extension
{
    [TestFixture]
    public class GameExtensionsTest
    {
        private const string _hdrFeatureA = "Feature A";
        private const string _hdrFeatureB = "Feature B";
        private const string _hdrFeatureC = "Feature C";

        private static readonly IEnumerable<string> _hdrFeatures =
        [
            _hdrFeatureA,
            _hdrFeatureB,
            _hdrFeatureC
        ];

        private const string _miscellaneousFeature = "Miscellaneous Feature";

        private const string _hdrExclusionTag = "HDR Exclusion Tag";
        private const string _miscellaneousTag = "Miscellaneous Tag";

        [Test]
        public void HasAnyFeature_ReturnsFalse_WhenGameHasNullFeatures()
        {
            Game game = new Game
            {
                FeatureIds = null
            };

            bool result = game.HasAnyFeature(_hdrFeatures);

            Assert.That(result, Is.False);
        }

        [Test]
        public void HasAnyFeature_ReturnsFalse_WhenGameHasEmptyFeatures()
        {
            Game game = new Game
            {
                FeatureIds = new HashSet<string>()
            };

            bool result = game.HasAnyFeature(_hdrFeatures);

            Assert.That(result, Is.False);
        }

        [Test]
        public void HasAnyFeature_ReturnsFalse_WhenGameHasNoHdrFeatures()
        {
            Game game = new Game
            {
                FeatureIds = new HashSet<string>
                {
                    _miscellaneousFeature
                }
            };

            bool result = game.HasAnyFeature(_hdrFeatures);

            Assert.That(result, Is.False);
        }

        [Test]
        public void HasAnyFeature_ReturnsTrue_WhenGameHasOneMatchingHdrFeatures()
        {
            Game game = new Game
            {
                FeatureIds = new HashSet<string>
                {
                    _hdrFeatureA,
                    _miscellaneousFeature
                }
            };

            bool result = game.HasAnyFeature(_hdrFeatures);

            Assert.That(result, Is.True);
        }

        [Test]
        public void HasAnyFeature_ReturnsTrue_WhenGameHasMultipleMatchingHdrFeatures()
        {
            Game game = new Game
            {
                FeatureIds = new HashSet<string>
                {
                    _hdrFeatureA,
                    _hdrFeatureB,
                    _miscellaneousFeature
                }
            };

            bool result = game.HasAnyFeature(_hdrFeatures);

            Assert.That(result, Is.True);
        }

        [Test]
        public void HasTag_ReturnsFalse_WhenGameHasNullTags()
        {
            Game game = new Game
            {
                TagIds = null
            };

            bool result = game.HasTag(_hdrExclusionTag);

            Assert.That(result, Is.False);
        }

        [Test]
        public void HasTag_ReturnsFalse_WhenGameHasEmptyTags()
        {
            Game game = new Game
            {
                TagIds = new HashSet<string>()
            };

            bool result = game.HasTag(_hdrExclusionTag);

            Assert.That(result, Is.False);
        }

        [Test]
        public void HasTag_ReturnsFalse_WhenGameHasNoHdrExclusionTag()
        {
            Game game = new Game
            {
                TagIds = new HashSet<string>
                {
                    _miscellaneousTag
                }
            };

            bool result = game.HasTag(_hdrExclusionTag);

            Assert.That(result, Is.False);
        }

        [Test]
        public void HasTag_ReturnsTrue_WhenGameHasHdrExclusionTag()
        {
            Game game = new Game
            {
                TagIds = new HashSet<string>
                {
                    _hdrExclusionTag,
                    _miscellaneousTag
                }
            };

            bool result = game.HasTag(_hdrExclusionTag);

            Assert.That(result, Is.True);
        }

        [Test]
        public static void AddTag_AddsTag_WhenGameHasNullTags()
        {
            Game game = new Game
            {
                TagIds = null
            };

            game.AddTag(_hdrExclusionTag);

            Assert.That(game.TagIds, Is.Not.Null);
            Assert.That(game.TagIds, Has.One.Items);
            Assert.That(game.TagIds, Contains.Item(_hdrExclusionTag));
        }

        [Test]
        public static void AddTag_AddsTag_WhenGameHasEmptyTags()
        {
            Game game = new Game
            {
                TagIds = new HashSet<string>()
            };

            game.AddTag(_hdrExclusionTag);

            Assert.That(game.TagIds, Is.Not.Null);
            Assert.That(game.TagIds, Has.One.Items);
            Assert.That(game.TagIds, Contains.Item(_hdrExclusionTag));
        }

        [Test]
        public static void AddTag_AddsTag_WhenGameHasNoMatchingTags()
        {
            Game game = new Game
            {
                TagIds = new HashSet<string>
                {
                    _miscellaneousTag
                }
            };

            game.AddTag(_hdrExclusionTag);

            Assert.That(game.TagIds, Is.Not.Null);
            Assert.That(game.TagIds, Has.Exactly(2).Items);
            Assert.That(game.TagIds, Contains.Item(_miscellaneousTag));
            Assert.That(game.TagIds, Contains.Item(_hdrExclusionTag));
        }

        [Test]
        public static void AddTag_DoesNotAddTag_WhenGameAlreadyHasTag()
        {
            Game game = new Game
            {
                TagIds = new HashSet<string>
                {
                    _hdrExclusionTag
                }
            };

            game.AddTag(_hdrExclusionTag);

            Assert.That(game.TagIds, Is.Not.Null);
            Assert.That(game.TagIds, Has.One.Items);
            Assert.That(game.TagIds, Contains.Item(_hdrExclusionTag));
        }
    }
}
