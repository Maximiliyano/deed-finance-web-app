import type { Config, Context } from '@netlify/functions';

const BACKEND_URL = process.env.BACKEND_URL;
const PROXY_SECRET = process.env.PROXY_SECRET;

export default async (req: Request, context: Context) => {
  if (!BACKEND_URL || !PROXY_SECRET) {
    return new Response('Proxy is not configured', { status: 500 });
  }

  const path = context.params.splat ?? '';
  const target = new URL(`/api/${path}`, BACKEND_URL);

  target.search = new URL(req.url).search;

  const headers = new Headers(req.headers);

  headers.delete('host');
  headers.set('X-Internal-Proxy-Key', PROXY_SECRET);

  const response = await fetch(target, {
    method: req.method,
    headers,
    body:
      req.method === 'GET' || req.method === 'HEAD'
        ? undefined
        : await req.arrayBuffer(),
  });

  const responseHeaders = new Headers(response.headers);

  return new Response(response.body, {
    status: response.status,
    headers: responseHeaders,
  });
};

export const config: Config = {
  path: '/api/*',
};
