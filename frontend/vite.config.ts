import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import tailwindcss from '@tailwindcss/vite';
import path from 'node:path';

const usePolling = process.env.VITE_USE_POLLING === '1';

// https://vite.dev/config/
export default defineConfig({
  plugins: [react(), tailwindcss()],
  server: {
    port: 5173,
    // Fail fast if 5173 is taken (e.g. unihub-web nginx on same port) instead of silently switching ports (breaks HMR URL).
    strictPort: true,
    host: true,
    watch: usePolling
      ? { usePolling: true, interval: 300 }
      : undefined,
  },
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src'),
      '@features': path.resolve(__dirname, './src/features'),
      '@shared': path.resolve(__dirname, './src/shared'),
    },
  },
});
