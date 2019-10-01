using DiscordRPC;
using System;
using System.Threading;
using WebSocketSharp;
using WebSocketSharp.Server;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.IO;
using System.Net.Http;
using System.Text;
using DiscordRPC.Logging;
using Newtonsoft.Json;

namespace Funipresence
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            var wssv = new WebSocketServer(8080, true);
            var server = new Server();
            wssv.SslConfiguration.ServerCertificate =
              new X509Certificate2("./cert/cert.pfx", "goku");
            wssv.AddWebSocketService<Server>("/");
            server.start();
            wssv.Start();
        }
    }

    public class Server : WebSocketBehavior
    {
        DiscordRpcClient client = new DiscordRpcClient("593320595765067838");

        public void start()
        {
            try
            {
                client.Logger = new ConsoleLogger() { Level = DiscordRPC.Logging.LogLevel.Trace, Colored = true };
                client.OnReady += (sender, e) =>
                {
                    Console.WriteLine("Ready!");
                };
                client.OnPresenceUpdate += (sender, e) =>
                {
                    Console.WriteLine("Received Update! {0}", e.Presence);
                };
                client.Initialize();
            }
            catch (Exception pp)
            {
                Console.WriteLine(pp);
            }

        }

        public string lastUri = "";
        protected override async void OnMessage(MessageEventArgs e)
        {
            if (!client.IsInitialized)
                client.Initialize();

            

            try
            {
                var msg = e.Data;
                JObject j = JObject.Parse(msg);
                string Site = (string)j["site"];
                string Name = (string)j["name"];
                bool Watching = (bool)j["watching"];
                int? Shows = (int?)j["shows"];
                string Episode = (string)j["episode"];
                string Image = (string)j["image"];
                int Reload = (int)j["reload"];
                string URL = (string)j["url"];

                if (lastUri == "")
                    lastUri = URL;

                RichPresence presence = new RichPresence();
                presence.Details = $"Watching {Name}";
                presence.State = Episode;
                presence.Assets = new Assets()
                    {
                        LargeImageKey = Name.ToLower().Replace(' ', '_'),
                        LargeImageText = Name
                    };
                switch (Site)
                {
                    case "funimation":
                        presence.Assets.SmallImageKey = "funimation";
                        presence.Assets.SmallImageText = "Watching on Funimation";
                        if (URL != lastUri || Reload == 1)
                        {
                            await DeleteImage();
                            lastUri = URL;
                            await MakeImage(Image, Name.ToLower().Replace(' ', '_'));
                            client.SetPresence(presence);
                        }
                        break;
                    case "twitch":
                        presence.Assets.SmallImageKey = "twitch";
                        presence.Assets.SmallImageText = "Watching on Twitch";
                        if (URL != lastUri || Reload == 1)
                        {
                            await DeleteImage();
                            lastUri = URL;
                            await MakeImage(Image, Name.ToLower().Replace(' ', '_'));
                            client.SetPresence(presence);
                        } 
                        break;
                    case "youtube":
                        presence.Assets.SmallImageKey = "youtube";
                        presence.Assets.SmallImageText = "Watching on YouTube";
                        if (URL != lastUri || Reload == 1)
                        {
                            await DeleteImage();
                            lastUri = URL;
                            await MakeImage(Image, Name.ToLower().Replace(' ', '_'));
                            client.SetPresence(presence);
                        }
                        break;
                }

                client.SetPresence(presence);
            }
            catch (Exception pp)
            {
                Console.WriteLine(pp);
            }
        }

        public async Task MakeImage(string url, string name)
        {
            HttpClient httpclient = new HttpClient();
            Stream image = await httpclient.GetStreamAsync(url);
            httpclient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("I'll let you figure this one out yourself ;)");
            httpclient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = image.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                var content = new StringContent($"{{\"name\":\"{name.ToLower().Replace(' ', '_')}\",\"image\":\"data:image/png;base64,{Convert.ToBase64String(ms.ToArray())}\",\"type\":\"1\"}}", Encoding.UTF8, "application/json");

                try
                {
                    await httpclient.PostAsync("https://discordapp.com/api/oauth2/applications/593320595765067838/assets", content).ContinueWith(response =>
                    {
                        Console.WriteLine(response.Result.ToString());
                    });
                }
                catch (HttpRequestException f)
                {
                    MessageBox.Show(f.ToString());
                }

            }
        }

        public async Task DeleteImage()
        {
            HttpClient http = new HttpClient();
            http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("I'll let you figure this one out yourself ;)");

            var r = await http.GetAsync("https://discordapp.com/api/v6/oauth2/applications/593320595765067838/assets");
            string s = await r.Content.ReadAsStringAsync();
            ResponseAssets[] a = JsonConvert.DeserializeObject<ResponseAssets[]>(s);
            if (a.Length < 1)
                return;

            foreach (ResponseAssets ass in a)
            {
                try
                {
                    if (ass.Name == "youtube" || ass.Name == "twitch" || ass.Name == "funimation")
                        continue;

                    Console.WriteLine($"Deleting {ass.Name}: {ass.id}");
                    await http.DeleteAsync($"https://discordapp.com/api/v6/oauth2/applications/593320595765067838/assets/{ass.id}").ContinueWith(re =>
                    {
                        Console.WriteLine(re.Result);
                    });
                }
                catch (HttpRequestException f)
                {
                    Console.WriteLine(f);
                }
            }
        }
    }

    public class ResponseAssets
    {
        public int Type;
        public string id;
        public string Name;
    }


}
