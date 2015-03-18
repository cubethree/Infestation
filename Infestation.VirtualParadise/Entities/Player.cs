using System;
using Infestation.Engine.Entities.Interfaces;
using VpNet;
using VpNet.Extensions;

namespace Infestation.VirtualParadise.Entities
{
    [Serializable]
    public class Player : IPlayer
    {
        //Interface Properties
        public int Id { get; set; }

        public int Points { get; set; }

        public int PointsPerMove { get; set; }

        //Custom Properties
        public bool Online { get; set; }
        public string Color { get; set; }
        public Guid WebOverlayId { get; set; }

        public Avatar<Vector3> VpAvatar { get; set; }
        public int TotalPoints { get; set; }

        public object Clone()
        {
            return this.Copy();
        }
    }
}
