using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HdrManager
{
    public class HdrManager : GenericPlugin
    {
        private static readonly ILogger logger = LogManager.GetLogger();

        private Guid HdrExclusionTagId = new Guid("b7f2a9d3-4c1e-4a8b-9f6d-2e3c1a5d7b84");

        public override Guid Id { get; } = Guid.Parse("b73b5b49-acdf-4da4-a2cc-b91d34d57c9a");

        public HdrManager(IPlayniteAPI api) : base(api)
        {
        }

        public override void OnApplicationStarted(OnApplicationStartedEventArgs args)
        {
            GetOrCreateTag(PlayniteApi.Resources.GetString("HdrManagerExclusionTag"));
            EnableSystemHdr();
        }

        public override void OnLibraryUpdated(OnLibraryUpdatedEventArgs args)
        {
            EnableSystemHdr();
        }

        public override IEnumerable<GameMenuItem> GetGameMenuItems(GetGameMenuItemsArgs args)
        {
            if (args.Games.All(game => HasHdrExclusionTag(game)))
            {
                yield return new GameMenuItem
                {
                    Description = PlayniteApi.Resources.GetString("ContextMenuRemoveExclusionTag"),
                    MenuSection = PlayniteApi.Resources.GetString("ContextMenuSectionHeader"),
                    Action = (a) =>
                    {
                        using (PlayniteApi.Database.BufferedUpdate())
                        {
                            foreach (var game in a.Games)
                            {
                                logger.Trace($"Removing HDR Exclusion tag from game {game.Name}");
                                game.TagIds?.RemoveAll(tagId => tagId == HdrExclusionTagId);
                                PlayniteApi.Database.Games.Update(game);
                            }
                        }
                    }
                };
            }
            else
            {
                yield return new GameMenuItem
                {
                    Description = PlayniteApi.Resources.GetString("ContextMenuAddExclusionTag"),
                    MenuSection = PlayniteApi.Resources.GetString("ContextMenuSectionHeader"),
                    Action = (a) =>
                    {
                        var hdrExclusionTag = GetOrCreateTag(PlayniteApi.Resources.GetString("HdrManagerExclusionTag"));
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

            if (args.Games.All(game => game.EnableSystemHdr))
            {
                yield return new GameMenuItem
                {
                    Description = PlayniteApi.Resources.GetString("ContextMenuDisableHdrSupport"),
                    MenuSection = PlayniteApi.Resources.GetString("ContextMenuSectionHeader"),
                    Action = (a) =>
                    {
                        using (PlayniteApi.Database.BufferedUpdate())
                        {
                            foreach (var game in a.Games)
                            {
                                logger.Trace($"Disabling HDR support for game {game.Name}");
                                game.EnableSystemHdr = false;
                                PlayniteApi.Database.Games.Update(game);
                            }
                        }
                    }
                };
            }
            else
            {
                yield return new GameMenuItem
                {
                    Description = PlayniteApi.Resources.GetString("ContextMenuEnableHdrSupport"),
                    MenuSection = PlayniteApi.Resources.GetString("ContextMenuSectionHeader"),
                    Action = (a) =>
                    {
                        using (PlayniteApi.Database.BufferedUpdate())
                        {
                            foreach (var game in a.Games)
                            {
                                logger.Trace($"Enabling HDR support for game {game.Name}");
                                game.EnableSystemHdr = true;
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
                Description = PlayniteApi.Resources.GetString("ExtensionMenuRunHdrActivation"),
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
                .Select(tag => tag.Id)
                .Contains(HdrExclusionTagId);
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
               .FirstOrDefault(t => t.Id == HdrExclusionTagId);
            if (tag == null)
            {
                logger.Info("Creating HDR Exclusion tag");
                tag = new Tag(name)
                {
                    Id = HdrExclusionTagId
                };
                PlayniteApi.Database.Tags.Add(tag);
            }
            else
            {
                if (tag.Name != name)
                {
                    logger.Info("Updating HDR Exclusion tag name");
                    tag.Name = name;
                    PlayniteApi.Database.Tags.Update(tag);
                }
            }
                return tag;
        }
    }
}