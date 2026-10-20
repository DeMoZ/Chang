// Chang: based on Unity's PWA template ServiceWorker.js, changed so that a new deploy is always picked up:
// - the cache name is unique per build (the data file name is a content hash), and old caches are deleted on activate;
// - the page (index.html, manifest) is network-first, the cache is only an offline fallback;
// - only this site's files are cached: requests to other hosts (CCD catalog and bundles) are not touched.
// - version.json (the deployed build number, checked by index.html) always goes to the network.
const cacheName = {{{ JSON.stringify(COMPANY_NAME + "-" + PRODUCT_NAME + "-" + PRODUCT_VERSION + "-" + DATA_FILENAME) }}};
const contentToCache = [
    "Build/{{{ LOADER_FILENAME }}}",
    "Build/{{{ FRAMEWORK_FILENAME }}}",
#if USE_THREADS
    "Build/{{{ WORKER_FILENAME }}}",
#endif
    "Build/{{{ DATA_FILENAME }}}",
    "Build/{{{ CODE_FILENAME }}}",
    "TemplateData/style.css"

];

self.addEventListener('install', function (e) {
    console.log('[Service Worker] Install');

    e.waitUntil((async function () {
      const cache = await caches.open(cacheName);
      console.log('[Service Worker] Caching all: app shell and content');
      await cache.addAll(contentToCache);
      await self.skipWaiting();
    })());
});

self.addEventListener('activate', function (e) {
    e.waitUntil((async function () {
      const keys = await caches.keys();
      await Promise.all(keys.filter((key) => key !== cacheName).map((key) => {
        console.log(`[Service Worker] Deleting old cache: ${key}`);
        return caches.delete(key);
      }));
      await self.clients.claim();
    })());
});

self.addEventListener('fetch', function (e) {
    const url = new URL(e.request.url);

    if (e.request.method !== 'GET' || url.origin !== self.location.origin || url.pathname.endsWith('/version.json')) {
      return;
    }

    const isPage = e.request.mode === 'navigate' || url.pathname.endsWith('.webmanifest');
    e.respondWith(isPage ? networkFirst(e.request) : cacheFirst(e.request));
});

async function networkFirst(request) {
    const cache = await caches.open(cacheName);

    try {
      const response = await fetch(request);
      if (response.ok) {
        cache.put(request, response.clone());
      }
      return response;
    } catch (error) {
      const cached = await cache.match(request);
      if (cached) {
        return cached;
      }
      throw error;
    }
}

async function cacheFirst(request) {
    const cache = await caches.open(cacheName);
    const cached = await cache.match(request);
    if (cached) {
      return cached;
    }

    const response = await fetch(request);
    if (response.ok) {
      cache.put(request, response.clone());
    }
    return response;
}
