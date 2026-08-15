(() => {
  const root = document.getElementById('duhbuh-overlay');

  // Shared overlay defaults. Notification channels may override any of these.
  const DEFAULTS = {
    channel: 'default',
    position: 'bottom-center',
    offsetX: 0,
    offsetY: 0,
    maxVisible: 3,
    maxQueued: 20,
    stackDirection: 'auto',
    spacing: 10,
    duration: 5000,
    enterAnimation: 'slide',
    enterDuration: 300,
    exitAnimation: 'fade',
    exitDuration: 300
  };

  // Each channel owns an independent lane/queue. Channels can therefore
  // occupy different parts of the OBS source without blocking one another.
  const channels = new Map();

  function normalisePosition(position) {
    const valid = [
      'top-left', 'top-center', 'top-right',
      'middle-left', 'center', 'middle-right',
      'bottom-left', 'bottom-center', 'bottom-right'
    ];
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

  function getChannel(event) {
    const config = normaliseConfig(event);
    let state = channels.get(config.channel);
    if (!state) {
      state = {
        channel: config.channel,
        config,
        queue: [],
        active: []
      };
      channels.set(config.channel, state);
    } else {
      // The newest notification supplies the channel configuration. This
      // allows settings changes to take effect without recreating the page.
      state.config = config;
    }
    return state;
  }

  function getStackDirection(config) {
    if (config.stackDirection === 'forward') return 'forward';
    if (config.stackDirection === 'reverse') return 'reverse';

    // Automatic stacking grows inward from the selected edge.
    if (config.position.startsWith('top-')) return 'forward';
    if (config.position.startsWith('bottom-')) return 'reverse';
    if (config.position.endsWith('-left')) return 'forward';
    if (config.position.endsWith('-right')) return 'reverse';
    return 'forward';
  }

  function applyLaneLayout(state) {
    const { config, active } = state;
    const direction = getStackDirection(config);

    active.forEach((item, index) => {
      const el = item.el;
      const horizontal = config.position.endsWith('-left') || config.position.endsWith('-right') || config.position === 'center';
      const vertical = config.position.startsWith('top-') || config.position.startsWith('bottom-') || config.position === 'center';

      el.style.setProperty('--duhbuh-offset-x', `${config.offsetX}px`);
      el.style.setProperty('--duhbuh-offset-y', `${config.offsetY}px`);
      el.style.setProperty('--duhbuh-spacing', `${config.spacing}px`);
      el.style.setProperty('--duhbuh-index', index);
      el.dataset.stackDirection = direction;
      el.dataset.horizontalStack = horizontal && !vertical ? 'true' : 'false';
      el.dataset.position = config.position;

      // CSS handles the actual anchor/stack placement. We expose the index
      // and direction here so variable-sized notifications can be measured
      // and repositioned by the browser without hard-coded dimensions.
      if (direction === 'reverse') {
        el.style.order = String(active.length - index);
      } else {
        el.style.order = String(index);
      }
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

    root.appendChild(el);

    const item = { el, event, removing: false };
    state.active.push(item);
    applyLaneLayout(state);

    requestAnimationFrame(() => {
      el.classList.add('visible');
    });

    if (config.duration > 0) {
      item.timer = window.setTimeout(() => hideNotification(state, item), config.duration);
    }
  }

  function pump(state) {
    while (state.active.length < state.config.maxVisible && state.queue.length) {
      displayNotification(state, state.queue.shift());
    }
    applyLaneLayout(state);
  }

  function showNotification(event) {
    const state = getChannel(event);

    if (state.queue.length >= state.config.maxQueued) {
      // Preserve the newest event when the queue is full.
      state.queue.shift();
    }

    state.queue.push(event);
    pump(state);
  }

  function handleOverlayPayload(payload) {
    if (!payload || payload.eventName !== 'duhbuh.overlay') return;
    const args = payload.args || {};

    showNotification({
      channel: args.channel || 'default',
      title: args.title || 'duhBuh',
      message: args.message || '',
      meta: args.meta || '',
      duration: args.duration,
      config: args.config || {}
    });
  }

  window.duhBuhOverlay = {
    notify: showNotification,
    clearChannel(channel) {
      const state = channels.get(channel);
      if (!state) return;
      state.queue.length = 0;
      state.active.slice().forEach(item => hideNotification(state, item));
    },
    clearAll() {
      channels.forEach(state => {
        state.queue.length = 0;
        state.active.slice().forEach(item => hideNotification(state, item));
      });
    }
  };

  const client = new StreamerbotClient({
    host: '127.0.0.1',
    port: 8080,
    endpoint: '/'
  });

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
    showNotification({
      channel: 'default',
      title: 'duhBuh',
      message: 'Overlay connected',
      meta: 'Browser source test',
      duration: 3000,
      config: { position: 'top-center' }
    });
  }
})();
