const apiBaseUrl = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5119'

export type MediaType = 'Film' | 'TvShow' | 'Book' | 'Comic' | 'Restaurant' | 'VideoGame'

export interface Recommendation {
  id: string
  recommenderId: string
  recommenderName: string
  recipientId: string
  recipientName: string
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
  recipientEmail: string
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

export interface User {
  id: string
  displayName: string
  email: string
}

export interface SignInResult {
  token: string
  user: User
}

async function toJson<T>(response: Response): Promise<T> {
  if (!response.ok) {
    const body = await response.text()
    throw new Error(body || `Request failed with status ${response.status}`)
  }
  return response.json() as Promise<T>
}

function authHeaders(token: string): HeadersInit {
  return { Authorization: `Bearer ${token}` }
}

export function signIn(provider: 'google' | 'microsoft', idToken: string): Promise<SignInResult> {
  return fetch(`${apiBaseUrl}/api/auth/${provider}`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ idToken }),
  }).then((r) => toJson<SignInResult>(r))
}

export function getMe(token: string): Promise<User> {
  return fetch(`${apiBaseUrl}/api/auth/me`, { headers: authHeaders(token) }).then((r) => toJson<User>(r))
}

export function getRecommendations(token: string): Promise<Recommendation[]> {
  return fetch(`${apiBaseUrl}/api/recommendations`, { headers: authHeaders(token) }).then((r) =>
    toJson<Recommendation[]>(r),
  )
}

export function createRecommendation(token: string, input: CreateRecommendationInput): Promise<Recommendation> {
  return fetch(`${apiBaseUrl}/api/recommendations`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json', ...authHeaders(token) },
    body: JSON.stringify(input),
  }).then((r) => toJson<Recommendation>(r))
}

export function rateRecommendation(token: string, id: string, score: number): Promise<Recommendation> {
  return fetch(`${apiBaseUrl}/api/recommendations/${id}/rate`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json', ...authHeaders(token) },
    body: JSON.stringify({ score }),
  }).then((r) => toJson<Recommendation>(r))
}

export function getStrength(token: string, recommenderId: string, recipientId: string): Promise<StrengthScore> {
  const params = new URLSearchParams({ recommenderId, recipientId })
  return fetch(`${apiBaseUrl}/api/recommendations/strength?${params}`, { headers: authHeaders(token) }).then((r) =>
    toJson<StrengthScore>(r),
  )
}
