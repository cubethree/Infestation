using System;
using System.Linq;
using System.Threading;
using Infestation.Engine.Enums;
using Infestation.Engine.Logic.Results;
using Infestation.VirtualParadise.Configuration;
using Infestation.VirtualParadise.Entities;
using Infestation.VirtualParadise.Extensions;
using Infestation.VirtualParadise.Web;
using VpNet;
using VpNet.Extensions;

namespace Infestation.VirtualParadise
{
    public static class Program
    {
        private static readonly Random Rng = new Random();
        public static DateTime LastPointDistribution;
        public static Game Game;
        private static Server _server;
        private static Instance _client;

        private static bool _running;
        private static bool _ignoreInput;
        private static int _cellsToQueried;

        static void Main(string[] args)
        {
            Game = new Game();
            Game.OnGameUpdate += GameOnOnGameUpdate;

            Console.WriteLine("{0} - Starting Web Api Server...", DateTime.Now.ToLongTimeString());

            _server = new Server(GameSettings.WebOverlayServerBinding); 

            _client = new Instance();
            _client.Connect(LoginSettings.ServerAddress, LoginSettings.ServerPort);
            _client.OnAvatarEnter += Client_OnAvatarEnter;
            _client.OnAvatarLeave += Client_OnAvatarLeave;
            _client.OnObjectCreate += Client_OnObjectCreate;
            _client.OnObjectClick += Client_OnObjectClick;
            _client.OnQueryCellEnd += Client_OnQueryCellEnd;
            _client.OnWorldDisconnect += delegate {
                Game.SaveState("main");
                _running = false;
            };

            Console.WriteLine("{0} - Logging in bot...", DateTime.Now.ToLongTimeString());

            _client.Login(LoginSettings.Username, LoginSettings.Password, "GameMaster");
            _client.Enter(LoginSettings.World);
            _client.LoginAndEnter();

            //Try load the previous state.
            if (Game.ContainsState("main"))
            {
                Console.WriteLine("{0} - Restoring game state...", DateTime.Now.ToLongTimeString());

                Game.LoadState("main");
            }
            else
            {
                Console.WriteLine("{0} - Querying world...", DateTime.Now.ToLongTimeString());
                //Query objects.
                for (var x = -5; x < 5; ++x)
                {
                    for (var z = -5; z < 5; ++z)
                    {
                        _client.QueryCell(x, z);
                    }
                }

                while (_cellsToQueried != 100)
                {
                    //Wait till query is done.
                    _client.Wait();
                }
            }

            Console.WriteLine("{0} - Found {1} blocks.", DateTime.Now.ToLongTimeString(), Game.Blocks.Count());
            Console.WriteLine("{0} - Starting Point Distribution...", DateTime.Now.ToLongTimeString());

            //Set boolean to running to handle user events.
            _running = true;

            //Start the web server.
            _server.Start();

            //Start the point distribution loop.
            new Thread(PointDistributionLoop)
            {
                IsBackground = true
            }.Start();

            //Start the persistence loop.
            new Thread(PersistenceLoop)
            {
                IsBackground = true
            }.Start();

            Console.WriteLine("{0} - Started...", DateTime.Now.ToLongTimeString());

            while (_running)
            {
                _client.Wait();
            }
        }

        private static void PersistenceLoop()
        {
            while (_running)
            {
                Thread.Sleep(GameSettings.PersistenseInterval);

                _ignoreInput = true;
                Game.SaveState("main");
                _ignoreInput = false;
            }
        }

        private static void PointDistributionLoop()
        {
            while (_running)
            {
                _ignoreInput = true;
                if (Game.GameOver)
                {
                    var randomBlock = Game.Blocks.Values.First(b => b.PlayerId.HasValue);
                    var player = Game.Players[randomBlock.PlayerId.Value];

                    _client.ConsoleMessage(DateTime.Now.ToLongTimeString() + " - We have a winner!",
                        new Color(0, 255, 0), TextEffectTypes.BoldItalic);
                    _client.ConsoleMessage(DateTime.Now.ToLongTimeString() + " - Congratulations " + player.VpAvatar.Name + "!",
                        new Color(0, 255, 0), TextEffectTypes.BoldItalic);
                    _client.ConsoleMessage(DateTime.Now.ToLongTimeString() + " - The game will restart in 1 minute.",
                        new Color(0, 255, 0), TextEffectTypes.BoldItalic);

                    Thread.Sleep(60000);

                    Game.Reset();
                }
                else
                {
                    Game.DistributePoints(GameSettings.TraditionalGameplay);
                }

                _ignoreInput = false;

                LastPointDistribution = DateTime.Now;
                Thread.Sleep(GameSettings.DistributionInterval);
            }
        }

