import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { GoogleOAuthProvider } from '@react-oauth/google'
import { MsalProvider } from '@azure/msal-react'
import { PublicClientApplication } from '@azure/msal-browser'
import './index.css'
import App from './App.tsx'
import { AuthProvider } from './auth/AuthContext'
import { GOOGLE_CLIENT_ID, MICROSOFT_CLIENT_ID, MICROSOFT_TENANT_ID, isMicrosoftConfigured } from './authConfig'

const msalInstance = isMicrosoftConfigured
  ? new PublicClientApplication({
      auth: {
        clientId: MICROSOFT_CLIENT_ID,
        authority: `https://login.microsoftonline.com/${MICROSOFT_TENANT_ID}`,
        redirectUri: window.location.origin,
      },
    })
  : null

function Root() {
  const content = (
    <AuthProvider>
      <App />
    </AuthProvider>
  )

  return (
    <GoogleOAuthProvider clientId={GOOGLE_CLIENT_ID}>
      {msalInstance ? <MsalProvider instance={msalInstance}>{content}</MsalProvider> : content}
    </GoogleOAuthProvider>
  )
}

async function bootstrap() {
  if (msalInstance) {
    await msalInstance.initialize()
  }

  createRoot(document.getElementById('root')!).render(
    <StrictMode>
      <Root />
    </StrictMode>,
  )
}

void bootstrap()
