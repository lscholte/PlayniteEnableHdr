using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HdrManager.Extensions
{
    /// <summary>
    /// Provides extension methods for working with feature and tag identifiers on Game instances.
    /// </summary>
    /// <remarks>These methods extend the Game class to simplify common operations related to features and
    /// tags, such as checking for the presence of specific identifiers or adding tags. All methods require a non-null
    /// Game instance.</remarks>
    public static class GameExtensions
    {
        /// <summary>
        /// Determines whether the specified game contains any of the given feature identifiers.
        /// </summary>
        /// <param name="game">The game to check for the presence of features. Cannot be null.</param>
        /// <param name="featureIds">A collection of feature identifiers to search for. Cannot be null.</param>
        /// <returns>true if the game contains at least one of the specified feature identifiers; otherwise, false.</returns>
        public static bool HasAnyFeature(this Game game, IEnumerable<Guid> featureIds)
        {
            return game
                .FeatureIds
                .EmptyIfNull()
                .Intersect(featureIds)
                .Any();
        }

        /// <summary>
        /// Determines whether the specified game contains the given tag identifier.
        /// </summary>
        /// <param name="game">The game instance to check for the presence of the tag. Cannot be null.</param>
        /// <param name="tagId">The unique identifier of the tag to locate within the game's tags.</param>
        /// <returns>true if the game contains the specified tag identifier; otherwise, false.</returns>
        public static bool HasTag(this Game game, Guid tagId)
        {
            return game
                .TagIds
                .EmptyIfNull()
                .Contains(tagId);
        }

        /// <summary>
        /// Adds the specified tag identifier to the game's collection of tag IDs if it is not already present.
        /// </summary>
        /// <remarks>If the game's tag collection is null, a new collection is created containing the
        /// specified tag. If the tag already exists in the collection, it is not added again.</remarks>
        /// <param name="game">The game instance to which the tag will be added. Cannot be null.</param>
        /// <param name="tagId">The unique identifier of the tag to add to the game.</param>
        public static void AddTag(this Game game, Guid tagId)
        {
            if (game.TagIds == null)
            {
                game.TagIds = new List<Guid>() { tagId };
            }
            else
            {
                game.TagIds.AddMissing(tagId);
            }
        }
    }
}
