/** @type {import('tailwindcss').Config} */
module.exports = {
  darkMode: 'class',
  content: [
    './src/**/*.{html,ts}'
  ],
  theme: {
    extend: {
      colors: {
        'app-bg': '#F5F5F0',
        'app-bg-dark': '#191a1f',
        'text-dark': '#3f3f46'
      }
    },
  },
  plugins: [],
}
