(function () {
  "use strict";
  const root = document.documentElement;
  const canvas = document.getElementById("unity-canvas");
  const viewport = document.getElementById("game-viewport");
  const probe = document.getElementById("safe-area-probe");
  const screenInfo = window.screen || {};
  if (!canvas || !viewport) return;
  const nativeRect = canvas.getBoundingClientRect.bind(canvas);
  const nativeAdd = window.addEventListener.bind(window);
  const isMobile = /Android|iPhone|iPad|iPod/i.test(navigator.userAgent || "") ||
    (navigator.platform === "MacIntel" && navigator.maxTouchPoints > 1);
  const state = { rotated: false, width: 1, height: 1, physicalWidth: 1, physicalHeight: 1 };
  let inputLayout = null;
  let lastLockAttempt = -Infinity;
  let lockPending = false;

  function editable(node) {
    return !!node && (node.isContentEditable || /^(INPUT|TEXTAREA|SELECT)$/.test(node.tagName));
  }
  function physicalLayout() {
    return { width: Math.max(1, root.clientWidth || window.innerWidth || 1),
      height: Math.max(1, root.clientHeight || window.innerHeight || 1) };
  }
  function stableLayout() {
    const size = physicalLayout();
    // A keyboard reducing height is not a rotation. Keep the pre-focus game shape.
    if (inputLayout) {
      if (Math.abs(size.width-inputLayout.width) > Math.max(2,inputLayout.width*.02))
        inputLayout = null;
      else size.height = Math.max(size.height,inputLayout.height);
    }
    return size;
  }
  function physicalInsets() {
    const css = probe ? getComputedStyle(probe) : {};
    return {left:parseFloat(css.paddingLeft)||0,right:parseFloat(css.paddingRight)||0,
      top:parseFloat(css.paddingTop)||0,bottom:parseFloat(css.paddingBottom)||0};
  }
  function safeArea() {
    const p = physicalInsets();
    const result = state.rotated ? {left:p.top,right:p.bottom,top:p.right,bottom:p.left} : p;
    result.left = Math.min(state.width/2,result.left);
    result.right = Math.min(state.width/2,result.right);
    result.top = Math.min(state.height/2,result.top);
    result.bottom = Math.min(state.height/2,result.bottom);
    result.width = state.width; result.height = state.height;
    return result;
  }
  function refresh() {
    const size = stableLayout();
    state.physicalWidth = size.width; state.physicalHeight = size.height;
    state.rotated = size.width < size.height;
    state.width = state.rotated ? size.height : size.width;
    state.height = state.rotated ? size.width : size.height;
    viewport.style.width = state.width+"px";
    viewport.style.height = state.height+"px";
    viewport.style.transform = state.rotated ? "translateX("+size.width+"px) rotate(90deg)" : "none";
    root.classList.add("orientation-checked","landscape-ready");
    root.classList.toggle("forced-landscape",state.rotated);
    canvas.inert = false; canvas.setAttribute("aria-hidden","false");
    const area = safeArea();
    ["left","right","top","bottom"].forEach(function (edge) {
      viewport.style.setProperty("--game-safe-"+edge,area[edge]+"px");
    });
    return true;
  }
  function logicalPoint(x,y) {
    return state.rotated ? {x:y,y:state.physicalWidth-x} : {x:x,y:y};
  }
  // Unity's Emscripten input uses client coordinates minus this rect, while its
  // render sizing uses the same width/height. Both must be in logical space.
  canvas.getBoundingClientRect = function () {
    if (!state.rotated) return nativeRect();
    return {x:0,y:0,left:0,top:0,right:state.width,bottom:state.height,
      width:state.width,height:state.height,toJSON:function(){return this;}};
  };

  const canvasEvents = new WeakSet();
  const proxies = new WeakMap();
  const pointers = new Set();
  const touches = new Set();
  let mouseHeld = false;
  const inputTypes = new Set(["mousedown","mouseup","mousemove","mouseenter","mouseleave","click","dblclick",
    "pointerdown","pointermove","pointerup","pointercancel","touchstart","touchmove","touchend","touchcancel","wheel"]);
  function onCanvas(event) {
    return event.target === canvas || (event.target && typeof event.target.nodeType === "number" && canvas.contains(event.target));
  }
  function track(event) {
    let belongs = onCanvas(event);
    if (/^pointer/.test(event.type)) {
      belongs = belongs || pointers.has(event.pointerId);
      if (event.type === "pointerdown" && belongs) pointers.add(event.pointerId);
      if (event.type === "pointerup" || event.type === "pointercancel") pointers.delete(event.pointerId);
    } else if (/^mouse/.test(event.type)) {
      if (event.type === "mousedown") mouseHeld = belongs;
      else belongs = belongs || mouseHeld;
      if (event.type === "mouseup") mouseHeld = false;
    } else if (/^touch/.test(event.type)) {
      const changed = Array.from(event.changedTouches || []);
      belongs = belongs || changed.some(function(t){return touches.has(t.identifier);});
      changed.forEach(function(t){
        if (event.type === "touchstart" && (belongs || t.target === canvas)) touches.add(t.identifier);
        if (event.type === "touchend" || event.type === "touchcancel") touches.delete(t.identifier);
      });
    }
    if (belongs) canvasEvents.add(event);
  }
  // Track before Unity registers capture or bubble handlers; no redispatch or
  // mutation of trusted DOM events, so HTML buttons retain native hit testing.
  inputTypes.forEach(function(type){nativeAdd(type,track,{capture:true,passive:true});});
  function mappedEvent(event) {
    if (!state.rotated || (!canvasEvents.has(event) && !onCanvas(event))) return event;
    if (proxies.has(event)) return proxies.get(event);
    const point = logicalPoint(Number(event.clientX)||0,Number(event.clientY)||0);
    const replacements = {clientX:point.x,clientY:point.y,pageX:point.x,pageY:point.y,
      screenX:point.x,screenY:point.y,offsetX:point.x,offsetY:point.y,
      movementX:Number(event.movementY)||0,movementY:-(Number(event.movementX)||0)};
    if (event.type === "wheel") {
      replacements.deltaX = Number(event.deltaY)||0;
      replacements.deltaY = -(Number(event.deltaX)||0);
    }
    if (/^touch/.test(event.type)) {
      const entries = new Map();
      function touchList(list) {
        const result = Array.from(list || [],function(t){
          if (entries.has(t.identifier)) return entries.get(t.identifier);
          const p = logicalPoint(t.clientX,t.clientY);
          const copy = {identifier:t.identifier,target:t.target,clientX:p.x,clientY:p.y,
            pageX:p.x,pageY:p.y,screenX:p.x,screenY:p.y,
            radiusX:t.radiusY,radiusY:t.radiusX,rotationAngle:t.rotationAngle,force:t.force};
          entries.set(t.identifier,copy); return copy;
        });
        result.item = function(index){return this[index] || null;};
        return result;
      }
      replacements.touches = touchList(event.touches);
      replacements.changedTouches = touchList(event.changedTouches);
      replacements.targetTouches = touchList(event.targetTouches);
    }
    const proxy = new Proxy(event,{get:function(target,key){
      if (Object.prototype.hasOwnProperty.call(replacements,key)) return replacements[key];
      const value = Reflect.get(target,key,target);
      return typeof value === "function" ? value.bind(target) : value;
    }});
    proxies.set(event,proxy);
    return proxy;
  }
  function wrapListeners(target) {
    const add = target.addEventListener.bind(target), remove = target.removeEventListener.bind(target);
    const listeners = new WeakMap();
    function capture(options){return typeof options === "boolean" ? options : !!(options && options.capture);}
    target.addEventListener = function(type,listener,options){
      if (!inputTypes.has(type) || !listener ||
          (typeof listener !== "function" && typeof listener.handleEvent !== "function"))
        return add(type,listener,options);
      let records = listeners.get(listener);
      if (!records) {records = new Map();listeners.set(listener,records);}
      const key = type+":"+capture(options);
      let wrapped = records.get(key);
      if (!wrapped) {
        wrapped = function(event){
          const mapped = mappedEvent(event);
          if (typeof listener === "function") return listener.call(this,mapped);
          return listener.handleEvent.call(listener,mapped);
        };
        records.set(key,wrapped);
      }
      // Options are passed through, preserving passive/once/AbortSignal semantics.
      return add(type,wrapped,options);
    };
    target.removeEventListener = function(type,listener,options){
      const records = listener && (typeof listener === "object" || typeof listener === "function") ? listeners.get(listener) : null;
      const wrapped = records && records.get(type+":"+capture(options));
      return remove(type,wrapped || listener,options);
    };
  }
  wrapListeners(canvas); wrapListeners(window); wrapListeners(document);

  function requestLock(force) {
    if (!screenInfo.orientation || typeof screenInfo.orientation.lock !== "function") return Promise.resolve(false);
    if (lockPending || (!force && Date.now()-lastLockAttempt<500)) return Promise.resolve(false);
    lastLockAttempt = Date.now(); lockPending = true;
    try {
      return Promise.resolve(screenInfo.orientation.lock("landscape"))
        .then(function(){return true;},function(){return false;})
        .finally(function(){lockPending=false;refresh();});
    } catch (_) {lockPending=false;refresh();return Promise.resolve(false);}
  }
  window.towerViewport = {safeArea:safeArea,refresh:refresh,logicalPoint:logicalPoint,
    snapshot:function(){return Object.assign({},state);}};
  window.towerIsPortrait = function(){return false;};
  window.towerRefreshOrientation = refresh;
  window.towerEnterLandscape = function(){
    const target = document.documentElement;
    const request = target.requestFullscreen || target.webkitRequestFullscreen;
    let pending = Promise.resolve();
    if (request && !document.fullscreenElement && !document.webkitFullscreenElement) {
      try {pending = Promise.resolve(request.call(target));} catch (_) {}
    }
    return pending.catch(function(){}).then(function(){return requestLock(true);}).then(refresh);
  };
  nativeAdd("resize",refresh);
  nativeAdd("orientationchange",function(){inputLayout=null;refresh();requestLock(false);});
  nativeAdd("pageshow",function(){refresh();requestLock(false);});
  nativeAdd("focus",function(){refresh();requestLock(false);});
  document.addEventListener("visibilitychange",function(){if(!document.hidden){refresh();requestLock(false);}});
  document.addEventListener("fullscreenchange",function(){inputLayout=null;refresh();requestLock(true);});
  document.addEventListener("webkitfullscreenchange",function(){inputLayout=null;refresh();requestLock(true);});
  if (screenInfo.orientation && typeof screenInfo.orientation.addEventListener === "function")
    screenInfo.orientation.addEventListener("change",function(){inputLayout=null;refresh();requestLock(false);});
  if (window.visualViewport) window.visualViewport.addEventListener("resize",refresh);
  document.addEventListener("focusin",function(event){
    if (editable(event.target)) {if(!inputLayout) inputLayout=physicalLayout();refresh();}
  },true);
  document.addEventListener("focusout",function(){
    Promise.resolve().then(function(){if(!editable(document.activeElement)){inputLayout=null;refresh();}});
  },true);
  nativeAdd("pointerdown",function(){requestLock(false);},{capture:true,passive:true});
  refresh();
  requestLock(false);
})();
