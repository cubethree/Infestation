using System;
using Infestation.Engine.Entities.Interfaces;
using VpNet;
using VpNet.Extensions;

namespace Infestation.VirtualParadise.Entities
{
    [Serializable]
    public class Block : IBlock<Block>
    {
        //Interface Properties & Methods
        public int Id { get; set; }

        public int? PlayerId { get; set; }

        public int Points { get; set; }

        public bool Selected { get; set; }

        public float X { get; set; }

        public float Y { get; set; }

        public float Z { get; set; }

        public bool InRange(Block second)
        {
            var xd = Math.Abs(this.X - second.X);
            var yd = Math.Abs(this.Y - second.Y);
            var zd = Math.Abs(this.Z - second.Z);

            return Math.Sqrt(xd * xd + yd * yd + zd * zd) < 0.11;
        }

        //Custom Properties
        public VpObject<Vector3> VpObject { get; set; }
 
        public object Clone()
        {
            return new Block
            {
                Id = this.Id,
                PlayerId = this.PlayerId,
                Points = this.Points,
                Selected = this.Selected,
                X = this.X,
                Y = this.Y,
                Z = this.Z,

                VpObject = this.VpObject.Copy()
            };
        }
    }
}
