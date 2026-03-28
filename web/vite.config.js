import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'

export default defineConfig({
    plugins: [vue()],
    test: {
        environment: 'jsdom',
        globals: true,
    },
    server: {
        proxy: {
            '/api': {
                target: process.env.VITE_API_BASE_URL || 'http://localhost:5000',
                changeOrigin: true,
            }
        }
    }
})
