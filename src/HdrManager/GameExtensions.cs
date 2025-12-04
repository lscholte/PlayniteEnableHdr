using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HdrManager
{
    public static class GameExtensions
    {
        public static bool HasHdrFeature(this Game game, IEnumerable<Guid> hdrFeatureIds)
        {
            return game
                .FeatureIds
                .EmptyIfNull()
                .Intersect(hdrFeatureIds)
                .Any();
        }

        public static bool HasHdrExclusionTag(this Game game, Guid hdrExclusionTagId)
        {
            return game
                .TagIds
                .EmptyIfNull()
                .Contains(hdrExclusionTagId);
        }

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
