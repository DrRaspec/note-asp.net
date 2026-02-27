/** @type {import('tailwindcss').Config} */
export default {
  content: ["./index.html", "./src/**/*.{ts,vue}"],
  theme: {
    extend: {
      colors: {
        "app-base": "var(--app-bg)",
        "app-panel": "var(--app-panel)",
        "app-text": "var(--app-text)",
        "app-muted": "var(--app-muted)",
        "app-border": "var(--app-border)",
        "app-border-soft": "var(--app-border-soft)",
        "app-accent": "var(--app-accent)",
        "app-accent-strong": "var(--app-accent-strong)"
      },
      boxShadow: {
        soft: "var(--app-shadow)"
      }
    }
  },
  plugins: []
};
