const API_BASE = import.meta.env.VITE_API_URL ?? "http://localhost:5161";
export const IMAGE_PLACEHOLDER =
  "data:image/svg+xml;utf8," +
  encodeURIComponent(
    `<svg xmlns='http://www.w3.org/2000/svg' width='800' height='500'>
      <defs>
        <linearGradient id='g' x1='0' y1='0' x2='1' y2='1'>
          <stop offset='0%' stop-color='#1e56d9'/>
          <stop offset='100%' stop-color='#2d7cf7'/>
        </linearGradient>
      </defs>
      <rect width='100%' height='100%' fill='url(#g)'/>
      <text x='50%' y='50%' dominant-baseline='middle' text-anchor='middle'
            fill='white' font-family='Segoe UI, Arial, sans-serif' font-size='28'>
        Artwork Image
      </text>
    </svg>`,
  );

export function resolveImageUrl(url: string | null | undefined): string {
  if (!url) return IMAGE_PLACEHOLDER;
  if (url.startsWith("http://") || url.startsWith("https://")) return url;
  if (url.startsWith("/")) return `${API_BASE}${url}`;
  return `${API_BASE}/${url}`;
}
