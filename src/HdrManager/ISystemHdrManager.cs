using Playnite;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HdrManager
{
    public interface ISystemHdrManager
    {
        Task EnableSystemHdrForManagedGames();

        Task SetSystemHdrForGames(IEnumerable<Game> games, bool enableSystemHdr);

        Task AddHdrExclusionTagToGames(IEnumerable<Game> games);

        Task RemoveHdrExclusionTagFromGames(IEnumerable<Game> games);

        Task<Tag> CreateOrUpdateHdrExclusionTag(string name);
    }
}
