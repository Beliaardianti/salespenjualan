import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    // Frontend memanggil /api/products, proxy meneruskannya ke backend sebagai /products.
    // Awalan /api hanya penanda di sisi frontend, supaya tidak bentrok dengan
    // halaman React yang alamatnya sama (misal halaman /products).
    proxy: {
      '/api': {
        target: 'http://localhost:5258',
        rewrite: (path) => path.replace(/^\/api/, ''),
      },
    },
  },
})
