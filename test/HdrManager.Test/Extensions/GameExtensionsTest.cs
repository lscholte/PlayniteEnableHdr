using HdrManager.Extensions;
using NUnit.Framework;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;

namespace HdrManager.Test.Extensions
{
    [TestFixture]
    public class GameExtensionsTest
    {
        private static readonly Guid hdrFeatureA = Guid.NewGuid();
        private static readonly Guid hdrFeatureB = Guid.NewGuid();
        private static readonly Guid hdrFeatureC = Guid.NewGuid();

        private static readonly IEnumerable<Guid> hdrFeatures = new List<Guid>
        {
            hdrFeatureA,
            hdrFeatureB,
            hdrFeatureC
        };

        private static readonly Guid miscellaneousFeature = Guid.NewGuid();

        private static readonly Guid hdrExclusionTag = Guid.NewGuid();
        private static readonly Guid miscellaneousTag = Guid.NewGuid();

        [Test]
        public void HasHdrFeature_ReturnsFalse_WhenGameHasNullFeatures()
        {
            Game game = new Game
            {
                FeatureIds = null
            };

            bool result = game.HasHdrFeature(hdrFeatures);

            Assert.That(result, Is.False);
        }

        [Test]
        public void HasHdrFeature_ReturnsFalse_WhenGameHasEmptyFeatures()
        {
            Game game = new Game
            {
                FeatureIds = new List<Guid>()
            };

            bool result = game.HasHdrFeature(hdrFeatures);

            Assert.That(result, Is.False);
        }

        [Test]
        public void HasHdrFeature_ReturnsFalse_WhenGameHasNoHdrFeatures()
        {
            Game game = new Game
            {
                FeatureIds = new List<Guid>
                {
                    miscellaneousFeature
                }
            };

            bool result = game.HasHdrFeature(hdrFeatures);

            Assert.That(result, Is.False);
        }

        [Test]
        public void HasHdrFeature_ReturnsTrue_WhenGameHasOneMatchingHdrFeatures()
        {
            Game game = new Game
            {
                FeatureIds = new List<Guid>
                {
                    hdrFeatureA,
                    miscellaneousFeature
                }
            };

            bool result = game.HasHdrFeature(hdrFeatures);

            Assert.That(result, Is.True);
        }

        [Test]
        public void HasHdrFeature_ReturnsTrue_WhenGameHasMultipleMatchingHdrFeatures()
        {
            Game game = new Game
            {
                FeatureIds = new List<Guid>
                {
                    hdrFeatureA,
                    hdrFeatureB,
                    miscellaneousFeature
                }
            };

            bool result = game.HasHdrFeature(hdrFeatures);

            Assert.That(result, Is.True);
        }

        [Test]
        public void HasHdrExclusionTag_ReturnsFalse_WhenGameHasNullTags()
        {
            Game game = new Game
            {
                TagIds = null
            };

            bool result = game.HasHdrExclusionTag(hdrExclusionTag);

            Assert.That(result, Is.False);
        }

        [Test]
        public void HasHdrExclusionTag_ReturnsFalse_WhenGameHasEmptyTags()
        {
            Game game = new Game
            {
                TagIds = new List<Guid>()
            };

            bool result = game.HasHdrExclusionTag(hdrExclusionTag);

            Assert.That(result, Is.False);
        }

        [Test]
        public void HasHdrExclusionTag_ReturnsFalse_WhenGameHasNoHdrExclusionTag()
        {
            Game game = new Game
            {
                TagIds = new List<Guid>
                {
                    miscellaneousTag
                }
            };

            bool result = game.HasHdrExclusionTag(hdrExclusionTag);

            Assert.That(result, Is.False);
        }

        [Test]
        public void HasHdrExclusionTag_ReturnsTrue_WhenGameHasHdrExclusionTag()
        {
            Game game = new Game
            {
                TagIds = new List<Guid>
                {
                    hdrExclusionTag,
                    miscellaneousTag
                }
            };

            bool result = game.HasHdrExclusionTag(hdrExclusionTag);

            Assert.That(result, Is.True);
        }

        [Test]
        public static void AddTag_AddsTag_WhenGameHasNullTags()
        {
            Game game = new Game
            {
                TagIds = null
            };

            game.AddTag(hdrExclusionTag);

            Assert.That(game.TagIds, Is.Not.Null);
            Assert.That(game.TagIds, Has.One.Items);
            Assert.That(game.TagIds, Contains.Item(hdrExclusionTag));
        }

        [Test]
        public static void AddTag_AddsTag_WhenGameHasEmptyTags()
        {
            Game game = new Game
            {
                TagIds = new List<Guid>()
            };

            game.AddTag(hdrExclusionTag);

            Assert.That(game.TagIds, Is.Not.Null);
            Assert.That(game.TagIds, Has.One.Items);
            Assert.That(game.TagIds, Contains.Item(hdrExclusionTag));
        }

        [Test]
        public static void AddTag_AddsTag_WhenGameHasNoMatchingTags()
        {
            Game game = new Game
            {
                TagIds = new List<Guid>
                {
                    miscellaneousTag
                }
            };

            game.AddTag(hdrExclusionTag);

            Assert.That(game.TagIds, Is.Not.Null);
            Assert.That(game.TagIds, Has.Exactly(2).Items);
            Assert.That(game.TagIds, Contains.Item(miscellaneousTag));
            Assert.That(game.TagIds, Contains.Item(hdrExclusionTag));
        }

        [Test]
        public static void AddTag_DoesNotAddTag_WhenGameAlreadyHasTag()
        {
            Game game = new Game
            {
                TagIds = new List<Guid>
                {
                    hdrExclusionTag
                }
            };

            game.AddTag(hdrExclusionTag);

            Assert.That(game.TagIds, Is.Not.Null);
            Assert.That(game.TagIds, Has.One.Items);
            Assert.That(game.TagIds, Contains.Item(hdrExclusionTag));
        }
    }
}
