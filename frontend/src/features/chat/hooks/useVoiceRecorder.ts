import { useCallback, useRef, useState } from 'react'

type RecorderState = 'idle' | 'recording' | 'stopped'

function createMediaRecorder(stream: MediaStream): MediaRecorder {
  const candidates = [
    'audio/webm;codecs=opus',
    'audio/webm',
    'audio/ogg;codecs=opus',
    'audio/mp4',
    '',
  ]
  for (const mime of candidates) {
    try {
      if (mime && !MediaRecorder.isTypeSupported(mime)) continue
      return mime ? new MediaRecorder(stream, { mimeType: mime }) : new MediaRecorder(stream)
    } catch {
      continue
    }
  }
  return new MediaRecorder(stream)
}

export function useVoiceRecorder() {
  const [state, setState] = useState<RecorderState>('idle')
  const [seconds, setSeconds] = useState(0)
  const [error, setError] = useState<string | null>(null)
  const [blob, setBlob] = useState<Blob | null>(null)
  const [lastMime, setLastMime] = useState<string>('audio/webm')

  const mediaRecorderRef = useRef<MediaRecorder | null>(null)
  const chunksRef = useRef<Blob[]>([])
  const streamRef = useRef<MediaStream | null>(null)
  const tickRef = useRef<ReturnType<typeof setInterval> | null>(null)
  const startedAtRef = useRef<number | null>(null)
  const skipBlobOnStopRef = useRef(false)

  const clearTick = useCallback(() => {
    if (tickRef.current) {
      clearInterval(tickRef.current)
      tickRef.current = null
    }
  }, [])

  const reset = useCallback(() => {
    clearTick()
    if (streamRef.current) {
      for (const t of streamRef.current.getTracks()) {
        t.stop()
      }
      streamRef.current = null
    }
    mediaRecorderRef.current = null
    chunksRef.current = []
    setSeconds(0)
    setBlob(null)
    setError(null)
    setState('idle')
    startedAtRef.current = null
    skipBlobOnStopRef.current = false
  }, [clearTick])

  const start = useCallback(async () => {
    setError(null)
    setBlob(null)
    chunksRef.current = []
    if (!navigator.mediaDevices?.getUserMedia) {
      setError('Microphone not supported')
      return
    }
    try {
      const stream = await navigator.mediaDevices.getUserMedia({ audio: true })
      streamRef.current = stream
      const rec = createMediaRecorder(stream)
      mediaRecorderRef.current = rec
      setLastMime(rec.mimeType || 'audio/webm')
      rec.ondataavailable = (ev) => {
        if (ev.data.size > 0) chunksRef.current.push(ev.data)
      }
      const sliceMs = 250
      rec.start(sliceMs)
      setState('recording')
      startedAtRef.current = Date.now()
      setSeconds(0)
      clearTick()
      tickRef.current = setInterval(() => {
        if (startedAtRef.current) {
          setSeconds(Math.floor((Date.now() - startedAtRef.current) / 1000))
        }
      }, 500)
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Microphone error')
      reset()
    }
  }, [clearTick, reset])

  const cancelRecording = useCallback(() => {
    const rec = mediaRecorderRef.current
    clearTick()
    if (!rec || rec.state === 'inactive') {
      reset()
      return
    }
    skipBlobOnStopRef.current = true
    rec.onstop = () => {
      skipBlobOnStopRef.current = false
      chunksRef.current = []
      if (streamRef.current) {
        for (const t of streamRef.current.getTracks()) {
          t.stop()
        }
        streamRef.current = null
      }
      mediaRecorderRef.current = null
      setBlob(null)
      setSeconds(0)
      setState('idle')
      startedAtRef.current = null
    }
    try {
      if (rec.state === 'recording') {
        rec.requestData()
      }
    } catch {
      // ignore
    }
    rec.stop()
  }, [clearTick, reset])

  const finishRecording = useCallback(() => {
    const rec = mediaRecorderRef.current
    if (!rec || rec.state === 'inactive') {
      reset()
      return
    }
    skipBlobOnStopRef.current = false
    rec.onstop = () => {
      const type = rec.mimeType || lastMime || 'audio/webm'
      setLastMime(type)
      const b = new Blob(chunksRef.current, { type })
      if (b.size === 0) {
        setError('VOICE_EMPTY')
        setState('idle')
      } else {
        setBlob(b)
        setState('stopped')
      }
      clearTick()
      if (streamRef.current) {
        for (const t of streamRef.current.getTracks()) {
          t.stop()
        }
        streamRef.current = null
      }
      mediaRecorderRef.current = null
      startedAtRef.current = null
    }
    try {
      if (rec.state === 'recording') {
        rec.requestData()
      }
    } catch {
      // ignore
    }
    rec.stop()
  }, [clearTick, lastMime, reset])

  return {
    state,
    seconds,
    error,
    blob,
    lastMime,
    start,
    /** End recording and keep blob for send / preview. */
    finishRecording,
    /** Abort recording without keeping audio (fixes race with reset). */
    cancelRecording,
    reset,
  }
}
