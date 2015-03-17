using System.Collections.Concurrent;
using Infestation.Engine.Entities.Interfaces;
using Infestation.Engine.Enums;

namespace Infestation.Engine.Logic.Results
{
    public class RuleResult<TPlayer, TBlock>
        where TPlayer : IPlayer
        where TBlock : IBlock<TBlock>
    {
        public ActionType Type { get; set; }
        public TPlayer Player { get; set; }

        public ConcurrentBag<EntityUpdate<TPlayer, TBlock>> Updates { get; private set; }

        public RuleResult()
        {
            Updates = new ConcurrentBag<EntityUpdate<TPlayer, TBlock>>();
        }
    }
}