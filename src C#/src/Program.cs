using DiscordRPC;
using System;
using DiscordRPC.Logging;
using System.Threading;
using WebSocketSharp;
using WebSocketSharp.Server;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography.X509Certificates;

namespace What_am_I_watching
{
    class Program : WebSocketBehavior
    {
        private static DiscordRpcClient client;

        static void Main(string[] args)
        {
            client = new DiscordRpcClient("381663579218247680", true, 0);
            client.Logger = new ConsoleLogger() { Level = DiscordRPC.Logging.LogLevel.Warning, Colored = true };
            client.OnReady += (sender, e) => {
                Console.WriteLine("Ready!");
            };
            client.Initialize();
            var wssv = new WebSocketServer(8080, true);
            wssv.SslConfiguration.ServerCertificate =
              new X509Certificate2("./cert/cert.pfx", "goku");
            wssv.AddWebSocketService<Program>("/");
            wssv.Start();
            MainLoop();
        }

        static void MainLoop()
        {
            while (client != null)
            {
                if (client != null)
                    client.Invoke();

                Thread.Sleep(10);
            }
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            var msg = e.Data;
            JObject j = JObject.Parse(msg);
            string Site = (string)j["site"];
            string Name = (string)j["name"];
            bool Watching = (bool)j["watching"];
            int? Shows = (int?)j["shows"];
            string Episode = (string)j["episode"];
            switch (Site)
            {
                case "funimation":
                    if (!Watching)
                    {
                        string b = Shows > 0
                            ? $"{Shows} shows in their queue"
                            : $"Browsing {Name}";

                        RichPresence presence = new RichPresence()
                        {
                            Details = $"Looking at {Name}",
                            State = b,
                            Assets = new Assets()
                            {
                                LargeImageKey = "funimation",
                                LargeImageText = $"Browsing {Name}"
                            }
                        };

                        client.SetPresence(presence);
                    }
                    else
                    {
                        RichPresence presence = new RichPresence()
                        {
                            Details = $"Watching {Name}",
                            State = Episode,
                            Assets = new Assets()
                            {
                                LargeImageKey = "funimation",
                                LargeImageText = Episode
                            }
                        };

                        client.SetPresence(presence);
                    }
                    break;
                case "youtube":
                    if (!Watching)
                    {
                        RichPresence presence = new RichPresence()
                        {
                            Details = $"Browsing {Name}",
                            State = "Browsing",
                            Assets = new Assets()
                            {
                                LargeImageKey = "youtube",
                                LargeImageText = $"Browsing {Name}"
                            }
                        };

                        client.SetPresence(presence);
                    }
                    else
                    {
                        RichPresence presence = new RichPresence()
                        {
                            Details = $"Watching {Name}",
                            State = Episode,
                            Assets = new Assets()
                            {
                                LargeImageKey = "youtube",
                                LargeImageText = Episode
                            }
                        };

                        client.SetPresence(presence);
                    }
                    break;
                case "twitch":
                    if (Watching)
                    {
                        RichPresence presence = new RichPresence()
                        {
                            Details = $"Watching {Name}",
                            State = Episode,
                            Assets = new Assets()
                            {
                                LargeImageKey = "twitch",
                                LargeImageText = Episode
                            }
                        };

                        client.SetPresence(presence);
                    }
                    break;
            }
        }
    }
}
