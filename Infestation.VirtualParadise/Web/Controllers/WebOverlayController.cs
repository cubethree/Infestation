using System;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Cors;
using Infestation.VirtualParadise.Configuration;
using Infestation.VirtualParadise.Web.Models;

namespace Infestation.VirtualParadise.Web.Controllers
{
    public class WebOverlayController : ApiController
    {
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public WebOverlayModel Get(Guid id)
        {
            var player = Program.Game.Players.Values.FirstOrDefault(p => p.WebOverlayId == id);

            if (player == null)
                return new WebOverlayModel();

            var lastDistribution = Program.LastPointDistribution;
            var newDistribution = lastDistribution.AddMilliseconds(GameSettings.DistributionInterval);
            var timeLeft = (int)newDistribution.Subtract(DateTime.Now).TotalSeconds;

            return new WebOverlayModel()
            {
                Points = player.Points,
                PointsPerMove = player.PointsPerMove.ToString("D3"),
                TotalPoints = player.TotalPoints,
                Time = timeLeft,
                GameMode = GameSettings.TraditionalGameplay
                    ? "Traditional"
                    : "Fast Play"
            };
        }

        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public WebOverlayModel Put(Guid id, int session, int mode)
        {
            var player = Program.Game.Players.Values.FirstOrDefault(p => p.WebOverlayId == id && p.VpAvatar.Session == session);

            if (player == null)
                return new WebOverlayModel();

            switch (mode)
            {
                case 1:
                    player.PointsPerMove += 1;
                    break;
                case 2:
                    player.PointsPerMove -= 1;
                    break;
                case 3:
                    player.PointsPerMove /= 2;
                    break;
                case 4:
                    player.PointsPerMove *= 2;
                    break;
            }

            player.PointsPerMove = Math.Min(999, Math.Max(player.PointsPerMove, 1));
            Program.Game.AddOrUpdate(player);

            return new WebOverlayModel()
            {
                PointsPerMove = player.PointsPerMove.ToString("D3")
            };
        }
    }
}
