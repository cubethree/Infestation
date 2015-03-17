using Infestation.Engine.Entities.Interfaces;
using Infestation.Engine.Enums;

namespace Infestation.Engine.Logic.Results
{
    public class EntityUpdate<TPlayer, TBlock> where TPlayer : IPlayer
        where TBlock : IBlock<TBlock>
    {
        public UpdateType Type { get; private set; }
        public TPlayer Player { get; private set; }
        public TBlock Block { get; private set; }

        public EntityUpdate(TPlayer player, TBlock block, UpdateType type)
        {
            Type = type;
            this.Player = player;
            this.Block = block;
        }

        public EntityUpdate(TPlayer player)
        {
            Type = UpdateType.Player;
            this.Player = player;
        }

        public EntityUpdate(TBlock block)
        {
            Type = UpdateType.Block;
            this.Block = block;
        }
    }
}