        private static void GameOnOnGameUpdate(object sender, RuleResult<Player, Block> ruleResult)
        {
            switch (ruleResult.Type)
            {
                case ActionType.None:
                case ActionType.Illegal:
                    _client.ConsoleMessage(ruleResult.Player.VpAvatar, "GameMaster", "This move is not possible...", new Color(0, 0, 255), TextEffectTypes.Italic);
                    return;
                case ActionType.Distribution:
                    foreach (var player in Game.Players.Values.Where(p => p.Online && Game.Blocks.Values.Any(b => b.PlayerId.HasValue && b.PlayerId == p.Id)))
                        _client.ConsoleMessage(player.VpAvatar, "GameMaster", "New points have have arrived...", new Color(0, 255, 0), TextEffectTypes.BoldItalic);                    
                    break;
                case ActionType.Reset:
                    _client.ConsoleMessage(DateTime.Now.ToLongTimeString() + " - A new game as been started...", new Color(0, 255, 0), TextEffectTypes.BoldItalic);
                    break;
            }

            VisualizeUpdates(_client, ruleResult);
        }

        private static void VisualizeUpdates(Instance sender, RuleResult<Player, Block> ruleResult)
        {
            lock (_client)
            {
                foreach (var executionUpdate in ruleResult.Updates)
                {
                    switch (executionUpdate.Type)
                    {
                        case UpdateType.Both:
                        case UpdateType.Block:
                            executionUpdate.Block.Visualize(sender, executionUpdate.Player);
                            break;
                    }
                }
            }
        }

        private static void Client_OnObjectClick(Instance sender, ObjectClickArgsT<Avatar<Vector3>, VpObject<Vector3>, Vector3> args)
        {
            if (!_running) return;
            if (_ignoreInput) return;
            if (args.Avatar.IsBot) return;

            Block block;

            var player = Game.Players.Values.SingleOrDefault(p => p.Id == args.Avatar.UserId && p.VpAvatar.Session == args.Avatar.Session);

            if (player != null && Game.Blocks.TryGetValue(args.VpObject.Id, out block))
            {
                if (block.VpObject.Model != GameSettings.BlockModel) return;
                if (block.VpObject.Owner != GameSettings.BlockOwner) return;

                Game.Execute(player, block);
            }
        }

        private static void Client_OnObjectCreate(Instance sender, ObjectCreateArgsT<Avatar<Vector3>, VpObject<Vector3>, Vector3> args)
        {
            if (args.VpObject.Model != GameSettings.BlockModel) return;
            if (args.VpObject.Owner != GameSettings.BlockOwner) return;

            if (!Game.Blocks.ContainsKey(args.VpObject.Id))
            {
                var block = new Block
                {
                    Id = args.VpObject.Id,
                    Points = 0,
                    Selected = false,
                    X = args.VpObject.Position.X,
                    Y = args.VpObject.Position.Y,
                    Z = args.VpObject.Position.Z,
                    VpObject = args.VpObject.Copy()
                };

                Game.AddOrUpdate(block);
                block.Visualize(sender);
            }
        }

        private static void Client_OnAvatarLeave(Instance sender, AvatarLeaveEventArgsT<Avatar<Vector3>, Vector3> args)
        {
            if (args.Avatar.IsBot) return;

            var player = Game.Players.Values.SingleOrDefault(p => p.Id == args.Avatar.UserId && p.VpAvatar.Session == args.Avatar.Session);

            if (player != null)
            {
                //Deselect block if any.
                var selectedBlock = Game.Blocks.Values.SingleOrDefault(b => b.PlayerId.HasValue && b.PlayerId == player.Id &&  b.Selected);
                if (selectedBlock != null)
                {
                    selectedBlock.Selected = false;
                    Game.AddOrUpdate(selectedBlock);
                }

                player.Online = false;
                Game.AddOrUpdate(player);
            }
        }

        private static void Client_OnAvatarEnter(Instance sender, AvatarEnterEventArgsT<Avatar<Vector3>, Vector3> args)
        {
            if (args.Avatar.IsBot) return;

            Player player;
            var isObserver = false;
            if (Game.Players.TryGetValue(args.Avatar.UserId, out player))
            {
                if (player.Online)
                {
                    _client.ConsoleMessage(args.Avatar, "GameMaster", "We have detected that you are already in this world, you will not be able to play with this instance.", new Color(255, 0, 0), TextEffectTypes.BoldItalic);
                    isObserver = true;
                }
                else
                {
                    player.VpAvatar = args.Avatar.Copy();
                    player.Online = true;
                    player.WebOverlayId = Guid.NewGuid();

                    Game.AddOrUpdate(player);
                }
            }
            else
            {
                player = new Player
                {
                    Id = args.Avatar.UserId,
                    Points = 10,
                    PointsPerMove = 1,
                    Color = String.Format("{0:X6}", Rng.Next(0x1000000)),
                    Online = true,
                    WebOverlayId = Guid.NewGuid(),
                    VpAvatar = args.Avatar.Copy()
                };

                //Register player.
                Game.AddOrUpdate(player);
            }

            //Send hud.
            var overlayUrl = String.Format("{0}?webOverlayId={1}&avatarSession={2}&isObserver={3}&callbackUrl={4}", GameSettings.WebOverlayUrl, player.WebOverlayId, player.VpAvatar.Session, isObserver, GameSettings.WebOverlayServerUrl);
            sender.UrlSendOverlay(args.Avatar.Session, overlayUrl);
        }

        private static void Client_OnQueryCellEnd(Instance sender, QueryCellEndArgs args)
        {
            _cellsToQueried += 1;
        }
    }
}