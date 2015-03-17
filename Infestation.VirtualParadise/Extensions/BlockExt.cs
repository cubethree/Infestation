using System;
using Infestation.VirtualParadise.Entities;
using VpNet;

namespace Infestation.VirtualParadise.Extensions
{
    public static class BlockExt
    {
        public static void Visualize(this Block block, Instance sender, Player player = null)
        {
            if (player == null)
            {
                block.VpObject.Description = "";
                block.VpObject.Action = "create color white, sign";
            }
            else
            {
                block.VpObject.Description = String.Format("{0}{1}({2})", player.VpAvatar.Name, Environment.NewLine, block.Points);
                block.VpObject.Action = block.Selected
                    ? String.Format("create color {0}, sign bcolor=white color=white shadow", player.Color)
                    : String.Format("create color white, sign bcolor={0} color=white shadow", player.Color);
            }

            sender.ChangeObject(block.VpObject);
        }
    }
}
