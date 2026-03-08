import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import { resolve } from 'path'

export default defineConfig({
  plugins: [
    vue(),
  ],
  resolve: {
    alias: [
      { find: '@assets', replacement: resolve(__dirname, 'src/assets') },
      { find: '@runtime', replacement: resolve(__dirname, 'src/runtime') },
      { find: '@pages', replacement: resolve(__dirname, 'src/pages') },
      { find: '@components', replacement: resolve(__dirname, 'src/components') },
    ]
  },
  build: {
    outDir: './dist/',
    emptyOutDir: true,
    assetsDir: 'assets',
  },
  server: {
    cors: true,
    host: "0.0.0.0",
    open: false,
    proxy: {
      '/api/wallpaper': {
        target: 'https://www.bing.com',
        changeOrigin: true,
        rewrite: _ => '/HPImageArchive.aspx?format=js&idx=0&n=1&mkt=zh-CN',
      },
      '/api': {
        target: 'http://127.0.0.1:12345/api',
        changeOrigin: true,
        rewrite: path => path.replace(/^\/api/, '')
      },
    },
  }
})