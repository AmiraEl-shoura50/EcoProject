const SHARE_CACHE = 'shared-receipts-v1';

self.addEventListener('install', () => self.skipWaiting());
self.addEventListener('activate', (event) => event.waitUntil(self.clients.claim()));

self.addEventListener('fetch', (event) => {
  const url = new URL(event.request.url);

  // أي حاجة غير مشاركة الإيصال بتعدي عادي من غير تدخل
  if (event.request.method === 'POST' && url.pathname === '/share-receipt') {
    event.respondWith(handleShare(event.request));
  }
});

async function handleShare(request) {
  const formData = await request.formData();
  const file = formData.get('receipt');
  const text = ['title', 'text', 'url']
    .map((key) => formData.get(key))
    .filter(Boolean)
    .join('\n');

  const cache = await caches.open(SHARE_CACHE);
  await cache.delete('/__shared-file');
  await cache.delete('/__shared-text');

  if (file instanceof File) {
    await cache.put(
      '/__shared-file',
      new Response(file, {
        headers: {
          'Content-Type': file.type,
          'X-File-Name': encodeURIComponent(file.name)
        }
      })
    );
  }

  if (text) {
    await cache.put('/__shared-text', new Response(text));
  }

  // نحوّل المستخدم لصفحة الموقع اللي هتعرض الإيصال
  return Response.redirect('/share-receipt?shared=1', 303);
}