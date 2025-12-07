using HdrManager.Extensions;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HdrManager
{
    public class SystemHdrManager : ISystemHdrManager
    {
        public static readonly Guid HdrExclusionTagId = Guid.Parse("b7f2a9d3-4c1e-4a8b-9f6d-2e3c1a5d7b84");

        private static readonly ILogger logger = LogManager.GetLogger();
        private static readonly List<string> HdrFeatureNames = new List<string>()
        {
            "HDR",
            "HDR Available"
        };

        private readonly IPlayniteAPI playniteApi;

        public SystemHdrManager(IPlayniteAPI playniteApi)
        {
            this.playniteApi = playniteApi;
        }

        public void EnableSystemHdrForManagedGames()
        {
            IEnumerable<Guid> hdrFeatureIds =
                playniteApi
                    .Database
                    .Features
                    .Where(f => HdrFeatureNames.Contains(f.Name))
                    .Select(f => f.Id)
                    .ToList();

            List<Game> managedHdrGames = playniteApi
                    .Database
                    .Games
                    .Where(game => game.HasHdrFeature(hdrFeatureIds) && !game.HasHdrExclusionTag(HdrExclusionTagId))
                    .ToList();

            logger.Info($"Enabling System HDR for {managedHdrGames.Count} games");
            SetSystemHdrForGames(managedHdrGames, true);
        }

        public void SetSystemHdrForGames(IEnumerable<Game> games, bool enableSystemHdr)
        {
            using (playniteApi.Database.BufferedUpdate())
            {
                foreach (var game in games)
                {
                    logger.Trace($"Setting EnableSystemHdr for game {game.Name} to {enableSystemHdr}");
                    game.EnableSystemHdr = enableSystemHdr;
                    playniteApi.Database.Games.Update(game);
                }
            }
        }

        public void AddHdrExclusionTagToGames(IEnumerable<Game> games)
        {
            using (playniteApi.Database.BufferedUpdate())
            {
                foreach (var game in games)
                {
                    logger.Trace($"Adding HDR Exclusion tag to game {game.Name}");
                    game.AddTag(HdrExclusionTagId);
                    playniteApi.Database.Games.Update(game);
                }
            }
        }

        public void RemoveHdrExclusionTagFromGames(IEnumerable<Game> games)
        {
            using (playniteApi.Database.BufferedUpdate())
            {
                foreach (var game in games)
                {
                    logger.Trace($"Removing HDR Exclusion tag from game {game.Name}");
                    game.TagIds?.RemoveAll(tagId => tagId == HdrExclusionTagId);
                    playniteApi.Database.Games.Update(game);
                }
            }
        }

        public Tag CreateOrUpdateHdrExclusionTag(string name)
        {
            Tag? tag = HdrExclusionTag;
            if (tag == null)
            {
                logger.Info("Creating HDR Exclusion tag");
                tag = new Tag(name)
                {
                    Id = HdrExclusionTagId
                };
                playniteApi.Database.Tags.Add(tag);
            }
            else
            {
                if (tag.Name != name)
                {
                    logger.Info("Updating HDR Exclusion tag name");
                    tag.Name = name;
                    playniteApi.Database.Tags.Update(tag);
                }
            }
            return tag;
        }

        private Tag? HdrExclusionTag
        {
            get
            {
                return playniteApi
                    .Database
                    .Tags
                    .FirstOrDefault(t => t.Id == HdrExclusionTagId);
            }
        }
    }
}
