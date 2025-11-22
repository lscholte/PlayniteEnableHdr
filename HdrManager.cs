using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HDRManager
{
    public class HdrManager : GenericPlugin
    {
        private static readonly ILogger logger = LogManager.GetLogger();

        private const string HdrExclusionTagName = "[HDR Manager] Excluded";
        private const string HdrManagerTitle = "HDR Manager";

        public override Guid Id { get; } = Guid.Parse("b73b5b49-acdf-4da4-a2cc-b91d34d57c9a");

        public HdrManager(IPlayniteAPI api) : base(api)
        {
        }

        public override void OnApplicationStarted(OnApplicationStartedEventArgs args)
        {
            EnableSystemHdr();
        }

        public override void OnLibraryUpdated(OnLibraryUpdatedEventArgs args)
        {
            EnableSystemHdr();
        }

        public override IEnumerable<GameMenuItem> GetGameMenuItems(GetGameMenuItemsArgs args)
        {
            if (args.Games.Any(game => HasHdrExclusionTag(game)))
            {
                yield return new GameMenuItem
                {
                    Description = "Remove HDR Exclusion Tag",
                    MenuSection = HdrManagerTitle,
                    Action = (a) =>
                    {
                        var hdrExclusionTag = GetOrCreateTag(HdrExclusionTagName);
                        using (PlayniteApi.Database.BufferedUpdate())
                        {
                            foreach (var game in a.Games)
                            {
                                logger.Trace($"Removing HDR Exclusion tag from game {game.Name}");
                                game.TagIds?.RemoveAll(tagId => tagId == hdrExclusionTag.Id);
                                PlayniteApi.Database.Games.Update(game);
                            }
                        }
                    }
                };
            }

            if (args.Games.Any(game => !HasHdrExclusionTag(game)))
            {
                yield return new GameMenuItem
                {
                    Description = "Add HDR Exclusion Tag",
                    MenuSection = HdrManagerTitle,
                    Action = (a) =>
                    {
                        var hdrExclusionTag = GetOrCreateTag(HdrExclusionTagName);
                        using (PlayniteApi.Database.BufferedUpdate())
                        {
                            foreach (var game in a.Games)
                            {
                                logger.Trace($"Adding HDR Exclusion tag to game {game.Name}");
                                AddTagToGame(game, hdrExclusionTag);
                                PlayniteApi.Database.Games.Update(game);
                            }
                        }
                    }
                };
            }
        }

        public override IEnumerable<MainMenuItem> GetMainMenuItems(GetMainMenuItemsArgs args)
        {
            yield return new MainMenuItem
            {
                Description = "Detect and Activate HDR",
                MenuSection = "@",
                Action = _ => EnableSystemHdr()
            };
        }

        private void EnableSystemHdr()
        {
            List<Game> filteredGames = PlayniteApi
                .Database
                .Games
                .Where(game => HasHdrFeature(game) && !HasHdrExclusionTag(game))
                .ToList();

            logger.Info($"Enabling System HDR for {filteredGames.Count} games");

            using (PlayniteApi.Database.BufferedUpdate())
            {
                foreach (var game in filteredGames)
                {
                    logger.Trace($"Enabling System HDR for game {game.Name}");
                    game.EnableSystemHdr = true;
                    PlayniteApi.Database.Games.Update(game);
                }
            }
        }

        private bool HasHdrFeature(Game game)
        {
            return game
                .Features
                .EmptyIfNull()
                .Select(feature => feature.Name)
                .Contains("HDR");
        }

        private bool HasHdrExclusionTag(Game game)
        {
            return game
                .Tags
                .EmptyIfNull()
                .Select(tag => tag.Name)
                .Contains(HdrExclusionTagName);
        }

        private void AddTagToGame(Game game, Tag tag)
        {
            if (game.TagIds == null)
            {
                game.TagIds = new List<Guid>() { tag.Id };
            }
            else
            {
                game.TagIds.AddMissing(tag.Id);
            }
        }

        private Tag GetOrCreateTag(string name)
        {
            Tag tag = PlayniteApi
               .Database
               .Tags
               .FirstOrDefault(t => t.Name == name);
            if (tag == null)
            {
                logger.Info("Creating HDR Exclusion tag");
                tag = PlayniteApi.Database.Tags.Add(name);
            }
            return tag;
        }
    }
}