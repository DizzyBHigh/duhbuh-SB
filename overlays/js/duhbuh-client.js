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

  // Public bridge: the C# side can later emit a normalized overlay event.
  window.duhBuhOverlay = { notify: showNotification };

  // Development/test hook. Remove or disable for production if desired.
  const params = new URLSearchParams(location.search);
  if (params.get('test') === '1') {
    showNotification({ title: 'duhBuh', message: 'Overlay connected', meta: 'Test notification', duration: 3000 });
  }
})();
