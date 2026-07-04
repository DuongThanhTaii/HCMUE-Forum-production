import { useEffect, useState } from 'react'

export type ThemeMode = 'light' | 'dark' | 'ocean' | 'sunset' | 'forest'

export function useTheme() {
  const [theme, setThemeState] = useState<ThemeMode>('light')

  useEffect(() => {
    const savedTheme = localStorage.getItem('app-theme') as ThemeMode | null
    if (savedTheme) {
      setThemeState(savedTheme)
      applyTheme(savedTheme)
    }
  }, [])

  const setTheme = (newTheme: ThemeMode) => {
    setThemeState(newTheme)
    localStorage.setItem('app-theme', newTheme)
    applyTheme(newTheme)
  }

  const applyTheme = (themeName: ThemeMode) => {
    const root = document.documentElement
    // Remove existing theme classes
    root.classList.remove('theme-light', 'theme-dark', 'theme-ocean', 'theme-sunset', 'theme-forest')
    // Add new theme class
    root.classList.add(`theme-${themeName}`)
  }

  return { theme, setTheme }
}
