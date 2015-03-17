using System;

namespace Infestation.Engine.Entities.Interfaces
{
    public interface IBlock<in TBlock> : ICloneable
    {
        int Id { get; set; }
        int? PlayerId { get; set; }
        int Points { get; set; }
        bool Selected { get; set; }
        float X { get; set; }
        float Y { get; set; }
        float Z { get; set; }
        bool InRange(TBlock second);
    }
}