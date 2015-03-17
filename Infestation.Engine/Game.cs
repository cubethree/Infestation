using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Infestation.Engine.Entities.Interfaces;
using Infestation.Engine.Enums;
using Infestation.Engine.Logic;
using Infestation.Engine.Logic.Results;
using Infestation.Engine.Stores;

namespace Infestation.Engine
{
    public abstract class Game<TPlayer, TBlock> where TPlayer : IPlayer where TBlock : IBlock<TBlock>
    {
        public delegate void GameUpdateHandler(object sender, RuleResult<TPlayer, TBlock> e);
        public event GameUpdateHandler OnGameUpdate;

        private readonly PlayerStore<TPlayer> _playerStore;
        private readonly BlockStore<TBlock> _blockStore;

        public Dictionary<int, TPlayer> Players
        {
            get { return _playerStore.Players.ToDictionary(pair => pair.Key, pair => (TPlayer) pair.Value.Clone()); }
        }

        public Dictionary<int, TBlock> Blocks
        {
            get { return _blockStore.Blocks.ToDictionary(pair => pair.Key, pair => (TBlock)pair.Value.Clone()); }
        }

        public bool GameOver
        {
            get
            {
                var randomBlock = Blocks.Values.FirstOrDefault(b => b.PlayerId.HasValue);
       
                if (randomBlock == null || !randomBlock.PlayerId.HasValue)
                    return false;
                    
                var randomPlayer = Players[randomBlock.PlayerId.Value];

                return Blocks.All(pair => pair.Value.PlayerId.HasValue && pair.Value.PlayerId.Value == randomPlayer.Id);
            }
        }

        protected Game()
        {
            this._playerStore = new PlayerStore<TPlayer>();
            this._blockStore = new BlockStore<TBlock>();
        }

        public void AddOrUpdate(TPlayer player)
        {
            _playerStore.AddOrUpdate((TPlayer)player.Clone());
        }

        public void AddOrUpdate(TBlock block)
        {
            _blockStore.AddOrUpdate((TBlock)block.Clone());                
        }

        public void Reset()
        {
            var changeSet = new RuleResult<TPlayer, TBlock>
            {
                Type = ActionType.Reset
            };

            foreach (var block in Blocks.Values.Where(b => b.PlayerId.HasValue))
            {
                block.PlayerId = null;
                block.Points = 0;
                block.Selected = false;

                changeSet.Updates.Add(new EntityUpdate<TPlayer, TBlock>(block));
            }

            foreach (var player in Players.Values)
            {
                player.Points = 10;
                player.PointsPerMove = 1;
                changeSet.Updates.Add(new EntityUpdate<TPlayer, TBlock>(player));
            }

            CommitChanges(changeSet);
            RaiseOnGameUpdate(changeSet);
        }

        public void DistributePoints(bool traditional = true)
        {
            var changeSet = new RuleResult<TPlayer, TBlock>
            {
                Type = ActionType.Distribution
            };

            if (traditional)
            {
                //Put points on the blocks.
                foreach (var block in Blocks.Values.Where(b => b.PlayerId.HasValue))
                {
                    TPlayer blockOwner;

                    if (!Players.TryGetValue(block.PlayerId.Value, out blockOwner))
                        continue;

                    block.Selected = false;
                    block.Points += 1;
                    changeSet.Updates.Add(new EntityUpdate<TPlayer, TBlock>(blockOwner, block, UpdateType.Block));
                }
            }
            else
            {
                //Give points to the player.
                foreach (var block in Blocks.Values.Where(b => b.Selected))
                {
                    if (block.PlayerId.HasValue)
                    {
                        var blockOwner = Players[block.PlayerId.Value];

                        block.Selected = false;

                        changeSet.Updates.Add(new EntityUpdate<TPlayer, TBlock>(blockOwner, block, UpdateType.Block));
                    }
                }

                foreach (var player in Players.Values)
                {
                    player.Points += Blocks.Values.Count(b => b.PlayerId.HasValue && b.PlayerId.Value == player.Id);

                    changeSet.Updates.Add(new EntityUpdate<TPlayer, TBlock>(player));
                }
            }

            CommitChanges(changeSet);
            RaiseOnGameUpdate(changeSet);
        }
        
        public void Execute(TPlayer player, TBlock block)
        {
            var changeSet = Rules.Apply(Blocks, (TPlayer)player.Clone(), (TBlock)block.Clone());

            CommitChanges(changeSet);
            RaiseOnGameUpdate(changeSet);
        }

        public void SaveState(string id)
        {
            if (!Directory.Exists("States"))
                Directory.CreateDirectory("States");

            var playerStatePath = String.Format("States/{0}_Players.bin", id);
            var blockStatePath = String.Format("States/{0}_Blocks.bin", id);

            _playerStore.Save(playerStatePath);
            _blockStore.Save(blockStatePath);
        }

        public void LoadState(string id)
        {
            if (!Directory.Exists("States"))
                throw new DirectoryNotFoundException("Data directory does not exist.");

            var playerStatePath = String.Format("States/{0}_Players.bin", id);
            var blockStatePath = String.Format("States/{0}_Blocks.bin", id);

            if (!File.Exists(playerStatePath) || !File.Exists(blockStatePath))
                throw new FileNotFoundException("State does not exist.");

            _playerStore.Load(playerStatePath);
            _blockStore.Load(blockStatePath);
        }

        public bool ContainsState(string id)
        {
            if (!Directory.Exists("States"))
                return false;

            var playerStatePath = String.Format("States/{0}_Players.bin", id);
            var blockStatePath = String.Format("States/{0}_Blocks.bin", id);

            return File.Exists(playerStatePath) && File.Exists(blockStatePath);
        }

        protected void CommitChanges(RuleResult<TPlayer, TBlock> ruleResult)
        {
            foreach (var entityUpdate in ruleResult.Updates)
            {
                switch (entityUpdate.Type)
                {
                    case UpdateType.Both:
                        _playerStore.AddOrUpdate(entityUpdate.Player);
                        _blockStore.AddOrUpdate(entityUpdate.Block);
                        break;
                    case UpdateType.Player:
                        _playerStore.AddOrUpdate(entityUpdate.Player);
                        break;
                    case UpdateType.Block:
                        _blockStore.AddOrUpdate(entityUpdate.Block);
                        break;
                }
            }
        }

        protected void RaiseOnGameUpdate(RuleResult<TPlayer, TBlock> e)
        {
            var  handler = OnGameUpdate;

            if (handler != null) handler(this, e);
        }
    }
}
