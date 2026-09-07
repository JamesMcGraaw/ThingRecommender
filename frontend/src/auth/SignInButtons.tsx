import { GoogleLogin } from '@react-oauth/google'
import { useMsal } from '@azure/msal-react'
import { isGoogleConfigured, isMicrosoftConfigured } from '../authConfig'
import { useAuth } from './AuthContext'

export function SignInButtons() {
  return (
    <div className="sign-in-buttons">
      {isGoogleConfigured ? <GoogleSignInButton /> : <p className="not-configured">Google sign-in not configured</p>}
      {isMicrosoftConfigured ? (
        <MicrosoftSignInButton />
      ) : (
        <p className="not-configured">Microsoft sign-in not configured</p>
      )}
    </div>
  )
}

function GoogleSignInButton() {
  const { signInWithGoogle } = useAuth()

  return (
    <GoogleLogin
      onSuccess={(credentialResponse) => {
        if (credentialResponse.credential) {
          void signInWithGoogle(credentialResponse.credential)
        }
      }}
      onError={() => console.error('Google sign-in failed')}
    />
  )
}

function MicrosoftSignInButton() {
  const { instance } = useMsal()
  const { signInWithMicrosoft } = useAuth()

  async function handleClick() {
    try {
      const result = await instance.loginPopup({ scopes: ['openid', 'profile', 'email'] })
      if (result.idToken) {
        await signInWithMicrosoft(result.idToken)
      }
    } catch (err) {
      console.error('Microsoft sign-in failed', err)
    }
  }

  return (
    <button type="button" onClick={handleClick}>
      Sign in with Microsoft
    </button>
  )
}
