const apiBaseUrl = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5119'

export type MediaType = 'Film' | 'TvShow' | 'Book' | 'Comic' | 'Restaurant' | 'VideoGame'

export interface Recommendation {
  id: string
  recommenderId: string
  recipientId: string
  thingId: string
  thingTitle: string
  mediaType: MediaType
  externalUrl: string | null
  note: string | null
  createdAtUtc: string
  score: number | null
  ratedAtUtc: string | null
}

export interface CreateRecommendationInput {
  recommenderId: string
  recipientId: string
  thingTitle: string
  mediaType: MediaType
  note?: string
}

export interface StrengthScore {
  recommenderId: string
  recipientId: string
  averageScore: number | null
  ratedCount: number
}

async function toJson<T>(response: Response): Promise<T> {
  if (!response.ok) {
    const body = await response.text()
    throw new Error(body || `Request failed with status ${response.status}`)
  }
  return response.json() as Promise<T>
}

export function getRecommendations(): Promise<Recommendation[]> {
  return fetch(`${apiBaseUrl}/api/recommendations`).then((r) => toJson<Recommendation[]>(r))
}

export function createRecommendation(input: CreateRecommendationInput): Promise<Recommendation> {
  return fetch(`${apiBaseUrl}/api/recommendations`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(input),
  }).then((r) => toJson<Recommendation>(r))
}

export function rateRecommendation(id: string, score: number): Promise<Recommendation> {
  return fetch(`${apiBaseUrl}/api/recommendations/${id}/rate`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ score }),
  }).then((r) => toJson<Recommendation>(r))
}

export function getStrength(recommenderId: string, recipientId: string): Promise<StrengthScore> {
  const params = new URLSearchParams({ recommenderId, recipientId })
  return fetch(`${apiBaseUrl}/api/recommendations/strength?${params}`).then((r) => toJson<StrengthScore>(r))
}
