export function getApiBaseUrl(): string {
  const meta = document.querySelector<HTMLMetaElement>('meta[name="vite-api-url"]')
  const content = meta?.content ?? ''
  if (content === '__VITE_API_URL__') return ''
  return content
}
