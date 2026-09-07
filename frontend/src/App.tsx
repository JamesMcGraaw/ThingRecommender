import { useEffect, useMemo, useState, type FormEvent } from 'react'
import './App.css'
import {
  createRecommendation,
  getRecommendations,
  getStrength,
  rateRecommendation,
  type MediaType,
  type Recommendation,
  type StrengthScore,
} from './api'
import { MEDIA_TYPES, TEST_USERS, userName } from './testUsers'

function App() {
  const [currentUserId, setCurrentUserId] = useState<string>(TEST_USERS[0].id)
  const otherUser = TEST_USERS.find((u) => u.id !== currentUserId)!

  const [recommendations, setRecommendations] = useState<Recommendation[]>([])
  const [strength, setStrength] = useState<StrengthScore | null>(null)
  const [loadError, setLoadError] = useState<string | null>(null)
  const [isSubmitting, setIsSubmitting] = useState(false)

  const [title, setTitle] = useState('')
  const [mediaType, setMediaType] = useState<MediaType>('Film')
  const [note, setNote] = useState('')

  async function refresh() {
    try {
      const [recs, strengthScore] = await Promise.all([
        getRecommendations(),
        getStrength(currentUserId, otherUser.id),
      ])
      setRecommendations(recs)
      setStrength(strengthScore)
      setLoadError(null)
    } catch (err) {
      setLoadError(err instanceof Error ? err.message : 'Failed to reach the API')
    }
  }

  useEffect(() => {
    refresh()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [currentUserId])

  const received = useMemo(
    () => recommendations.filter((r) => r.recipientId === currentUserId),
    [recommendations, currentUserId],
  )
  const sent = useMemo(
    () => recommendations.filter((r) => r.recommenderId === currentUserId),
    [recommendations, currentUserId],
  )

  async function handleCreate(e: FormEvent) {
    e.preventDefault()
    if (!title.trim()) return

    setIsSubmitting(true)
    try {
      await createRecommendation({
        recommenderId: currentUserId,
        recipientId: otherUser.id,
        thingTitle: title.trim(),
        mediaType,
        note: note.trim() || undefined,
      })
      setTitle('')
      setNote('')
      await refresh()
    } catch (err) {
      setLoadError(err instanceof Error ? err.message : 'Failed to create recommendation')
    } finally {
      setIsSubmitting(false)
    }
  }

  async function handleRate(id: string, score: number) {
    try {
      await rateRecommendation(id, score)
      await refresh()
    } catch (err) {
      setLoadError(err instanceof Error ? err.message : 'Failed to submit rating')
    }
  }

  return (
    <main>
      <header>
        <h1>ThingRecommender</h1>
        <label>
          Signed in as{' '}
          <select value={currentUserId} onChange={(e) => setCurrentUserId(e.target.value)}>
            {TEST_USERS.map((u) => (
              <option key={u.id} value={u.id}>
                {u.displayName}
              </option>
            ))}
          </select>
        </label>
      </header>

      {loadError && <p className="error">{loadError}</p>}

      <section>
        <h2>How much {otherUser.displayName} trusts your taste</h2>
        {strength && strength.ratedCount > 0 ? (
          <p>
            Average score: <strong>{strength.averageScore?.toFixed(1)}</strong> / 10 (
            {strength.ratedCount} rated)
          </p>
        ) : (
          <p>No ratings yet.</p>
        )}
      </section>

      <section>
        <h2>Recommend something to {otherUser.displayName}</h2>
        <form onSubmit={handleCreate}>
          <label>
            Title
            <input value={title} onChange={(e) => setTitle(e.target.value)} required />
          </label>
          <label>
            Type
            <select value={mediaType} onChange={(e) => setMediaType(e.target.value as MediaType)}>
              {MEDIA_TYPES.map((type) => (
                <option key={type} value={type}>
                  {type}
                </option>
              ))}
            </select>
          </label>
          <label>
            Note
            <input
              value={note}
              onChange={(e) => setNote(e.target.value)}
              placeholder="I think you'll love it..."
            />
          </label>
          <button type="submit" disabled={isSubmitting}>
            Recommend
          </button>
        </form>
      </section>

      <section>
        <h2>Recommended to you</h2>
        {received.length === 0 && <p>Nothing yet.</p>}
        <ul className="recommendation-list">
          {received.map((r) => (
            <li key={r.id}>
              <ThingTitle recommendation={r} />
              <br />
              from {userName(r.recommenderId)}
              {r.note && <em> — "{r.note}"</em>}
              <div className="rating">
                {r.score === null ? (
                  <RateControl onRate={(score) => handleRate(r.id, score)} />
                ) : (
                  <span>Your rating: {r.score}/10</span>
                )}
              </div>
            </li>
          ))}
        </ul>
      </section>

      <section>
        <h2>Sent by you</h2>
        {sent.length === 0 && <p>Nothing yet.</p>}
        <ul className="recommendation-list">
          {sent.map((r) => (
            <li key={r.id}>
              <ThingTitle recommendation={r} />
              <br />
              to {userName(r.recipientId)} —{' '}
              {r.score === null ? 'not rated yet' : `rated ${r.score}/10`}
            </li>
          ))}
        </ul>
      </section>
    </main>
  )
}

function ThingTitle({ recommendation }: { recommendation: Recommendation }) {
  return (
    <>
      <span className="thing-title">{recommendation.thingTitle}</span>{' '}
      <span className="media-type">({recommendation.mediaType})</span>
      {recommendation.externalUrl && (
        <>
          {' '}
          <a href={recommendation.externalUrl} target="_blank" rel="noreferrer">
            more info
          </a>
        </>
      )}
    </>
  )
}

function RateControl({ onRate }: { onRate: (score: number) => void }) {
  const [score, setScore] = useState(8)

  return (
    <span>
      <select value={score} onChange={(e) => setScore(Number(e.target.value))}>
        {Array.from({ length: 10 }, (_, i) => i + 1).map((n) => (
          <option key={n} value={n}>
            {n}
          </option>
        ))}
      </select>
      <button type="button" onClick={() => onRate(score)}>
        Rate
      </button>
    </span>
  )
}

export default App
