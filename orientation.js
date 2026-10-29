(function () {
  "use strict";
  const root = document.documentElement;
  const canvas = document.getElementById("unity-canvas");
  const guard = document.getElementById("orientation-guard");
  const loading = document.getElementById("loading-screen");
  const screenInfo = window.screen || {};
  const isMobile = /Android|iPhone|iPad|iPod/i.test(navigator.userAgent || "") ||
    (navigator.platform === "MacIntel" && navigator.maxTouchPoints > 1);
  let blocked = true;
  let inputLayout = null;
  let blockedLayout = null;
  let lastLockAttempt = -Infinity;
  let lockPending = false;

  function editable(node) {
    return node && (node.isContentEditable || /^(INPUT|TEXTAREA|SELECT)$/.test(node.tagName));
  }
  function layout() {
    return {
      width: root.clientWidth || window.innerWidth,
      height: root.clientHeight || window.innerHeight
    };
  }
  function physicalLandscape() {
    if (!isMobile) return null;
    const type = screenInfo.orientation && screenInfo.orientation.type;
    if (/^landscape/.test(type || "")) return true;
    if (/^portrait/.test(type || "")) return false;
    // iOS exposes this even where ScreenOrientation.lock is unavailable.
    if (typeof window.orientation === "number") return Math.abs(window.orientation) % 180 === 90;
    return null;
  }
  function isLandscape() {
    const size = layout();
    // A keyboard can reduce the viewport height without rotating the phone.
    // Preserve the pre-focus shape for browsers without a physical orientation API.
    if (inputLayout) {
      if (Math.abs(size.width - inputLayout.width) >= Math.max(2, size.width * .02) ||
          (!editable(document.activeElement) && size.height >= inputLayout.height - 2))
        inputLayout = null;
      else size.height = Math.max(size.height, inputLayout.height);
    }
    return physicalLandscape() !== false && size.width > size.height;
  }
  function dismissInput() {
    const active = document.activeElement;
    if (editable(active) && typeof active.blur === "function") active.blur();
    if (navigator.virtualKeyboard && typeof navigator.virtualKeyboard.hide === "function") {
      try { navigator.virtualKeyboard.hide(); } catch (_) {}
    }
  }
  function refresh() {
    const size = layout();
    let next = !isLandscape();
    // Without physical APIs, keyboard height alone cannot unlock a mobile portrait.
    // A real rotation changes width; an explicit orientation API remains authoritative.
    if (isMobile && blocked && !next && physicalLandscape() === null && blockedLayout &&
        Math.abs(size.width - blockedLayout.width) < Math.max(2, size.width * .02))
      next = true;
    if (next && !blockedLayout) blockedLayout = size;
    if (!next) blockedLayout = null;
    if (next !== blocked || !root.classList.contains("orientation-checked")) {
      blocked = next;
      root.classList.add("orientation-checked");
      root.classList.toggle("landscape-ready", !blocked);
      guard.hidden = !blocked;
      canvas.inert = blocked;
      canvas.setAttribute("aria-hidden", String(blocked));
      loading.inert = blocked;
    }
    if (blocked) dismissInput();
    return !blocked;
  }
  function requestLock(force) {
    if (!screenInfo.orientation || typeof screenInfo.orientation.lock !== "function")
      return Promise.resolve(false);
    if (lockPending || (!force && Date.now() - lastLockAttempt < 500))
      return Promise.resolve(false);
    lastLockAttempt = Date.now();
    lockPending = true;
    try {
      return Promise.resolve(screenInfo.orientation.lock("landscape"))
        .then(function () { return true; }, function () { return false; })
        .finally(function () { lockPending = false; refresh(); });
    } catch (_) {
      lockPending = false;
      return Promise.resolve(false);
    }
  }
  // The native Unity bridge reads this, even while Time.timeScale is zero.
  window.towerIsPortrait = function () { return blocked; };
  window.towerRefreshOrientation = refresh;
  window.towerEnterLandscape = function () {
    const target = document.documentElement;
    const request = target.requestFullscreen || target.webkitRequestFullscreen;
    let pending = Promise.resolve();
    if (request && !document.fullscreenElement && !document.webkitFullscreenElement) {
      try { pending = Promise.resolve(request.call(target)); } catch (_) {}
    }
    // A denied fullscreen/lock request must still leave the rotate guard working.
    return pending.catch(function () {}).then(function () { return requestLock(true); }).then(refresh);
  };
  document.getElementById("orientation-button").addEventListener("click", window.towerEnterLandscape);
  window.addEventListener("resize", refresh);
  window.addEventListener("orientationchange", function () { refresh(); requestLock(false); });
  window.addEventListener("pageshow", function () { refresh(); requestLock(false); });
  window.addEventListener("focus", function () { refresh(); requestLock(false); });
  document.addEventListener("visibilitychange", function () {
    if (!document.hidden) { refresh(); requestLock(false); }
  });
  function fullscreenChanged() { refresh(); requestLock(true); }
  document.addEventListener("fullscreenchange", fullscreenChanged);
  document.addEventListener("webkitfullscreenchange", fullscreenChanged);
  if (screenInfo.orientation && typeof screenInfo.orientation.addEventListener === "function")
    screenInfo.orientation.addEventListener("change", function () { refresh(); requestLock(false); });
  if (window.visualViewport) window.visualViewport.addEventListener("resize", refresh);
  document.addEventListener("focusin", function (event) {
    if (!editable(event.target)) return;
    if (!inputLayout) inputLayout = layout();
    refresh();
    if (blocked) { dismissInput(); event.stopImmediatePropagation(); }
  }, true);
  document.addEventListener("focusout", function () {
    // Focus can move directly between two input fields.
    Promise.resolve().then(function () {
      refresh();
    });
  }, true);
  document.addEventListener("pointerdown", function (event) {
    refresh();
    if (blocked && !guard.contains(event.target)) {
      event.preventDefault();
      event.stopImmediatePropagation();
      return;
    }
    requestLock(false);
  }, true);
  refresh();
  requestLock(false);
})();
