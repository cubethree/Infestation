using System.Collections.Concurrent;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Infestation.Engine.Entities.Interfaces;

namespace Infestation.Engine.Stores
{
    internal class PlayerStore<TPlayer> where TPlayer : IPlayer
    {
        public ConcurrentDictionary<int, TPlayer> Players { get; private set; }

        public PlayerStore()
        {
            Players = new ConcurrentDictionary<int, TPlayer>();
        }

        public void AddOrUpdate(TPlayer updatePlayer)
        {
            Players.AddOrUpdate(updatePlayer.Id, updatePlayer, (i, player) => updatePlayer);
        }

        public void Load(string filePath)
        {
            using (var fs = File.Open(filePath, FileMode.Open))
            {
                var formatter = new BinaryFormatter();
                var obj = formatter.Deserialize(fs);
                Players = (ConcurrentDictionary<int, TPlayer>)obj;
            }
        }

        public void Save(string filePath)
        {
            using (var ms = File.OpenWrite(filePath))
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, Players);
            }
        }
    }
}