import type { ConversionRequest, ConversionResponse, ProblemDetails } from './types'
import { getApiBaseUrl } from './getApiBaseUrl'

function buildUrl(path: string): string {
  const base = getApiBaseUrl().replace(/\/+$/, '')
  if (!base) return path.startsWith('/') ? path : `/${path}`
  return `${base}${path.startsWith('/') ? '' : '/'}${path}`
}

export async function createConversion(
  payload: ConversionRequest,
): Promise<ConversionResponse> {
  const res = await fetch(buildUrl('/api/conversions'), {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(payload),
  })

  if (!res.ok) {
    const problem = (await res.json().catch(() => undefined)) as ProblemDetails | undefined
    throw Object.assign(new Error(problem?.detail ?? 'Request failed'), { status: res.status, problem })
  }

  return (await res.json()) as ConversionResponse
}

export async function getConversion(auditId: string): Promise<ConversionResponse> {
  const res = await fetch(buildUrl(`/api/conversions/${encodeURIComponent(auditId)}`), {
    method: 'GET',
    headers: { 'Accept': 'application/json' },
  })

  if (!res.ok) {
    const problem = (await res.json().catch(() => undefined)) as ProblemDetails | undefined
    throw Object.assign(new Error(problem?.detail ?? 'Request failed'), { status: res.status, problem })
  }

  return (await res.json()) as ConversionResponse
}
