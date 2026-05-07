/** @type {import('tailwindcss').Config} */
export default {
  content: ['./index.html', './src/**/*.{ts,tsx}'],
  theme: {
    extend: {
      colors: {
        // Inspired by reference: deep teal/navy ground with crimson accent
        ink: {
          900: '#061a22',
          800: '#0a2532',
          700: '#0e3142',
          600: '#143c50',
          500: '#1c4c63',
        },
        accent: {
          50: '#fdecf1',
          100: '#fbd0dc',
          200: '#f5a4b8',
          300: '#ec7691',
          400: '#df5474',
          500: '#d6486a',
          600: '#c23659',
          700: '#9d2a48',
          800: '#761f37',
          900: '#4f1525',
        },
      },
      fontFamily: {
        sans: ['Inter', 'system-ui', '-apple-system', 'Segoe UI', 'Roboto', 'sans-serif'],
        serif: ['"Source Serif Pro"', 'Charter', 'Cambria', 'Georgia', 'serif'],
      },
      boxShadow: {
        glow: '0 0 80px -10px rgba(45, 212, 191, 0.18)',
        card: '0 20px 50px -20px rgba(0, 0, 0, 0.6)',
        'card-hover': '0 28px 70px -20px rgba(214, 72, 106, 0.35)',
      },
      backgroundImage: {
        'radial-glow':
          'radial-gradient(ellipse 90% 60% at 80% 30%, rgba(45, 212, 191, 0.18), transparent 60%)',
        'card-gradient':
          'linear-gradient(160deg, rgba(255,255,255,0.04) 0%, rgba(255,255,255,0.01) 100%)',
      },
      keyframes: {
        'fade-in': {
          '0%': { opacity: '0', transform: 'translateY(8px)' },
          '100%': { opacity: '1', transform: 'translateY(0)' },
        },
        'slide-in-right': {
          '0%': { opacity: '0', transform: 'translateX(20px)' },
          '100%': { opacity: '1', transform: 'translateX(0)' },
        },
        'pulse-soft': {
          '0%, 100%': { opacity: '0.6' },
          '50%': { opacity: '1' },
        },
      },
      animation: {
        'fade-in': 'fade-in 280ms ease-out',
        'slide-in-right': 'slide-in-right 220ms ease-out',
        'pulse-soft': 'pulse-soft 1.6s ease-in-out infinite',
      },
    },
  },
  plugins: [],
};
