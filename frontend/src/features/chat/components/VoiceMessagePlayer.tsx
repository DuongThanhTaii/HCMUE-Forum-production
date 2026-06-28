import { useEffect, useRef, useState } from 'react'
import { useTranslation } from 'react-i18next'

function formatMmSs(seconds: number): string {
  if (!Number.isFinite(seconds) || seconds < 0) return '0:00'
  const m = Math.floor(seconds / 60)
  const s = Math.floor(seconds % 60)
  return `${m}:${s.toString().padStart(2, '0')}`
}

/**
 * Browsers often leave `duration` as 0, NaN, or Infinity for webm/opus until enough
 * data is buffered. `seekable` end is a reliable fallback for static file URLs.
 */
function readBestDurationSeconds(el: HTMLAudioElement): number {
  const d = el.duration
  if (Number.isFinite(d) && d > 0 && d !== Number.POSITIVE_INFINITY) {
    return d
  }
  try {
    if (el.seekable && el.seekable.length > 0) {
      const end = el.seekable.end(el.seekable.length - 1)
      if (Number.isFinite(end) && end > 0 && end !== Number.POSITIVE_INFINITY) {
        return end
      }
    }
  } catch {
    /* ignore */
  }
  return 0
}

export function VoiceMessagePlayer({
  src,
  isSelf,
}: {
  src: string
  isSelf: boolean
}) {
  const { t } = useTranslation()
  const audioRef = useRef<HTMLAudioElement | null>(null)
  const [playing, setPlaying] = useState(false)
  const [duration, setDuration] = useState(0)
  const [current, setCurrent] = useState(0)
  const [loadError, setLoadError] = useState(false)

  useEffect(() => {
    const el = audioRef.current
    if (!el) return

    const refreshDuration = () => {
      const sec = readBestDurationSeconds(el)
      if (sec <= 0) return
      setDuration((prev) => (Math.abs(prev - sec) < 0.02 ? prev : sec))
    }

    const onMeta = () => refreshDuration()
    const onDurationChange = () => refreshDuration()
    const onProgress = () => refreshDuration()
    const onCanPlay = () => refreshDuration()
    const onTime = () => {
      setCurrent(el.currentTime)
      refreshDuration()
    }
    const onEnd = () => setPlaying(false)
    const onPause = () => setPlaying(false)
    const onPlay = () => {
      setPlaying(true)
      refreshDuration()
    }
    const onError = () => setLoadError(true)

    el.addEventListener('loadedmetadata', onMeta)
    el.addEventListener('durationchange', onDurationChange)
    el.addEventListener('progress', onProgress)
    el.addEventListener('loadeddata', onCanPlay)
    el.addEventListener('canplay', onCanPlay)
    el.addEventListener('canplaythrough', onCanPlay)
    el.addEventListener('timeupdate', onTime)
    el.addEventListener('ended', onEnd)
    el.addEventListener('pause', onPause)
    el.addEventListener('play', onPlay)
    el.addEventListener('error', onError)

    el.load()

    return () => {
      el.removeEventListener('loadedmetadata', onMeta)
      el.removeEventListener('durationchange', onDurationChange)
      el.removeEventListener('progress', onProgress)
      el.removeEventListener('loadeddata', onCanPlay)
      el.removeEventListener('canplay', onCanPlay)
      el.removeEventListener('canplaythrough', onCanPlay)
      el.removeEventListener('timeupdate', onTime)
      el.removeEventListener('ended', onEnd)
      el.removeEventListener('pause', onPause)
      el.removeEventListener('play', onPlay)
      el.removeEventListener('error', onError)
    }
  }, [src])

  const toggle = () => {
    const el = audioRef.current
    if (!el || loadError) return
    if (playing) {
      el.pause()
    } else {
      void el.play().catch(() => setLoadError(true))
    }
  }

  const progress = duration > 0 ? Math.min(100, (current / duration) * 100) : 0
  const totalLabel = duration > 0 ? formatMmSs(duration) : '—'

  return (
    <div
      className={`flex max-w-[min(100%,280px)] items-center gap-2 rounded-full px-2 py-1.5 pl-2 ${
        isSelf ? 'bg-indigo-700/95 text-white' : 'bg-slate-200 text-slate-900'
      }`}
    >
      <audio ref={audioRef} src={src} preload="auto" className="hidden" />
      <button
        type="button"
        onClick={toggle}
        aria-label={playing ? t('chat.voice.pause') : t('chat.voice.play')}
        className={`flex h-10 w-10 shrink-0 items-center justify-center rounded-full shadow-sm transition ${
          isSelf
            ? 'bg-white text-indigo-700 hover:bg-indigo-50'
            : 'bg-white text-slate-800 hover:bg-slate-50'
        }`}
      >
        {playing ? (
          <span className="flex gap-0.5" aria-hidden>
            <span className="h-3 w-0.5 rounded-sm bg-current" />
            <span className="h-3 w-0.5 rounded-sm bg-current" />
          </span>
        ) : (
          <svg viewBox="0 0 24 24" className="h-5 w-5 pl-0.5" fill="currentColor" aria-hidden>
            <path d="M8 5v14l11-7z" />
          </svg>
        )}
      </button>
      <div className="min-w-0 flex-1 py-0.5">
        <div
          className={`mb-1 h-1.5 overflow-hidden rounded-full ${
            isSelf ? 'bg-indigo-400/40' : 'bg-slate-400/35'
          }`}
        >
          <div
            className={`h-full rounded-full transition-[width] duration-150 ${
              isSelf ? 'bg-white' : 'bg-indigo-500'
            }`}
            style={{ width: `${progress}%` }}
          />
        </div>
        <div className="flex items-center justify-between gap-2 text-[10px] tabular-nums leading-none">
          <span className={isSelf ? 'text-indigo-100' : 'text-slate-600'}>
            {loadError ? t('chat.voice.loadError') : t('chat.voice.messageBubble')}
          </span>
          <span className={isSelf ? 'text-indigo-100' : 'text-slate-500'}>
            {formatMmSs(current)} · {totalLabel}
          </span>
        </div>
      </div>
    </div>
  )
}
