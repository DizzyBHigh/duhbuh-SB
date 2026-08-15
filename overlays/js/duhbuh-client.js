(() => {
  const root = document.getElementById('duhbuh-overlay');

  const DEFAULTS = {
    channel: 'default', position: 'bottom-center', offsetX: 0, offsetY: 0,
    maxVisible: 3, maxQueued: 20, stackDirection: 'auto', spacing: 10,
    duration: 5000, enterAnimation: 'slide', enterDuration: 300,
    exitAnimation: 'fade', exitDuration: 300
  };
  const channels = new Map();
  const recentlyHandled = new Map();
  const DEDUPE_WINDOW_MS = 1000;

  function normalisePosition(position) {
    const valid = ['top-left','top-center','top-right','middle-left','center','middle-right','bottom-left','bottom-center','bottom-right'];
    return valid.includes(position) ? position : DEFAULTS.position;
  }

  function normaliseConfig(event) {
    const config = { ...DEFAULTS, ...(event.config || {}) };
    config.channel = String(event.channel || config.channel || DEFAULTS.channel);
    config.position = normalisePosition(config.position);
    config.offsetX = Number(config.offsetX) || 0;
    config.offsetY = Number(config.offsetY) || 0;
    config.maxVisible = Math.max(1, Number(config.maxVisible) || DEFAULTS.maxVisible);
    config.maxQueued = Math.max(0, Number(config.maxQueued) || DEFAULTS.maxQueued);
    config.spacing = Math.max(0, Number(config.spacing) || 0);
    config.duration = Math.max(0, Number(config.duration) || DEFAULTS.duration);
    config.enterDuration = Math.max(0, Number(config.enterDuration) || 0);
    config.exitDuration = Math.max(0, Number(config.exitDuration) || 0);
    return config;
  }

  function getStackDirection(config) {
    if (config.stackDirection === 'forward') return 'forward';
    if (config.stackDirection === 'reverse') return 'reverse';
    if (config.position.startsWith('top-') || config.position.startsWith('bottom-')) return 'forward';
    if (config.position.endsWith('-left')) return 'forward';
    if (config.position.endsWith('-right')) return 'reverse';
    return 'forward';
  }

  function createLane(config) {
    const lane = document.createElement('div');
    lane.className = 'duhbuh-lane';
    lane.dataset.position = config.position;
    lane.dataset.stackDirection = getStackDirection(config);
    lane.style.setProperty('--duhbuh-offset-x', `${config.offsetX}px`);
    lane.style.setProperty('--duhbuh-offset-y', `${config.offsetY}px`);
    lane.style.setProperty('--duhbuh-spacing', `${config.spacing}px`);
    root.appendChild(lane);
    return lane;
  }

  function applyLaneConfig(state) {
    const { config, lane } = state;
    lane.dataset.position = config.position;
    lane.dataset.stackDirection = getStackDirection(config);
    lane.style.setProperty('--duhbuh-offset-x', `${config.offsetX}px`);
    lane.style.setProperty('--duhbuh-offset-y', `${config.offsetY}px`);
    lane.style.setProperty('--duhbuh-spacing', `${config.spacing}px`);
  }

  function getChannel(event) {
    const config = normaliseConfig(event);
    let state = channels.get(config.channel);
    if (!state) {
      state = { channel: config.channel, config, queue: [], active: [], lane: createLane(config) };
      channels.set(config.channel, state);
    } else {
      state.config = config;
      applyLaneConfig(state);
    }
    return state;
  }

  function applyLaneLayout(state) {
    applyLaneConfig(state);
    state.active.forEach((item, index) => {
      item.el.style.order = String(index);
      item.el.dataset.stackIndex = String(index);
    });
  }

  function removeActive(state, item) {
    const index = state.active.indexOf(item);
    if (index !== -1) state.active.splice(index, 1);
    applyLaneLayout(state);
    pump(state);
  }

  function hideNotification(state, item) {
    if (item.removing) return;
    item.removing = true;
    if (item.timer) window.clearTimeout(item.timer);
    item.el.classList.remove('visible');
    item.el.classList.add(`duhbuh-exit-${state.config.exitAnimation}`);
    window.setTimeout(() => {
      item.el.remove();
      removeActive(state, item);
    }, state.config.exitDuration);
  }

  function displayNotification(state, event) {
    const config = state.config;
    const el = document.createElement('section');
    el.className = 'duhbuh-notification';
    el.classList.add(`duhbuh-enter-${config.enterAnimation}`);
    el.innerHTML = '<div class="duhbuh-title"></div><div class="duhbuh-body"></div><div class="duhbuh-meta"></div>';
    el.querySelector('.duhbuh-title').textContent = event.title || 'duhBuh';
    el.querySelector('.duhbuh-body').textContent = event.message || '';
    el.querySelector('.duhbuh-meta').textContent = event.meta || '';

    state.lane.appendChild(el);
    const item = { el, event, removing: false };
    state.active.push(item);
    applyLaneLayout(state);

    requestAnimationFrame(() => el.classList.add('visible'));
    if (config.duration > 0) item.timer = window.setTimeout(() => hideNotification(state, item), config.duration);
  }

  function pump(state) {
    while (state.active.length < state.config.maxVisible && state.queue.length) {
      displayNotification(state, state.queue.shift());
    }
    applyLaneLayout(state);
  }

  function showNotification(event) {
    const state = getChannel(event);
    if (state.queue.length >= state.config.maxQueued) state.queue.shift();
    state.queue.push(event);
    pump(state);
  }

  function handleOverlayPayload(payload) {
    if (!payload || payload.eventName !== 'duhbuh.overlay') return;

    // The same Streamer.bot custom notification can be exposed through more
    // than one WebSocket event envelope. Ignore the same payload if it arrives
    // again within the short deduplication window.
    let dedupeKey;
    try {
      dedupeKey = JSON.stringify(payload);
    } catch {
      dedupeKey = null;
    }
    if (dedupeKey) {
      const now = Date.now();
      const previous = recentlyHandled.get(dedupeKey);
      if (previous && now - previous < DEDUPE_WINDOW_MS) return;
      recentlyHandled.set(dedupeKey, now);
      for (const [key, timestamp] of recentlyHandled) {
        if (now - timestamp >= DEDUPE_WINDOW_MS) recentlyHandled.delete(key);
      }
    }

    const args = payload.args || {};
    showNotification({
      channel: args.channel || 'default', title: args.title || 'duhBuh',
      message: args.message || '', meta: args.meta || '', duration: args.duration,
      config: args.config || {}
    });
  }

  window.duhBuhOverlay = {
    notify: showNotification,
    clearChannel(channel) {
      const state = channels.get(channel); if (!state) return;
      state.queue.length = 0; state.active.slice().forEach(item => hideNotification(state, item));
    },
    clearAll() {
      channels.forEach(state => { state.queue.length = 0; state.active.slice().forEach(item => hideNotification(state, item)); });
    }
  };

  const client = new StreamerbotClient({ host: '127.0.0.1', port: 8080, endpoint: '/' });
  client.on('General.Custom', ({ event, data }) => {
    console.log('[duhBuh Overlay] General.Custom received:', event, data);
    handleOverlayPayload(data?.data || data);
  });
  client.on('Custom.Event', ({ event, data }) => {
    console.log('[duhBuh Overlay] Custom.Event received:', event, data);
    handleOverlayPayload(data?.data || data);
  });
  console.info('[duhBuh Overlay] Connected to Streamer.bot WebSocket client.');

  const params = new URLSearchParams(location.search);
  if (params.get('test') === '1') {
    showNotification({ channel: 'default', title: 'duhBuh', message: 'Overlay connected', meta: 'Browser source test', duration: 3000, config: { position: 'top-center' } });
  }
})();
