import { createContext, useContext, useEffect, useState, type ReactNode } from 'react'
import { getMe, signIn as apiSignIn, type User } from '../api'

const TOKEN_STORAGE_KEY = 'thingrecommender.token'

interface AuthContextValue {
  user: User | null
  token: string | null
  loading: boolean
  signInWithGoogle: (idToken: string) => Promise<void>
  signInWithMicrosoft: (idToken: string) => Promise<void>
  signOut: () => void
}

const AuthContext = createContext<AuthContextValue | null>(null)

export function AuthProvider({ children }: { children: ReactNode }) {
  const [token, setToken] = useState<string | null>(() => localStorage.getItem(TOKEN_STORAGE_KEY))
  const [user, setUser] = useState<User | null>(null)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    if (!token) {
      setLoading(false)
      return
    }

    getMe(token)
      .then(setUser)
      .catch(() => {
        localStorage.removeItem(TOKEN_STORAGE_KEY)
        setToken(null)
      })
      .finally(() => setLoading(false))
    // Only ever re-check the token that was in storage when the app loaded.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  async function completeSignIn(provider: 'google' | 'microsoft', idToken: string) {
    const result = await apiSignIn(provider, idToken)
    localStorage.setItem(TOKEN_STORAGE_KEY, result.token)
    setToken(result.token)
    setUser(result.user)
  }

  function signOut() {
    localStorage.removeItem(TOKEN_STORAGE_KEY)
    setToken(null)
    setUser(null)
  }

  return (
    <AuthContext.Provider
      value={{
        user,
        token,
        loading,
        signInWithGoogle: (idToken) => completeSignIn('google', idToken),
        signInWithMicrosoft: (idToken) => completeSignIn('microsoft', idToken),
        signOut,
      }}
    >
      {children}
    </AuthContext.Provider>
  )
}

export function useAuth(): AuthContextValue {
  const context = useContext(AuthContext)
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider')
  }
  return context
}
