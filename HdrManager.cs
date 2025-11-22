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

        public override IEnumerable<MainMenuItem> GetMainMenuItems(GetMainMenuItemsArgs args)
        {
            yield return new MainMenuItem
            {
                Description = "Detect and Activate HDR",
                Action = _ => EnableSystemHdr(),
                MenuSection = "@"
            };
        }

        private void EnableSystemHdr()
        {
            List<Game> filteredGames = PlayniteApi
                .Database
                .Games
                .Where(game => HasHdrFeature(game))
                .ToList();

            logger.Info($"Enabling System HDR for {filteredGames.Count} games");

            foreach (var game in filteredGames)
            {
                logger.Debug($"Enabling System HDR for game {game.Name}");
                game.EnableSystemHdr = true;
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

    }
}