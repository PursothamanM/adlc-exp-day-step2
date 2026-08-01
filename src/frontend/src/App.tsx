import React, { useState } from 'react'
import type { ConversionRequest, ConversionResponse } from './api/types'
import { createConversion, getConversion } from './api/conversions'

const currencyCodeRegex = /^[A-Z]{3}$/

function formatNumber(n: number): string {
  if (!Number.isFinite(n)) return ''
  return n.toLocaleString(undefined, { maximumFractionDigits: 6 })
}

export default function App(): JSX.Element {
  const [amount, setAmount] = useState<string>('')
  const [fromCurrency, setFromCurrency] = useState<string>('USD')
  const [toCurrency, setToCurrency] = useState<string>('EUR')

  const [result, setResult] = useState<ConversionResponse | null>(null)
  const [submitError, setSubmitError] = useState<string>('')
  const [submitting, setSubmitting] = useState<boolean>(false)

  const [lookupAuditId, setLookupAuditId] = useState<string>('')
  const [lookupResult, setLookupResult] = useState<ConversionResponse | null>(null)
  const [lookupError, setLookupError] = useState<string>('')
  const [lookupLoading, setLookupLoading] = useState<boolean>(false)

  async function onSubmit(e: React.FormEvent): Promise<void> {
    e.preventDefault()

    setSubmitError('')
    setResult(null)

    const parsedAmount = Number(amount)
    const normalizedFrom = fromCurrency.trim().toUpperCase()
    const normalizedTo = toCurrency.trim().toUpperCase()

    const req: ConversionRequest = {
      amount: parsedAmount,
      fromCurrency: normalizedFrom,
      toCurrency: normalizedTo,
    }

    if (!Number.isFinite(req.amount) || req.amount <= 0) {
      setSubmitError('Amount must be a number greater than 0.')
      return
    }

    if (!currencyCodeRegex.test(req.fromCurrency) || !currencyCodeRegex.test(req.toCurrency)) {
      setSubmitError('Currency codes must be exactly 3 uppercase letters (e.g., USD).')
      return
    }

    setSubmitting(true)
    try {
      const r = await createConversion(req)
      setResult(r)
      setLookupAuditId(r.auditId)
    } catch (err) {
      setSubmitError((err as Error).message)
    } finally {
      setSubmitting(false)
    }
  }

  async function onLookup(e: React.FormEvent): Promise<void> {
    e.preventDefault()

    setLookupError('')
    setLookupResult(null)

    const id = lookupAuditId.trim()
    if (!id) {
      setLookupError('Audit ID is required.')
      return
    }

    setLookupLoading(true)
    try {
      const r = await getConversion(id)
      setLookupResult(r)
    } catch (err) {
      setLookupError((err as Error).message)
    } finally {
      setLookupLoading(false)
    }
  }

  function renderConversionSummary(r: ConversionResponse): JSX.Element {
    return (
      <div style={{ display: 'grid', gap: 8 }}>
        <div>
          <strong>Converted:</strong> {formatNumber(r.convertedAmount)} {r.toCurrency}
        </div>
        <div>
          <strong>Rate:</strong> {r.rate} ({r.fromCurrency} → {r.toCurrency})
        </div>
        <div>
          <strong>Provider date:</strong> {r.providerDate}
        </div>
        <div>
          <strong>Executed at (UTC):</strong> {new Date(r.executedAtUtc).toISOString()}
        </div>
        <div>
          <strong>Audit ID:</strong> <code>{r.auditId}</code>
        </div>
      </div>
    )
  }

  return (
    <div style={{ maxWidth: 880, margin: '0 auto', padding: 16, fontFamily: 'system-ui, sans-serif' }}>
      <h1 style={{ marginTop: 8 }}>Real-Time Currency Conversion & Audit Trail</h1>

      <section style={{ marginTop: 24 }}>
        <h2>Convert now</h2>
        <form onSubmit={onSubmit} style={{ display: 'grid', gap: 12, maxWidth: 520 }}>
          <label style={{ display: 'grid', gap: 6 }}>
            Amount
            <input
              value={amount}
              onChange={(e) => setAmount(e.target.value)}
              inputMode="decimal"
              placeholder="100.00"
              style={{ padding: 8, border: '1px solid #ccc', borderRadius: 6 }}
            />
          </label>

          <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 12 }}>
            <label style={{ display: 'grid', gap: 6 }}>
              From (3-letter code)
              <input
                value={fromCurrency}
                onChange={(e) => setFromCurrency(e.target.value.toUpperCase())}
                placeholder="USD"
                style={{ padding: 8, border: '1px solid #ccc', borderRadius: 6 }}
              />
            </label>
            <label style={{ display: 'grid', gap: 6 }}>
              To (3-letter code)
              <input
                value={toCurrency}
                onChange={(e) => setToCurrency(e.target.value.toUpperCase())}
                placeholder="EUR"
                style={{ padding: 8, border: '1px solid #ccc', borderRadius: 6 }}
              />
            </label>
          </div>

          <button
            type="submit"
            disabled={submitting}
            style={{ padding: 10, borderRadius: 6, border: 'none', background: '#2563eb', color: 'white' }}
          >
            {submitting ? 'Converting…' : 'Convert'}
          </button>

          {submitError ? (
            <div style={{ color: '#b91c1c' }} role="alert">
              {submitError}
            </div>
          ) : null}
        </form>

        {result ? (
          <div style={{ marginTop: 18, padding: 16, border: '1px solid #e5e7eb', borderRadius: 10 }}>
            <h3 style={{ marginTop: 0 }}>Result</h3>
            {renderConversionSummary(result)}
          </div>
        ) : null}
      </section>

      <section style={{ marginTop: 28 }}>
        <h2>Lookup past conversion</h2>
        <form onSubmit={onLookup} style={{ display: 'grid', gap: 12, maxWidth: 520 }}>
          <label style={{ display: 'grid', gap: 6 }}>
            Audit ID
            <input
              value={lookupAuditId}
              onChange={(e) => setLookupAuditId(e.target.value)}
              placeholder="e.g. 3f2a..."
              style={{ padding: 8, border: '1px solid #ccc', borderRadius: 6 }}
            />
          </label>
          <button
            type="submit"
            disabled={lookupLoading}
            style={{ padding: 10, borderRadius: 6, border: 'none', background: '#0f766e', color: 'white' }}
          >
            {lookupLoading ? 'Loading…' : 'Fetch audit record'}
          </button>
          {lookupError ? (
            <div style={{ color: '#b91c1c' }} role="alert">
              {lookupError}
            </div>
          ) : null}
        </form>

        {lookupResult ? (
          <div style={{ marginTop: 18, padding: 16, border: '1px solid #e5e7eb', borderRadius: 10 }}>
            <h3 style={{ marginTop: 0 }}>Audit record</h3>
            {renderConversionSummary(lookupResult)}
          </div>
        ) : null}
      </section>
    </div>
  )
}
