export type ConversionRequest = {
  amount: number
  fromCurrency: string
  toCurrency: string
}

export type ConversionResponse = {
  auditId: string
  amount: number
  fromCurrency: string
  toCurrency: string
  rate: number
  convertedAmount: number
  providerDate: string
  executedAtUtc: string
}

export type ProblemDetails = {
  type?: string
  title?: string
  status?: number
  detail?: string
  instance?: string
}
