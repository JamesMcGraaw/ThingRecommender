import { useEffect, useMemo, useState, type FormEvent } from 'react'
import './App.css'
import { useAuth } from './auth/AuthContext'
import { SignInButtons } from './auth/SignInButtons'
import {
  createRecommendation,
  getRecommendations,
  getStrength,
  logManualRecommendation,
  rateRecommendation,
  type MediaType,
  type MediaTypeStrength,
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

  const [manualRecommenderName, setManualRecommenderName] = useState('')
  const [manualTitle, setManualTitle] = useState('')
  const [manualMediaType, setManualMediaType] = useState<MediaType>('Film')
  const [manualNote, setManualNote] = useState('')
  const [isSubmittingManual, setIsSubmittingManual] = useState(false)

  const received = useMemo(() => recommendations.filter((r) => r.recipientId === userId), [recommendations, userId])
  const sent = useMemo(() => recommendations.filter((r) => r.recommenderId === userId), [recommendations, userId])

  const peopleYouveRecommendedTo = useMemo(() => {
    const seen = new Map<string, string>()
    for (const r of sent) seen.set(r.recipientId, r.recipientName)
    return [...seen.entries()]
  }, [sent])

  const peopleYouTrust = useMemo(() => {
    const byRecommender = new Map<string, { name: string; scores: number[]; scoresByType: Map<MediaType, number[]> }>()
    for (const r of received) {
      const key = r.recommenderId ?? `external:${r.recommenderName}`
      const entry = byRecommender.get(key) ?? {
        name: r.recommenderName,
        scores: [],
        scoresByType: new Map<MediaType, number[]>(),
      }
      if (r.score !== null) {
        entry.scores.push(r.score)
        const typeScores = entry.scoresByType.get(r.mediaType) ?? []
        typeScores.push(r.score)
        entry.scoresByType.set(r.mediaType, typeScores)
      }
      byRecommender.set(key, entry)
    }
    return [...byRecommender.entries()].map(([key, { name, scores, scoresByType }]) => ({
      key,
      name,
      averageScore: scores.length > 0 ? scores.reduce((a, b) => a + b, 0) / scores.length : null,
      ratedCount: scores.length,
      byMediaType: [...scoresByType.entries()]
        .map(([mediaType, typeScores]) => ({
          mediaType,
          averageScore: typeScores.reduce((a, b) => a + b, 0) / typeScores.length,
          ratedCount: typeScores.length,
        }))
        .sort((a, b) => a.mediaType.localeCompare(b.mediaType)),
    }))
  }, [received])

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

  async function handleLogManual(e: FormEvent) {
    e.preventDefault()
    if (!manualTitle.trim() || !manualRecommenderName.trim()) return

    setIsSubmittingManual(true)
    try {
      await logManualRecommendation(token, {
        externalRecommenderName: manualRecommenderName.trim(),
        thingTitle: manualTitle.trim(),
        mediaType: manualMediaType,
        note: manualNote.trim() || undefined,
      })
      setManualRecommenderName('')
      setManualTitle('')
      setManualNote('')
      await refresh()
    } catch (err) {
      setLoadError(err instanceof Error ? err.message : 'Failed to log recommendation')
    } finally {
      setIsSubmittingManual(false)
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
                  {strength && <MediaTypeBreakdown entries={strength.byMediaType} />}
                </li>
              )
            })}
          </ul>
        </section>
      )}

      {peopleYouTrust.length > 0 && (
        <section>
          <h2>How much you trust people's recommendations</h2>
          <ul>
            {peopleYouTrust.map(({ key, name, averageScore, ratedCount, byMediaType }) => (
              <li key={key}>
                {name}: {ratedCount > 0 ? `${averageScore?.toFixed(1)} / 10 (${ratedCount} rated)` : 'no ratings yet'}
                <MediaTypeBreakdown entries={byMediaType} />
              </li>
            ))}
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
        <h2>Log something someone recommended to you</h2>
        <p className="hint">For recommendations from people who aren't on ThingRecommender.</p>
        <form onSubmit={handleLogManual}>
          <label>
            Their name
            <input
              value={manualRecommenderName}
              onChange={(e) => setManualRecommenderName(e.target.value)}
              placeholder="Nathan"
              required
            />
          </label>
          <label>
            Title
            <input value={manualTitle} onChange={(e) => setManualTitle(e.target.value)} required />
          </label>
          <label>
            Type
            <select value={manualMediaType} onChange={(e) => setManualMediaType(e.target.value as MediaType)}>
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
              value={manualNote}
              onChange={(e) => setManualNote(e.target.value)}
              placeholder="said it was unmissable..."
            />
          </label>
          <button type="submit" disabled={isSubmittingManual}>
            Log it
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

function MediaTypeBreakdown({ entries }: { entries: MediaTypeStrength[] }) {
  const rated = entries.filter((e) => e.ratedCount > 0)
  if (rated.length === 0) return null

  return (
    <ul className="media-type-breakdown">
      {rated.map((e) => (
        <li key={e.mediaType}>
          {e.mediaType}: {e.averageScore?.toFixed(1)} / 10 ({e.ratedCount} rated)
        </li>
      ))}
    </ul>
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
