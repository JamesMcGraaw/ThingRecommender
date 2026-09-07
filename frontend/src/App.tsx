import { useEffect, useMemo, useState, type FormEvent } from 'react'
import './App.css'
import { useAuth } from './auth/AuthContext'
import { SignInButtons } from './auth/SignInButtons'
import {
  createRecommendation,
  getRecommendations,
  getStrength,
  rateRecommendation,
  type MediaType,
  type Recommendation,
  type StrengthScore,
} from './api'
import { MEDIA_TYPES } from './constants'

function App() {
  const { user, token, loading, signOut } = useAuth()

  if (loading) {
    return (
      <main>
        <p>Loading…</p>
      </main>
    )
  }

  if (!user || !token) {
    return (
      <main>
        <h1>ThingRecommender</h1>
        <p>Recommend a film, show, book, or restaurant — see how well your taste lines up.</p>
        <SignInButtons />
      </main>
    )
  }

  return <SignedInApp token={token} userId={user.id} userName={user.displayName} onSignOut={signOut} />
}

function SignedInApp({
  token,
  userId,
  userName,
  onSignOut,
}: {
  token: string
  userId: string
  userName: string
  onSignOut: () => void
}) {
  const [recommendations, setRecommendations] = useState<Recommendation[]>([])
  const [strengths, setStrengths] = useState<Record<string, StrengthScore>>({})
  const [loadError, setLoadError] = useState<string | null>(null)
  const [isSubmitting, setIsSubmitting] = useState(false)

  const [recipientEmail, setRecipientEmail] = useState('')
  const [title, setTitle] = useState('')
  const [mediaType, setMediaType] = useState<MediaType>('Film')
  const [note, setNote] = useState('')

  const received = useMemo(() => recommendations.filter((r) => r.recipientId === userId), [recommendations, userId])
  const sent = useMemo(() => recommendations.filter((r) => r.recommenderId === userId), [recommendations, userId])

  const peopleYouveRecommendedTo = useMemo(() => {
    const seen = new Map<string, string>()
    for (const r of sent) seen.set(r.recipientId, r.recipientName)
    return [...seen.entries()]
  }, [sent])

  async function refresh() {
    try {
      const recs = await getRecommendations(token)
      setRecommendations(recs)
      setLoadError(null)
    } catch (err) {
      setLoadError(err instanceof Error ? err.message : 'Failed to reach the API')
    }
  }

  useEffect(() => {
    refresh()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  useEffect(() => {
    Promise.all(
      peopleYouveRecommendedTo.map(([recipientId]) =>
        getStrength(token, userId, recipientId).then((s) => [recipientId, s] as const),
      ),
    )
      .then((entries) => setStrengths(Object.fromEntries(entries)))
      .catch(() => {
        /* strength scores are supplementary - a failure here shouldn't block the rest of the page */
      })
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [peopleYouveRecommendedTo.map(([id]) => id).join(',')])

  async function handleCreate(e: FormEvent) {
    e.preventDefault()
    if (!title.trim() || !recipientEmail.trim()) return

    setIsSubmitting(true)
    try {
      await createRecommendation(token, {
        recipientEmail: recipientEmail.trim(),
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
      await rateRecommendation(token, id, score)
      await refresh()
    } catch (err) {
      setLoadError(err instanceof Error ? err.message : 'Failed to submit rating')
    }
  }

  return (
    <main>
      <header>
        <h1>ThingRecommender</h1>
        <span>
          Signed in as {userName} <button type="button" onClick={onSignOut}>Sign out</button>
        </span>
      </header>

      {loadError && <p className="error">{loadError}</p>}

      {peopleYouveRecommendedTo.length > 0 && (
        <section>
          <h2>How much people trust your taste</h2>
          <ul>
            {peopleYouveRecommendedTo.map(([recipientId, name]) => {
              const strength = strengths[recipientId]
              return (
                <li key={recipientId}>
                  {name}:{' '}
                  {strength && strength.ratedCount > 0
                    ? `${strength.averageScore?.toFixed(1)} / 10 (${strength.ratedCount} rated)`
                    : 'no ratings yet'}
                </li>
              )
            })}
          </ul>
        </section>
      )}

      <section>
        <h2>Recommend something</h2>
        <form onSubmit={handleCreate}>
          <label>
            Recipient's email
            <input
              type="email"
              value={recipientEmail}
              onChange={(e) => setRecipientEmail(e.target.value)}
              placeholder="friend@example.com"
              required
            />
          </label>
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
              from {r.recommenderName}
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
              to {r.recipientName} — {r.score === null ? 'not rated yet' : `rated ${r.score}/10`}
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
