using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HdrManager.Extensions
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

        public static bool HasTag(this Game game, Guid tagId)
        {
            return game
                .TagIds
                .EmptyIfNull()
                .Contains(tagId);
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
