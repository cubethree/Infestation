using System.Collections.Concurrent;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Infestation.Engine.Entities.Interfaces;

namespace Infestation.Engine.Stores
{
    internal class BlockStore<TBlock> where TBlock : IBlock<TBlock>
    {
        public ConcurrentDictionary<int, TBlock> Blocks { get; private set; }
        
        public BlockStore()
        {
            Blocks = new ConcurrentDictionary<int, TBlock>();
        }

        public void AddOrUpdate(TBlock updateBlock)
        {
            Blocks.AddOrUpdate(updateBlock.Id, updateBlock, (i, block) => updateBlock);
        }

        public void Load(string filePath)
        {
            using (var fs = File.Open(filePath, FileMode.Open))
            {
                var formatter = new BinaryFormatter();
                var obj = formatter.Deserialize(fs);
                Blocks = (ConcurrentDictionary<int, TBlock>)obj;
            }
        }

        public void Save(string filePath)
        {
            using (var ms = File.OpenWrite(filePath))
            {
                var formatter = new BinaryFormatter();

                formatter.Serialize(ms, Blocks);
            }
        }
    }
}