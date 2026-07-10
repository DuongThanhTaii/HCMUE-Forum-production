import toast from 'react-hot-toast'

const lastNotifiedByThread = new Map<string, string>()

export function notifyInboundChatMessage(opts: {
  threadKey: string
  messageId: string
  title: string
  body: string
}): void {
  const prev = lastNotifiedByThread.get(opts.threadKey)
  if (prev === opts.messageId) return
  lastNotifiedByThread.set(opts.threadKey, opts.messageId)

  // Show an in-app toast notification
  toast(`Tin nhắn mới từ ${opts.title}: ${opts.body}`, {
    icon: '💬',
    duration: 4000,
  })

  // Play notification sound
  try {
    const audio = new Audio('/assets/sounds/message.ogg')
    audio.play().catch(() => {
      // Ignore autoplay errors if user hasn't interacted with the document
    })
  } catch (err) {
    // Ignore environments where Audio is not defined
  }

  if (typeof Notification === 'undefined') return
  if (Notification.permission !== 'granted') return
  if (typeof document !== 'undefined' && document.visibilityState === 'visible') return

  try {
    new Notification(opts.title, { body: opts.body, tag: opts.threadKey })
  } catch {
    // Ignore unsupported environments
  }
}

export function requestChatNotificationPermission(): Promise<NotificationPermission> {
  if (typeof Notification === 'undefined') {
    return Promise.resolve('denied')
  }
  if (Notification.permission === 'granted' || Notification.permission === 'denied') {
    return Promise.resolve(Notification.permission)
  }
  return Notification.requestPermission()
}
