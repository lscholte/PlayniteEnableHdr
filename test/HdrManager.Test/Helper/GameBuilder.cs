using Playnite;
using System;
using System.Linq;

namespace HdrManager.Test.Helper
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

        public GameBuilder WithTagIds(params string[] tagIds)
        {
            _game.TagIds = tagIds.ToHashSet();
            return this;
        }

        public GameBuilder WithFeatureIds(params string[] featureIds)
        {
            _game.FeatureIds = featureIds.ToHashSet();
            return this;
        }

        public Game Build()
        {
            return _game;
        }
    }
}
