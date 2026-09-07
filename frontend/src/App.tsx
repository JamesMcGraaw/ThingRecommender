import { useEffect, useState } from 'react'
import './App.css'

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL ?? 'https://localhost:5001'

type ApiStatus = {
  status: string
  timeUtc: string
}

function App() {
  const [apiStatus, setApiStatus] = useState<ApiStatus | null>(null)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    fetch(`${apiBaseUrl}/api/status`)
      .then((response) => {
        if (!response.ok) throw new Error(`API returned ${response.status}`)
        return response.json() as Promise<ApiStatus>
      })
      .then(setApiStatus)
      .catch((err: Error) => setError(err.message))
  }, [])

  return (
    <main>
      <h1>ThingRecommender</h1>
      <p>Recommend a film, show, book, or restaurant — see how well your taste lines up.</p>
      <p>
        API status:{' '}
        {error && <span>unreachable ({error})</span>}
        {!error && !apiStatus && <span>checking…</span>}
        {apiStatus && <span>{apiStatus.status} @ {apiStatus.timeUtc}</span>}
      </p>
    </main>
  )
}

export default App
