using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HdrManager
{
    public interface ISystemHdrManager
    {
        void EnableSystemHdrForManagedGames();

        void SetSystemHdrForGames(IEnumerable<Game> games, bool enableSystemHdr);

        void AddHdrExclusionTagToGames(IEnumerable<Game> games);

        void RemoveHdrExclusionTagFromGames(IEnumerable<Game> games);

        Tag CreateOrUpdateHdrExclusionTag(string name);
    }
}
