(() => {
  const root = document.getElementById('duhbuh-overlay');
  const branding = document.getElementById('duhbuh-branding');
  const brandingDark = document.getElementById('duhbuh-branding-dark');
  const brandingLight = document.getElementById('duhbuh-branding-light');
  const DEFAULTS = { channel:'default', position:'bottom-center', offsetX:0, offsetY:0, maxVisible:3, maxQueued:20, stackDirection:'auto', spacing:10, duration:5000, enterAnimation:'slide', enterDuration:300, exitAnimation:'fade', exitDuration:300, scale:100, backgroundColor:'#E60F0F12', titleColor:'#FFFFFFFF', messageColor:'#FFFFFFFF', metaColor:'#B3FFFFFF', borderColor:'#00000000', backgroundOpacity:90, borderWidth:0, borderRadius:12, titleSize:24, messageSize:18, metaSize:13 };
  const channels = new Map();
  const recentEvents = new Map();

  function normalisePosition(position) {
    const valid=['top-left','top-center','top-right','middle-left','center','middle-right','bottom-left','bottom-center','bottom-right'];
    return valid.includes(position) ? position : DEFAULTS.position;
  }

  function normaliseConfig(event) {
    const config={...DEFAULTS,...(event.config||{})};
    config.channel=String(event.channel||config.channel||DEFAULTS.channel);
    config.position=normalisePosition(config.position);
    config.offsetX=Number(config.offsetX)||0; config.offsetY=Number(config.offsetY)||0;
    config.maxVisible=Math.max(1,Number(config.maxVisible)||DEFAULTS.maxVisible);
    config.maxQueued=Math.max(0,Number(config.maxQueued)||DEFAULTS.maxQueued);
    config.spacing=Math.max(0,Number(config.spacing)||0);
    config.duration=Math.max(0,Number(config.duration)||DEFAULTS.duration);
    config.enterDuration=Math.max(0,Number(config.enterDuration)||0);
    config.exitDuration=Math.max(0,Number(config.exitDuration)||0);
    config.scale=Math.max(50,Math.min(200,Number(config.scale)||DEFAULTS.scale));
    config.backgroundOpacity=Math.max(0,Math.min(100,Number(config.backgroundOpacity)||0));
    config.borderWidth=Math.max(0,Number(config.borderWidth)||0);
    config.borderRadius=Math.max(0,Number(config.borderRadius)||0);
    config.titleSize=Math.max(8,Number(config.titleSize)||DEFAULTS.titleSize);
    config.messageSize=Math.max(8,Number(config.messageSize)||DEFAULTS.messageSize);
    config.metaSize=Math.max(8,Number(config.metaSize)||DEFAULTS.metaSize);
    return config;
  }

  function toCssColor(value, opacityOverride) {
    const hex=String(value||'').trim().replace(/^#/,'');
    const opacity=opacityOverride===undefined ? 1 : Math.max(0,Math.min(1,opacityOverride));
    if (/^[0-9a-fA-F]{8}$/.test(hex)) {
      const a=parseInt(hex.slice(0,2),16)/255, rgb=hex.slice(2);
      return `rgba(${parseInt(rgb.slice(0,2),16)},${parseInt(rgb.slice(2,4),16)},${parseInt(rgb.slice(4,6),16)},${Math.max(0,Math.min(1,a*opacity))})`;
    }
    if (/^[0-9a-fA-F]{6}$/.test(hex)) {
      return `rgba(${parseInt(hex.slice(0,2),16)},${parseInt(hex.slice(2,4),16)},${parseInt(hex.slice(4,6),16)},${opacity})`;
    }
    return value||'#ffffff';
  }

  function applyBrandingTheme(theme) {
    if (!branding || !brandingDark || !brandingLight) return;
    const requested=String(theme||'system').toLowerCase();
    let resolved=requested;
    if (resolved!=='dark' && resolved!=='light') {
      resolved=window.matchMedia && window.matchMedia('(prefers-color-scheme: light)').matches ? 'light' : 'dark';
    }
    branding.className='duhbuh-branding';
    branding.dataset.theme=resolved;
    brandingDark.hidden=resolved!=='dark';
    brandingLight.hidden=resolved!=='light';
    console.info('[duhBuh Overlay] Branding theme:', resolved, '(requested:', requested + ')');
  }

  function getStackDirection(config) {
    if(config.stackDirection==='forward') return 'forward';
    if(config.stackDirection==='reverse') return 'reverse';
    if(config.position.startsWith('top-')||config.position.startsWith('bottom-')) return 'forward';
    if(config.position.endsWith('-left')) return 'forward';
    if(config.position.endsWith('-right')) return 'reverse';
    return 'forward';
  }

  function createLane(config) {
    const lane=document.createElement('div'); lane.className='duhbuh-lane'; root.appendChild(lane); return lane;
  }

  function applyLaneConfig(state) {
    const {config,lane}=state;
    lane.dataset.position=config.position; lane.dataset.stackDirection=getStackDirection(config);
    lane.style.setProperty('--duhbuh-offset-x',`${config.offsetX}px`);
    lane.style.setProperty('--duhbuh-offset-y',`${config.offsetY}px`);
    lane.style.setProperty('--duhbuh-spacing',`${config.spacing}px`);
  }

  function getChannel(event) {
    const config=normaliseConfig(event); let state=channels.get(config.channel);
    if(!state){ state={channel:config.channel,config,queue:[],active:[],lane:createLane(config)}; channels.set(config.channel,state); }
    else { state.config=config; applyLaneConfig(state); }
    return state;
  }

  function applyLaneLayout(state) {
    applyLaneConfig(state);
    state.active.forEach((item,index)=>{item.el.style.order=String(index);item.el.dataset.stackIndex=String(index);});
  }

  function removeActive(state,item){ const index=state.active.indexOf(item); if(index!==-1)state.active.splice(index,1); applyLaneLayout(state); pump(state); }

  function hideNotification(state,item){
    if(item.removing)return; item.removing=true; if(item.timer)window.clearTimeout(item.timer);
    item.el.classList.remove('visible'); item.el.classList.add(`duhbuh-exit-${state.config.exitAnimation}`);
    window.setTimeout(()=>{item.el.remove();removeActive(state,item);},state.config.exitDuration);
  }

  function displayNotification(state,event){
    const config=state.config, el=document.createElement('section');
    el.className='duhbuh-notification'; el.classList.add(`duhbuh-enter-${config.enterAnimation}`);
    el.innerHTML='<div class="duhbuh-title"></div><div class="duhbuh-body"></div><div class="duhbuh-meta"></div>';
    el.querySelector('.duhbuh-title').textContent=event.title||'duhBuh';
    el.querySelector('.duhbuh-body').textContent=event.message||'';
    el.querySelector('.duhbuh-meta').textContent=event.meta||'';
    el.style.setProperty('--duhbuh-scale',String(config.scale/100));
    el.style.setProperty('--duhbuh-background-color',toCssColor(config.backgroundColor,config.backgroundOpacity/100));
    el.style.setProperty('--duhbuh-title-color',toCssColor(config.titleColor));
    el.style.setProperty('--duhbuh-message-color',toCssColor(config.messageColor));
    el.style.setProperty('--duhbuh-meta-color',toCssColor(config.metaColor));
    el.style.setProperty('--duhbuh-border-color',toCssColor(config.borderColor));
    el.style.setProperty('--duhbuh-border-width',`${config.borderWidth}px`);
    el.style.setProperty('--duhbuh-border-radius',`${config.borderRadius}px`);
    el.style.setProperty('--duhbuh-title-size',`${config.titleSize}px`);
    el.style.setProperty('--duhbuh-message-size',`${config.messageSize}px`);
    el.style.setProperty('--duhbuh-meta-size',`${config.metaSize}px`);
    el.style.setProperty('--duhbuh-enter-duration',`${config.enterDuration}ms`);
    el.style.setProperty('--duhbuh-exit-duration',`${config.exitDuration}ms`);
    el.style.setProperty('--duhbuh-background-opacity','1');
    console.log('[duhBuh Overlay] Applying notification config:', {channel:config.channel,backgroundColor:config.backgroundColor,titleColor:config.titleColor,messageColor:config.messageColor,metaColor:config.metaColor,borderColor:config.borderColor,backgroundOpacity:config.backgroundOpacity,scale:config.scale});
    console.log('[duhBuh Overlay] Computed notification styles:', {background:el.style.getPropertyValue('--duhbuh-background-color'),title:el.style.getPropertyValue('--duhbuh-title-color'),message:el.style.getPropertyValue('--duhbuh-message-color'),meta:el.style.getPropertyValue('--duhbuh-meta-color'),border:el.style.getPropertyValue('--duhbuh-border-color')});
    state.lane.appendChild(el);
    const item={el,event,removing:false}; state.active.push(item); applyLaneLayout(state);
    requestAnimationFrame(()=>el.classList.add('visible'));
    if(config.duration>0)item.timer=window.setTimeout(()=>hideNotification(state,item),config.duration);
  }

  function pump(state){ while(state.active.length<state.config.maxVisible&&state.queue.length)displayNotification(state,state.queue.shift()); applyLaneLayout(state); }

  function showNotification(event){
    const signature=`${event.channel||'default'}|${event.title||''}|${event.message||''}|${event.meta||''}`;
    const now=Date.now(), last=recentEvents.get(signature)||0;
    if(now-last<1000){console.debug('[duhBuh Overlay] Suppressed duplicate notification:',signature);return;}
    recentEvents.set(signature,now);
    window.setTimeout(()=>{if((recentEvents.get(signature)||0)===now)recentEvents.delete(signature);},1100);
    const state=getChannel(event);
    if(state.queue.length>=state.config.maxQueued)state.queue.shift();
    state.queue.push(event); pump(state);
  }

  function handleOverlayPayload(payload){
    if(!payload)return;
    console.log('[duhBuh Overlay] Overlay payload:', payload);
    if(payload.eventName!=='duhbuh.overlay')return;
    const args=payload.args||{};
    console.log('[duhBuh Overlay] Overlay args:', args);
    console.log('[duhBuh Overlay] Overlay config:', args.config||{});
    showNotification({channel:args.channel||'default',title:args.title||'duhBuh',message:args.message||'',meta:args.meta||'',duration:args.duration,config:args.config||{}});
  }

  window.duhBuhOverlay={
    notify:showNotification,
    setTheme:applyBrandingTheme,
    clearChannel(channel){const state=channels.get(channel);if(!state)return;state.queue.length=0;state.active.slice().forEach(item=>hideNotification(state,item));},
    clearAll(){channels.forEach(state=>{state.queue.length=0;state.active.slice().forEach(item=>hideNotification(state,item));});}
  };

  const client=new StreamerbotClient({host:'127.0.0.1',port:8080,endpoint:'/'});
  client.on('General.Custom',({event,data})=>{
    console.log('[duhBuh Overlay] General.Custom received:',event,data);
    const payload = data?.data || data;
    console.log('[duhBuh Overlay] Extracted payload:', payload);
    handleOverlayPayload(payload);
  });
  console.info('[duhBuh Overlay] Connected to Streamer.bot WebSocket client.');

  const params=new URLSearchParams(location.search);
  const bannerTheme=params.get('theme')||'system';
  const bannerVisible=params.get('banner')!=='0' && params.get('banner')!=='false';
  if (branding) branding.style.display=bannerVisible?'block':'none';
  applyBrandingTheme(bannerTheme);
  if (bannerTheme==='system' && window.matchMedia) {
    const media=window.matchMedia('(prefers-color-scheme: light)');
    const updateTheme=()=>applyBrandingTheme('system');
    if (media.addEventListener) media.addEventListener('change',updateTheme);
    else if (media.addListener) media.addListener(updateTheme);
  }

  if(params.get('test')==='1')showNotification({channel:'default',title:'duhBuh',message:'Overlay connected',meta:'Browser source test',duration:3000,config:{position:'top-center'}});
})();