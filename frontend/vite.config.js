import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      // Proxy API requests
      '/api': {
        target: 'http://localhost:5232',
        changeOrigin: true,
      },
      // Proxy SignalR WebSocket connections
      '/chathub': {
        target: 'ws://localhost:5232',
        ws: true,
      },
    }
  }
})
