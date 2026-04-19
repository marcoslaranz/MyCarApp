/* Manifest version: quT26/wP */
// In development, always fetch from the network and do not enable offline support.
// This is because caching would make development more difficult (changes would not
// be reflected on the first load after each change).
//self.addEventListener('fetch', () => { });

// Development service worker - no caching, always fetch from network
self.addEventListener('fetch', (event) => {
    event.respondWith(fetch(event.request));
});
