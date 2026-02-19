/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./Views/**/*.cshtml",
    "./wwwroot/**/*.html",
    "./wwwroot/**/*.js"
  ],
  darkMode: 'class',
  theme: {
    extend: {
      colors: {
        'brand-black': '#0a0a0a',
        'brand-dark': '#121212',
        'brand-red': {
          light: '#ff4d4d',
          DEFAULT: '#e60000',
          dark: '#990000',
        },
        'brand-navy': {
          light: '#1a2a6c',
          DEFAULT: '#0f172a',
          dark: '#050b18',
        }
      },
      animation: {
        'pulse-slow': 'pulse 3s cubic-bezier(0.4, 0, 0.6, 1) infinite',
      }
    },
  },
  plugins: [],
}
