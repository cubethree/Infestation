using System;
using System.Collections.Generic;
using System.Linq;
using Infestation.Engine.Entities.Interfaces;
using Infestation.Engine.Enums;
using Infestation.Engine.Logic.Results;

namespace Infestation.Engine.Logic
{
    internal static class Rules
    {
        public static RuleResult<TPlayer, TBlock> Apply<TPlayer, TBlock>(IDictionary<int, TBlock> boardArray, TPlayer player, TBlock block) 
            where TPlayer : IPlayer where TBlock : IBlock<TBlock>
        {
            var currentAction = GetActionByCurrentState(boardArray, player, block);
            var result = new RuleResult<TPlayer, TBlock>
            {
                Type = currentAction,
                Player = player
            };

            switch (currentAction)
            {
                case ActionType.Select:
                    block.Selected = true;
                    result.Updates.Add(new EntityUpdate<TPlayer, TBlock>(player, block, UpdateType.Block));
                    break;
                case ActionType.Deselect:
                    block.Selected = false;
                    result.Updates.Add(new EntityUpdate<TPlayer, TBlock>(player, block, UpdateType.Block));
                    break;
                case ActionType.Place:
                    var placePoints = Math.Max(Math.Min(player.Points, player.PointsPerMove), 1);
                    player.Points -= placePoints;
                    block.Points += placePoints;
                    block.PlayerId = player.Id;
                    result.Updates.Add(new EntityUpdate<TPlayer, TBlock>(player, block, UpdateType.Both));
                    break;
                case ActionType.Tranfer:
                    var transferSource = boardArray.Values.Single(o => o.PlayerId.HasValue && o.PlayerId.Value == player.Id && o.Selected);
                    var transferPoints = Math.Min(transferSource.Points - 1, Math.Max(player.PointsPerMove, 1));
                    transferSource.Points -= transferPoints;
                    block.Points += transferPoints;

                    //if one point left, deselect block.
                    if (transferSource.Points == 1)
                        transferSource.Selected = false;

                    result.Updates.Add(new EntityUpdate<TPlayer, TBlock>(player, transferSource, UpdateType.Block));
                    result.Updates.Add(new EntityUpdate<TPlayer, TBlock>(player, block, UpdateType.Block));
                    break;
                case ActionType.Attack:
                    var attackSource = boardArray.Values.Single(o => o.PlayerId.HasValue && o.PlayerId.Value == player.Id && o.Selected);
                    //Attack blok
                    attackSource.Points = attackSource.Points - (block.Points + 1);
                    block.PlayerId = player.Id;
                    block.Points = 1;
                    block.Selected = false;

                    //if one point left, deselect block.
                    if (attackSource.Points == 1)
                        attackSource.Selected = false;

                    result.Updates.Add(new EntityUpdate<TPlayer, TBlock>(player, attackSource, UpdateType.Block));
                    result.Updates.Add(new EntityUpdate<TPlayer, TBlock>(player, block, UpdateType.Block));
                    break;
            }

            return result;
        }

        private static ActionType GetActionByCurrentState<TPlayer, TBlock>(IDictionary<int, TBlock> boardArray, TPlayer sourcePlayer, TBlock targetBlock)
            where TPlayer : IPlayer where TBlock : IBlock<TBlock>
        {
            var alreadyPlaying = boardArray.Values.Any(b => b.PlayerId.HasValue && b.PlayerId.Value == sourcePlayer.Id);

            if (alreadyPlaying)
            {
                var selectedBlock = boardArray.Values.SingleOrDefault(o => o.PlayerId.HasValue && o.PlayerId.Value == sourcePlayer.Id && o.Selected);

                if (selectedBlock == null)
                {
                    var hasBlockInRange = boardArray.Values.Any(o => o.PlayerId.HasValue && o.PlayerId.Value == sourcePlayer.Id && targetBlock.InRange(o));

                    if (!hasBlockInRange)
                        return ActionType.None;

                    if (targetBlock.PlayerId.HasValue && targetBlock.PlayerId != sourcePlayer.Id)
                        return ActionType.None;

                    if (sourcePlayer.Points > 0)
                        return ActionType.Place;

                    if (targetBlock.PlayerId.HasValue && targetBlock.PlayerId.Value == sourcePlayer.Id && targetBlock.Points > 1)
                        return ActionType.Select;
                }
                else
                {
                    if (selectedBlock.Id == targetBlock.Id)
                        return ActionType.Deselect;

                    if (!selectedBlock.InRange(targetBlock))
                        return ActionType.Illegal;

                    if (targetBlock.PlayerId.HasValue && (targetBlock.PlayerId.Value == sourcePlayer.Id))
                        return selectedBlock.Points <= 1
                            ? ActionType.Illegal
                            : ActionType.Tranfer;

                    if (!targetBlock.PlayerId.HasValue || (targetBlock.PlayerId.Value != sourcePlayer.Id))
                        return selectedBlock.Points <= 1 || selectedBlock.Points <= (targetBlock.Points + 1)
                            ? ActionType.Illegal
                            : ActionType.Attack;
                }
            }
            else
            {
                if (targetBlock.PlayerId.HasValue) 
                    return ActionType.Illegal;
                
                if(sourcePlayer.Points > 0)
                    return ActionType.Place;
            }

            return ActionType.None;
        }
    }
}
