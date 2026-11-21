import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// Ishlab chiqishda /api so'rovlari backend'ga (launchSettings: http://localhost:5148) uzatiladi —
// refresh cookie bir xil origin'da qoladi (BR-AUTH-05).
export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      '/api': { target: 'http://localhost:5148', changeOrigin: false },
    },
  },
})
