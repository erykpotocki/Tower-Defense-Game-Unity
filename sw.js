"use strict";
const RELEASE = "486128cbbb7158d53dd804129cbec8ea1b61a39a22463b46cb3b1204cf19cb13";
const SCOPE = new URL(self.registration.scope);
const PREFIX = "tower-pwa-" + encodeURIComponent(SCOPE.pathname) + "-";
const CACHE_NAME = PREFIX + RELEASE;
const INFO_URL = new URL("__pwa_cache_info__", SCOPE).href;
const CORE_FILES = ["./", "index.html", "style.css", "orientation.js", "manifest.webmanifest", "offline-assets.json",
  "icons/icon-192.png", "icons/icon-512.png", "icons/maskable-512.png", "icons/apple-touch-icon.png"];

function scopedUrl(path) {
  const url = new URL(path, SCOPE);
  if (url.origin !== SCOPE.origin || !url.pathname.startsWith(SCOPE.pathname))
    throw new Error("Offline asset is outside the application scope.");
  url.search = "";
  url.hash = "";
  return url.href;
}
function usable(response) {
  if (!response || !response.ok || response.status !== 200 || response.type === "opaque") return false;
  const url = new URL(response.url || SCOPE.href);
  return url.origin === SCOPE.origin && url.pathname.startsWith(SCOPE.pathname);
}
async function announce(message) {
  const clients = await self.clients.matchAll({type: "window", includeUncontrolled: true});
  clients.forEach(function (client) { client.postMessage(message); });
}
async function precache() {
  const response = await fetch(new URL("offline-assets.json?v=" + RELEASE, SCOPE).href, {cache: "no-store"});
  if (!usable(response)) throw new Error("Cannot read the offline asset list.");
  const manifest = await response.json();
  if (manifest.release !== RELEASE || !Array.isArray(manifest.files))
    throw new Error("Offline asset list does not match this release.");
  const urls = Array.from(new Set(CORE_FILES.concat(manifest.files).map(scopedUrl)));
  const cache = await caches.open(CACHE_NAME);
  let next = 0;
  let done = 0;
  async function download() {
    while (next < urls.length) {
      const url = urls[next++];
      const downloadUrl = new URL(url);
      downloadUrl.searchParams.set("v", RELEASE);
      const result = await fetch(downloadUrl.href, {cache: "default", credentials: "same-origin"});
      if (!usable(result)) throw new Error("Cannot cache " + url);
      await cache.put(url, result);
      done++;
      if (done === urls.length || done % 8 === 0)
        await announce({type: "PWA_CACHE_PROGRESS", release: RELEASE, done: done, total: urls.length});
    }
  }
  try {
    // Cache one response at a time to limit peak memory on phones.
    await download();
    await cache.put(INFO_URL, new Response(JSON.stringify({release: RELEASE, complete: true, created: Date.now()}),
      {headers: {"Content-Type": "application/json"}}));
    await announce({type: "PWA_OFFLINE_READY", release: RELEASE});
  } catch (error) {
    await caches.delete(CACHE_NAME);
    throw error;
  }
}
self.addEventListener("install", function (event) {
  event.waitUntil(precache().then(function () { return self.skipWaiting(); }));
});
self.addEventListener("activate", function (event) {
  event.waitUntil((async function () {
    // Keep one prior complete release so an already-open tab can still use its hashed assets.
    const names = (await caches.keys()).filter(function (name) { return name.startsWith(PREFIX) && name !== CACHE_NAME; });
    const old = [];
    for (const name of names) {
      const cache = await caches.open(name);
      const info = await cache.match(INFO_URL);
      let created = 0;
      if (info) { try { created = (await info.json()).created || 0; } catch (_) {} }
      old.push({name: name, created: created});
    }
    old.sort(function (a, b) { return b.created - a.created; });
    for (const entry of old.slice(1)) await caches.delete(entry.name);
    await self.clients.claim();
  })());
});
async function cachedFor(requestUrl, version) {
  const name = PREFIX + version;
  if (!(await caches.has(name))) return undefined;
  const cache = await caches.open(name);
  const canonical = new URL(requestUrl);
  canonical.search = "";
  canonical.hash = "";
  return cache.match(canonical.href);
}
async function navigation(request) {
  try {
    const response = await fetch(request, {cache: "no-store"});
    if (response.ok) return response;
  } catch (_) {}
  const cache = await caches.open(CACHE_NAME);
  return (await cache.match(new URL("index.html", SCOPE).href)) ||
    new Response("Gra nie jest jeszcze gotowa offline. Otwórz ją raz przy działającym połączeniu.",
      {status: 503, headers: {"Content-Type": "text/plain; charset=utf-8"}});
}
self.addEventListener("fetch", function (event) {
  const request = event.request;
  const url = new URL(request.url);
  if (request.method !== "GET" || request.headers.has("Range") ||
      url.origin !== SCOPE.origin || !url.pathname.startsWith(SCOPE.pathname)) return;
  if (request.mode === "navigate") {
    event.respondWith(navigation(request));
    return;
  }
  const relative = url.pathname.slice(SCOPE.pathname.length);
  const streaming = relative.startsWith("StreamingAssets/");
  const allowed = relative.startsWith("Build/") || streaming || relative.startsWith("icons/") ||
    relative === "style.css" || relative === "orientation.js" || relative === "manifest.webmanifest" || relative === "offline-assets.json";
  if (!allowed) return;
  const version = url.searchParams.get("v") || RELEASE;
  event.respondWith((async function () {
    // StreamingAssets URLs are set by Unity without a release query: prefer fresh network content,
    // retaining the versioned precache only as offline fallback.
    if (streaming) {
      try {
        const response = await fetch(request, {cache: "no-store"});
        if (response.ok) return response;
      } catch (_) {}
      const fallback = await cachedFor(url.href, version);
      if (fallback) return fallback;
      return new Response("Offline asset unavailable", {status: 503});
    }
    const cached = await cachedFor(url.href, version);
    if (cached) return cached;
    // A new release may be loading while the previous worker still controls the page.
    // Its version query prevents the previous worker from returning stale build files.
    return fetch(request);
  })());
});
