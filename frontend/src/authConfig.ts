export const GOOGLE_CLIENT_ID = import.meta.env.VITE_GOOGLE_CLIENT_ID ?? ''
export const MICROSOFT_CLIENT_ID = import.meta.env.VITE_MICROSOFT_CLIENT_ID ?? ''
export const MICROSOFT_TENANT_ID = import.meta.env.VITE_MICROSOFT_TENANT_ID ?? 'common'

export const isGoogleConfigured = GOOGLE_CLIENT_ID.length > 0
export const isMicrosoftConfigured = MICROSOFT_CLIENT_ID.length > 0
