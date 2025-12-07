using Playnite.SDK.Models;
using System;
using System.Linq;

namespace HdrManager.Test.Helpers
{
    public class GameBuilder
    {
        private readonly Game _game;

        public GameBuilder()
        {
            _game = new Game();
        }

        public GameBuilder WithName(string name)
        {
            _game.Name = name;
            return this;
        }

        public GameBuilder WithEnableSystemHdr(bool enableSystemHdr)
        {
            _game.EnableSystemHdr = enableSystemHdr;
            return this;
        }

        public GameBuilder WithTagIds(params Guid[] tagIds)
        {
            _game.TagIds = tagIds.ToList();
            return this;
        }

        public GameBuilder WithFeatureIds(params Guid[] featureIds)
        {
            _game.FeatureIds = featureIds.ToList();
            return this;
        }

        public Game Build()
        {
            return _game;
        }
    }
}
