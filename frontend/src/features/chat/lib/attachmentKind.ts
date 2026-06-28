export function primaryMime(mime: string): string {
  const s = mime.trim()
  const i = s.indexOf(';')
  return i >= 0 ? s.slice(0, i).trim().toLowerCase() : s.toLowerCase()
}

export function isImageAttachment(mimeType: string, fileName: string): boolean {
  const mime = primaryMime(mimeType)
  if (mime.startsWith('image/')) return true
  const ext = fileName.split('.').pop()?.toLowerCase() ?? ''
  return ['jpg', 'jpeg', 'png', 'gif', 'webp', 'bmp', 'svg'].includes(ext)
}

export function isVoiceAttachment(mimeType: string, fileName: string): boolean {
  const mime = primaryMime(mimeType)
  if (mime.startsWith('audio/')) return true
  const ext = fileName.split('.').pop()?.toLowerCase() ?? ''
  return ['webm', 'ogg', 'm4a', 'mp3', 'wav', 'aac'].includes(ext)
}

export function isFileAttachment(mimeType: string, fileName: string): boolean {
  return !isImageAttachment(mimeType, fileName) && !isVoiceAttachment(mimeType, fileName)
}
