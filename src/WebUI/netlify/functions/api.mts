import type { Config, Context } from '@netlify/functions';

export default async (req: Request, context: Context) => {
  const backendUrl = process.env.BACKEND_URL;
  const proxySecret = process.env.PROXY_SECRET;

  if (!backendUrl || !proxySecret) {
    console.error('Missing BACKEND_URL or PROXY_SECRET');
    return new Response('Proxy is not configured', { status: 500 });
  }

  const incomingUrl = new URL(req.url);
  const targetUrl = new URL(
    incomingUrl.pathname + incomingUrl.search,
    backendUrl
  );

  const headers = new Headers(req.headers);
  headers.delete('content-length');
  headers.delete('accept-encoding');

  headers.set('x-forwarded-host', req.headers.get('host') || 'localhost');
  headers.set('x-forwarded-proto', incomingUrl.protocol.replace(':', ''));
  headers.set('x-forwarded-for', req.headers.get('x-forwarded-for') || 'unknown');

  headers.set('X-Internal-Proxy-Key', proxySecret);

  const body =
    req.method === 'GET' || req.method === 'HEAD'
      ? undefined
      : await req.arrayBuffer();

  try {
    const response = await fetch(targetUrl, {
      method: req.method,
      headers,
      body,
      redirect: 'manual',
      signal: AbortSignal.timeout(25_000),
    });

    const responseHeaders = new Headers(response.headers);
    responseHeaders.delete('content-encoding');
    responseHeaders.delete('transfer-encoding');

    return new Response(response.body, {
      status: response.status,
      headers: responseHeaders,
    });
  } catch (err: any) {
    console.error('Proxy fetch failed:', {
      target: targetUrl.toString(),
      message: err?.message,
      cause: err?.cause?.code ?? err?.cause?.message ?? err?.cause,
    });

    return new Response(
      JSON.stringify({ error: 'Upstream request failed' }),
      { status: 502, headers: { 'content-type': 'application/json' } }
    );
  }
};

export const config: Config = {
  path: '/api/*',
};
