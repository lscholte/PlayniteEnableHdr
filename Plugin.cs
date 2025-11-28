using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace HdrManager
{
    public class Plugin : GenericPlugin
    {
        #region Private Fields

        private static readonly ILogger logger = LogManager.GetLogger();

        private readonly PluginSettings pluginSettings;

        private readonly Guid HdrExclusionTagId = Guid.Parse("b7f2a9d3-4c1e-4a8b-9f6d-2e3c1a5d7b84");
        private readonly Guid PCGamingWikiPluginId = Guid.Parse("c038558e-427b-4551-be4c-be7009ce5a8d");
        private readonly List<string> HdrFeatures = new List<string>()
        {
            "HDR",
            "HDR Available"
        };

        #endregion

        #region Constructors

        public Plugin(IPlayniteAPI api) : base(api)
        {
            Properties = new GenericPluginProperties()
            {
                HasSettings = true
            };
            pluginSettings = new PluginSettings(this);
        }

        #endregion

        #region Public Properties

        public override Guid Id { get; } = Guid.Parse("b73b5b49-acdf-4da4-a2cc-b91d34d57c9a");

        #endregion

        #region Public Methods

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return pluginSettings;
        }

        public override UserControl GetSettingsView(bool firstRunView)
        {
            return new PluginSettingsView();
        }

        public override void OnApplicationStarted(OnApplicationStartedEventArgs args)
        {
            GetOrCreateTag(PlayniteApi.Resources.GetString("HdrManagerExclusionTag"));
            EnableSystemHdr();
            ShowPcGamingWikiWarning();
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

        #endregion

        #region Private Methods

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
                .Intersect(HdrFeatures)
                .Any();
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

        private void ShowPcGamingWikiWarning()
        {
            if (!pluginSettings.IsPCGamingWikiWarningSuppressed &&
                !PlayniteApi.Addons.Plugins.Any(plugin => plugin.Id == PCGamingWikiPluginId))
            {
                var okResponse = new MessageBoxOption(PlayniteApi.Resources.GetString("DialogResponseOK"), true, true);
                var suppressWarningResponse = new MessageBoxOption(PlayniteApi.Resources.GetString("DialogResponseSuppressWarning"));

                List<MessageBoxOption> options = new List<MessageBoxOption>()
                {
                    okResponse,
                    suppressWarningResponse
                };

                MessageBoxOption response = PlayniteApi.Dialogs.ShowMessage(
                    PlayniteApi.Resources.GetString("PCGamingWikiDialogWarningMessage"),
                    "",
                    MessageBoxImage.Warning,
                    options);
                if (response == suppressWarningResponse)
                {
                    pluginSettings.IsPCGamingWikiWarningSuppressed = true;
                    SavePluginSettings(pluginSettings);
                }
            }
        }

        #endregion
    }
}