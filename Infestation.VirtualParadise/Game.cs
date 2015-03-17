using System.Linq;
using Infestation.Engine;
using Infestation.Engine.Enums;
using Infestation.Engine.Logic.Results;
using Infestation.VirtualParadise.Entities;

namespace Infestation.VirtualParadise
{
    public class Game : Game<Player, Block>
    {
        public new void DistributePoints(bool traditional)
        {
            base.DistributePoints(traditional);

            PreProcessTotalPoints();
        }

        public new void Reset()
        {
            base.Reset();

            PreProcessTotalPoints();
        }

        public new void LoadState(string id)
        {
            base.LoadState(id);

            var changeSet = new RuleResult<Player, Block>
            {
                Type = ActionType.Reset
            };

            foreach (var player in Players.Values.Where(p => p.Online))
            {
                player.Online = false;
                changeSet.Updates.Add(new EntityUpdate<Player, Block>(player));
            }

            foreach (var block in Blocks.Values)
            {
                if (block.PlayerId.HasValue)
                {
                    var player = Players[block.PlayerId.Value];
                    block.Selected = false;
                    changeSet.Updates.Add(new EntityUpdate<Player, Block>(player, block, UpdateType.Block));
                }
                else
                {
                    block.Selected = false;
                    changeSet.Updates.Add(new EntityUpdate<Player, Block>(block));
                }
            }

            base.CommitChanges(changeSet);
            base.RaiseOnGameUpdate(changeSet);
        }

        private void PreProcessTotalPoints()
        {
            //Preprocess block stats.
            foreach (var player in Players.Values)
            {
                //Add all the points on the blocks.
                player.TotalPoints = Blocks.Values
                    .Where(block => block.PlayerId.HasValue && block.PlayerId.Value == player.Id)
                    .Sum(b => b.Points);

                //Add the available points.
                player.TotalPoints += player.Points;

                //Update
                AddOrUpdate(player);
            }
        }
    }
}
