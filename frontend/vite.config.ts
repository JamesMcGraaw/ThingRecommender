import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

// https://vite.dev/config/
export default defineConfig(({ command }) => ({
  plugins: [react()],
  // GitHub Pages serves project sites under /<repo>/ - only apply that prefix
  // for production builds so local dev still runs at the root.
  base: command === 'build' ? '/ThingRecommender/' : '/',
}))
