const https = require('https');
const fs = require('fs');

const WebSocket = require('ws');
const rpc = require(`discord-rpc`);
const client = new rpc.Client({ transport: 'ipc' });
client.login('381663579218247680');

const server = https.createServer({
  cert: fs.readFileSync('./cert/certificate.pem'),
  key: fs.readFileSync('./cert/key.pem'),
});
server.listen(8080);
client.on(`ready`, () => {
  console.log(`Running!`);
});
const wss = new WebSocket.Server({ server });

wss.on('connection', (ws) => {
  ws.on('message', (msg) => {
    let data = JSON.parse(msg);
    if (data.site === 'funimation') {
      if (!data.watching) {
        let b = data.shows ? `${data.shows} shows in their queue` : `Browsing ${data.name}`;
        client.setActivity({
          details: `Looking at ${data.name}`,
          state: b,
          largeImageKey: 'funimation',
          largeImageText: `Browsing ${data.name}`,
        });
      } else {
        client.setActivity({
          details: `Watching ${data.name}`,
          state: data.episode,
          largeImageKey: 'funimation',
          largeImageText: data.episode,
        });
      }
    } else if (data.site === 'youtube') {
      if (data.watching) {
        client.setActivity({
          details: `Watching ${data.name}`,
          state: data.episode,
          largeImageKey: 'youtube',
          largeImageText: data.episode,
        });
      } else {
        client.setActivity({
          details: `Browsing ${data.name}`,
          state: 'Browsing',
          largeImageKey: 'youtube',
          largeImageText: `Browsing ${data.name}`,
        });
      }
    } else if (data.site === 'twitch') {
      if (data.watching) {
        client.setActivity({
          details: `Watching ${data.name}`,
          state: data.episode,
          largeImageKey: 'twitch',
          largeImageText: data.episode,
        });
      }
    }
  });
});
