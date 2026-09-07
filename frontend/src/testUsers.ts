import type { MediaType } from './api'

// Matches ThingRecommender.Domain.Seed.SeedUserIds — real sign-in replaces this later.
export const TEST_USERS = [
  { id: '11111111-1111-1111-1111-111111111111', displayName: 'Alice' },
  { id: '22222222-2222-2222-2222-222222222222', displayName: 'Bob' },
] as const

export const MEDIA_TYPES: MediaType[] = ['Film', 'TvShow', 'Book', 'Comic', 'Restaurant', 'VideoGame']

export function userName(id: string): string {
  return TEST_USERS.find((u) => u.id === id)?.displayName ?? id
}
