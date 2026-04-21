using HdrManager.Extension;
using Playnite;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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

        private readonly IPlayniteApi _playniteApi;

        public SystemHdrManager(IPlayniteApi playniteApi)
        {
            _playniteApi = playniteApi;
        }

        public static string HdrExclusionTagId { get; } = "b7f2a9d3-4c1e-4a8b-9f6d-2e3c1a5d7b84";

        public async Task EnableSystemHdrForManagedGames()
        {
            IEnumerable<string> hdrFeatureIds =
                _playniteApi
                    .Library
                    .Features
                    .Where(f => IsHdrFeature(f.Name))
                    .Select(f => f.Id)
                    .ToList();

            List<Game> managedHdrGames =
                _playniteApi
                    .Library
                    .Games
                    .Where(game => game.HasAnyFeature(hdrFeatureIds) && !game.HasTag(HdrExclusionTagId))
                    .ToList();

            _logger.Info($"Enabling System HDR for {managedHdrGames.Count} games");
            await SetSystemHdrForGames(managedHdrGames, true);
        }

        public async Task SetSystemHdrForGames(IEnumerable<Game> games, bool enableSystemHdr)
        {
            foreach (var game in games)
            {
                _logger.Trace($"Setting EnableSystemHdr for game {game.Name} to {enableSystemHdr}");
                game.EnableSystemHdr = enableSystemHdr;
            }

            await _playniteApi.Library.Games.MakeBulkChangesAsync([], games, []);
        }

        public async Task AddHdrExclusionTagToGames(IEnumerable<Game> games)
        {
            foreach (var game in games)
            {
                _logger.Trace($"Adding HDR Exclusion tag to game {game.Name}");
                game.AddTag(HdrExclusionTagId);
            }

            await _playniteApi.Library.Games.MakeBulkChangesAsync([], games, []);
        }

        public async Task RemoveHdrExclusionTagFromGames(IEnumerable<Game> games)
        {
            foreach (var game in games)
            {
                _logger.Trace($"Removing HDR Exclusion tag from game {game.Name}");
                game.TagIds?.Remove(HdrExclusionTagId);
            }

            await _playniteApi.Library.Games.MakeBulkChangesAsync([], games, []);
        }

        public async Task<Tag> CreateOrUpdateHdrExclusionTag(string name)
        {
            Tag? tag = HdrExclusionTag;
            if (tag == null)
            {
                _logger.Info("Creating HDR Exclusion tag");
                tag = new Tag(name)
                {
                    Id = HdrExclusionTagId
                };

                await _playniteApi.Library.Tags.AddAsync(tag);
            }
            else
            {
                if (tag.Name != name)
                {
                    _logger.Info("Updating HDR Exclusion tag name");
                    tag.Name = name;
                    await _playniteApi.Library.Tags.UpdateAsync(tag);
                }
            }
            return tag;
        }

        private Tag? HdrExclusionTag
        {
            get
            {
                return _playniteApi
                    .Library
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
