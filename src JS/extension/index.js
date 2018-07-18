var a = new WebSocket('wss://localhost:8080');
let data = {};
a.onopen = () => {
  data.site = window.location.hostname.split('.')[1];
  if (window.location.hostname === `www.funimation.com`) {
    if (document.getElementById('player')) {
      data.watching = true;
      data.name = document.getElementsByClassName(`show-headline video-title`)[0].innerText;
      data.episode = document.getElementsByClassName('episode-headline')[0].innerHTML.split('<br>').join();
      a.send(JSON.stringify(data));
    } else if (document.URL === 'https://www.funimation.com/') {
      data.watching = false;
      data.name = 'the home page';
      a.send(JSON.stringify(data));
    } else if (document.URL.includes('account')) {
      setInterval(() => {
        data.watching = false;
        data.name = 'their queue';
        data.shows = document.getElementsByClassName('queue-item clearfix ui-sortable-handle').length;
        a.send(JSON.stringify(data));
      }, 6000);
    } else {
      data.watching = false;
      data.name = document.getElementsByTagName('meta')[3].content.split(',')[0];
      a.send(JSON.stringify(data));
    }
  } else if (window.location.hostname === `www.youtube.com`) {
    // Using a setInterval because youtube is stupid
    setInterval(() => {
      if (document.URL.includes('watch?v=')) {
        data.watching = true;
        data.name = document.getElementsByClassName('yt-simple-endpoint style-scope ytd-video-owner-renderer')[0]
          .attributes[1].textContent;
        data.episode = document.getElementsByClassName('title style-scope ytd-video-primary-info-renderer')[0].innerText;
        a.send(JSON.stringify(data));
      } else if (document.URL.includes('channel') || document.URL.includes('user')) {
        data.watching = false;
        data.name = document.getElementById('channel-title').innerText;
        a.send(JSON.stringify(data));
      } else if (document.URL.includes('subscriptions')) {
        data.watching = false;
        data.name = 'subscriptions';
        a.send(JSON.stringify(data));
      } else if (document.URL.includes('trending')) {
        data.watching = false;
        data.name = 'the trending page';
        a.send(JSON.stringify(data));
      }
    }, 5000);
  } else if (window.location.hostname === 'www.twitch.tv') {
    if (document.getElementsByClassName('pl-overlay pl-overlay__fullscreen')[0]) {
      data.watching = true;
      data.name = document.getElementsByClassName('player-text-link player-text-link--no-color qa-display-name')[0].innerText;
      data.episode = document.getElementsByClassName('player-streaminfo__title qa-stream-title')[0].innerText;
      a.send(JSON.stringify(data));
    }
  }
};
