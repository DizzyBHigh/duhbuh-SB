(() => {
  const root = document.getElementById('duhbuh-overlay');
  const queue = [];
  let showing = false;

  function showNotification(event) {
    queue.push(event);
    if (!showing) next();
  }

  function next() {
    if (!queue.length) { showing = false; return; }
    showing = true;
    const event = queue.shift();
    const el = document.createElement('section');
    el.className = 'duhbuh-notification';
    el.innerHTML = '<div class="duhbuh-title"></div><div class="duhbuh-body"></div><div class="duhbuh-meta"></div>';
    el.querySelector('.duhbuh-title').textContent = event.title || 'duhBuh';
    el.querySelector('.duhbuh-body').textContent = event.message || '';
    el.querySelector('.duhbuh-meta').textContent = event.meta || '';
    root.appendChild(el);
    requestAnimationFrame(() => el.classList.add('visible'));
    const duration = Number(event.duration || 5000);
    window.setTimeout(() => {
      el.classList.remove('visible');
      window.setTimeout(() => { el.remove(); next(); }, 300);
    }, duration);
  }

  window.duhBuhOverlay = { notify: showNotification };

  // Streamer.bot's official client automatically subscribes when .on() is used.
  // The overlay only listens for our namespaced Custom.Event payloads.
  const client = new StreamerbotClient({
    host: '127.0.0.1',
    port: 8080,
    endpoint: '/'
  });

  client.on('Custom.Event', ({ data }) => {
    if (!data || data.eventName !== 'duhbuh.overlay') return;
    const args = data.args || {};
    showNotification({
      title: args.title || 'duhBuh',
      message: args.message || '',
      meta: args.meta || '',
      duration: args.duration || 5000
    });
  });

  console.info('[duhBuh Overlay] Connected to Streamer.bot WebSocket client.');

  // Development/test hook.
  const params = new URLSearchParams(location.search);
  if (params.get('test') === '1') {
    showNotification({
      title: 'duhBuh',
      message: 'Overlay connected',
      meta: 'Browser source test',
      duration: 3000
    });
  }
})();
