using HdrManager.Extension;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace HdrManager
{
    public class SystemHdrManager : ISystemHdrManager
    {
        private static readonly ILogger _logger = LogManager.GetLogger();

        private static readonly string[] _hdrFeatureToken =
        [
            "HDR",
            "High Dynamic Range",
            "H D R"
        ];

        private static readonly string[] _negationTokens =
        [
            "No",
            "Not",
            "Without",
            "Disable",
            "Disabled"
        ];

        private readonly IPlayniteAPI _playniteApi;

        public SystemHdrManager(IPlayniteAPI playniteApi)
        {
            _playniteApi = playniteApi;
        }

        public static Guid HdrExclusionTagId { get; } = Guid.Parse("b7f2a9d3-4c1e-4a8b-9f6d-2e3c1a5d7b84");

        public void EnableSystemHdrForManagedGames()
        {
            IEnumerable<Guid> hdrFeatureIds =
                _playniteApi
                    .Database
                    .Features
                    .Where(f => IsHdrFeature(f.Name))
                    .Select(f => f.Id)
                    .ToList();

            List<Game> managedHdrGames =
                _playniteApi
                    .Database
                    .Games
                    .Where(game => game.HasAnyFeature(hdrFeatureIds) && !game.HasTag(HdrExclusionTagId))
                    .ToList();

            _logger.Info($"Enabling System HDR for {managedHdrGames.Count} games");
            SetSystemHdrForGames(managedHdrGames, true);
        }

        public void SetSystemHdrForGames(IEnumerable<Game> games, bool enableSystemHdr)
        {
            using (_playniteApi.Database.BufferedUpdate())
            {
                foreach (var game in games)
                {
                    _logger.Trace($"Setting EnableSystemHdr for game {game.Name} to {enableSystemHdr}");
                    game.EnableSystemHdr = enableSystemHdr;
                    _playniteApi.Database.Games.Update(game);
                }
            }
        }

        public void AddHdrExclusionTagToGames(IEnumerable<Game> games)
        {
            using (_playniteApi.Database.BufferedUpdate())
            {
                foreach (var game in games)
                {
                    _logger.Trace($"Adding HDR Exclusion tag to game {game.Name}");
                    game.AddTag(HdrExclusionTagId);
                    _playniteApi.Database.Games.Update(game);
                }
            }
        }

        public void RemoveHdrExclusionTagFromGames(IEnumerable<Game> games)
        {
            using (_playniteApi.Database.BufferedUpdate())
            {
                foreach (var game in games)
                {
                    _logger.Trace($"Removing HDR Exclusion tag from game {game.Name}");
                    game.TagIds?.RemoveAll(tagId => tagId == HdrExclusionTagId);
                    _playniteApi.Database.Games.Update(game);
                }
            }
        }

        public Tag CreateOrUpdateHdrExclusionTag(string name)
        {
            Tag? tag = HdrExclusionTag;
            if (tag == null)
            {
                _logger.Info("Creating HDR Exclusion tag");
                tag = new Tag(name)
                {
                    Id = HdrExclusionTagId
                };
                _playniteApi.Database.Tags.Add(tag);
            }
            else
            {
                if (tag.Name != name)
                {
                    _logger.Info("Updating HDR Exclusion tag name");
                    tag.Name = name;
                    _playniteApi.Database.Tags.Update(tag);
                }
            }
            return tag;
        }

        private Tag? HdrExclusionTag
        {
            get
            {
                return _playniteApi
                    .Database
                    .Tags
                    .FirstOrDefault(t => t.Id == HdrExclusionTagId);
            }
        }

        private static bool IsHdrFeature(string featureName)
        {
            if (string.IsNullOrWhiteSpace(featureName))
            {
                return false;
            }

            static string ToWordBoundaryPattern(string token) => $@"\b{Regex.Escape(token)}\b";

            var hdrPattern = string.Join("|", _hdrFeatureToken.Select(ToWordBoundaryPattern));
            bool hasHdr = Regex.IsMatch(featureName, hdrPattern, RegexOptions.IgnoreCase);
            if (!hasHdr)
            {
                return false;
            }

            var negationPattern = string.Join("|", _negationTokens.Select(ToWordBoundaryPattern));
            var isNegated = Regex.IsMatch(featureName, negationPattern, RegexOptions.IgnoreCase);
            return !isNegated;
        }
    }
}
