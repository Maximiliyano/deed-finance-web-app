import type { Config, Context } from '@netlify/functions';

export default async (req: Request, context: Context) => {
  const backendUrl = process.env.BACKEND_URL;
  const proxySecret = process.env.PROXY_SECRET;

  if (!backendUrl || !proxySecret) {
    console.error('Missing BACKEND_URL or PROXY_SECRET');

    return new Response('Proxy is not configured', {
      status: 500
    });
  }

  const incomingUrl = new URL(req.url);
  const targetUrl = new URL(
    incomingUrl.pathname + incomingUrl.search,
    backendUrl
  );

  const headers = new Headers(req.headers);

  headers.delete('host');
  headers.set('X-Internal-Proxy-Key', proxySecret);

  const body =
    req.method === 'GET' || req.method === 'HEAD'
      ? undefined
      : await req.arrayBuffer();

  const response = await fetch(targetUrl, {
    method: req.method,
    headers,
    body,
  });

  return new Response(response.body, {
    status: response.status,
    headers: response.headers,
  });
};

export const config: Config = {
  path: '/api/*',
};
