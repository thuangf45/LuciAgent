// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// * Author:      Nguyen Minh Thuan (thuangf45)
// * License:     AGPL-3.0-only
// * LinkedIn:    https://www.linkedin.com/in/thuangf45
// * NuGet:       https://www.nuget.org/profiles/thuangf45
// * Portfolio:   https://thuangf45.github.io
// * Github:      https://github.com/thuangf45
// * Blog:        https://dev.to/thuangf45
// * Contact:     kingnemacc@gmail.com
// * Copyright (c) 2026 thuangf45. All rights reserved.
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

const DYNAMIC_CACHE_NAME = 'lucia-dynamic-fortress-v1';

let dynamicExtensions = ['.html', '.js', '.css', '.json', '.svg', '.png', '.jpg', '.jpeg'];

self.addEventListener('message', (event) => {
    if (event.data && event.data.type === 'SYNC_EXTENSIONS') {
        dynamicExtensions = event.data.extensions;
    }
});

function isTargetAsset(url) {
    // const isInternal = url.origin === self.location.origin;
    const hasTargetExt = dynamicExtensions.some(ext => url.pathname.endsWith(ext));
    const isRoot = url.pathname === '/';
    return hasTargetExt || isRoot;
}

function cleanCacheKey(request) {
    const url = new URL(request.url);
    const hasTargetExt = dynamicExtensions.some(ext => url.pathname.endsWith(ext));
    if (hasTargetExt) {
        return new Request(url.origin + url.pathname);
    }
    return request;
}

self.addEventListener('install', (e) => { self.skipWaiting(); });
self.addEventListener('activate', (e) => { e.waitUntil(self.clients.claim()); });

self.addEventListener('fetch', (e) => {
    if (e.request.method !== 'GET') return;

    const requestUrl = new URL(e.request.url);
    if (!isTargetAsset(requestUrl)) return;

    const cleanedRequest = cleanCacheKey(e.request);

    e.respondWith(
        fetch(e.request)
            .then((networkResponse) => {
                if (!networkResponse || networkResponse.status !== 200) return networkResponse;

                const responseToCache = networkResponse.clone();
                e.waitUntil(
                    caches.open(DYNAMIC_CACHE_NAME).then((cache) => {
                        cache.put(cleanedRequest, responseToCache);
                    })
                );
                return networkResponse;
            })
            .catch((err) => {
                if (e.request.headers.get('X-Fetch-Mode') === 'Realtime-Sync') {
                    return caches.open(DYNAMIC_CACHE_NAME).then((cache) => {
                        const strictStaticKey = requestUrl.origin + '/content.json';

                        return cache.match(strictStaticKey).then((cachedResponse) => {
                            if (cachedResponse) {
                                return new Response(cachedResponse.body, {
                                    status: 503,
                                    statusText: 'Server Offline But Blueprint Attached',
                                    headers: { 'Content-Type': 'application/json' }
                                });
                            }
                            return Promise.reject(err);
                        });
                    });
                }

                return caches.match(cleanedRequest);
            })
    );
});