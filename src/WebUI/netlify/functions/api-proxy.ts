// netlify/functions/api-proxy.ts
import type { Config, Context } from "@netlify/functions";

export default async (req: Request, context: Context) => {
  const secretUrl = Netlify.env.get("API_SECRET_URL");

  // Fetch data from your secret API using the hidden environment variable
  const response = await fetch(`${secretUrl}/secure-data`, {
    headers: {
      "Authorization": `Bearer ${Netlify.env.get("API_KEY")}`
    }
  });

  const data = await response.json();
  return Response.json(data);
};

export const config: Config = {
  path: "/api/secure-data"
};
