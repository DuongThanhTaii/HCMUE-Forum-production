const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:5034'

export function getChatHubUrl(): string {
  const base = API_URL.replace(/\/$/, '')
  return `${base}/hubs/chat`
}
