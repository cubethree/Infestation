using System;

namespace Infestation.Engine.Entities.Interfaces
{
    public interface IPlayer : ICloneable
    {
        int Id { get; set; }
        int Points { get; set; }
        int PointsPerMove { get; set; }
    }
}