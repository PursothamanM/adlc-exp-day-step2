import { describe, expect, it, vi } from 'vitest'
import { render, screen, fireEvent, waitFor } from '@testing-library/react'
import App from './App'

describe('App', () => {
  it('renders the conversion form', () => {
    render(<App />)
    expect(screen.getByText('Convert now')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /convert/i })).toBeInTheDocument()
  })

  it('submits and renders result on success', async () => {
    const mockResponse = {
      auditId: 'audit-1',
      amount: 100,
      fromCurrency: 'USD',
      toCurrency: 'EUR',
      rate: 0.92,
      convertedAmount: 92,
      providerDate: '2026-01-15',
      executedAtUtc: '2026-01-15T14:03:27.481Z',
    }

    globalThis.fetch = vi.fn(async () => {
      return {
        ok: true,
        status: 200,
        json: async () => mockResponse,
      } as unknown as Response
    })

    render(<App />)

    fireEvent.change(screen.getByPlaceholderText('100.00'), { target: { value: '100' } })
    fireEvent.click(screen.getByRole('button', { name: /convert/i }))

    await waitFor(() => {
      expect(screen.getByText('audit-1')).toBeInTheDocument()
      expect(screen.getByText(/Converted:/i)).toBeInTheDocument()
    })
  })
})
