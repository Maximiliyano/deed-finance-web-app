module.exports = {
  content: ["./src/**/*.{html,ts}"],
  theme: {
    extend: {
      colors: {
        primary: {
          DEFAULT: '#6366f1',
          hover:   '#4f46e5',
          light:   '#eef2ff',
        }
      },
      fontFamily: {
        sans: ['Inter', 'Roboto', 'sans-serif'],
      },
    },
  },
  plugins: [],
}
