using Playnite.SDK.Models;
using System;
using System.Linq;

namespace HdrManager.Test.Helpers
{
    public class GameBuilder
    {
        private readonly Game game;

        public GameBuilder()
        {
            game = new Game();
        }

        public GameBuilder WithName(string name)
        {
            game.Name = name;
            return this;
        }

        public GameBuilder WithEnableSystemHdr(bool enableSystemHdr)
        {
            game.EnableSystemHdr = enableSystemHdr;
            return this;
        }

        public GameBuilder WithTagIds(params Guid[] tagIds)
        {
            game.TagIds = tagIds.ToList();
            return this;
        }

        public GameBuilder WithFeatureIds(params Guid[] featureIds)
        {
            game.FeatureIds = featureIds.ToList();
            return this;
        }

        public Game Build()
        {
            return game;
        }
    }
}
