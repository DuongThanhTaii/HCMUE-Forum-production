import { useCallback, useEffect, useRef, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useAppSelector } from '@shared/hooks/useAppSelector'
import { useChatContext } from '../context/ChatContext'
import type { WebRtcSignalPayload } from '../types/chat.types'

// ─── helpers ──────────────────────────────────────────────────────────────────

function parseSessionDescription(json: string): RTCSessionDescriptionInit {
  const o = JSON.parse(json) as { type: RTCSdpType; sdp: string }
  return { type: o.type, sdp: o.sdp }
}

function serializeSdp(d: RTCSessionDescription | RTCSessionDescriptionInit): string {
  return JSON.stringify({ type: d.type, sdp: d.sdp })
}

function sdpHasVideo(sdp: string): boolean {
  return /(^|\r|\n)m=video /m.test(sdp)
}

function defaultIceServers(): RTCIceServer[] {
  try {
    const raw = import.meta.env.VITE_WEBRTC_ICE_SERVERS
    if (typeof raw === 'string' && raw.trim().length > 0) {
      return JSON.parse(raw) as RTCIceServer[]
    }
  } catch { /* ignore */ }
  return [
    { urls: 'stun:stun.l.google.com:19302' },
    { urls: 'stun:stun1.l.google.com:19302' },
  ]
}

function extractRelayError(e: unknown, t: (k: string) => string): string {
  const m = e instanceof Error ? e.message : String(e)
  if (m.includes('chat_hub_not_connected')) return t('chat.calls.hubNotConnected')
  if (m.includes('webrtc_peer_offline')) return t('chat.calls.peerOffline')
  return m
}

// ─── types ────────────────────────────────────────────────────────────────────

export type CallPhase = 'idle' | 'outgoing' | 'incoming' | 'connected'
export type CallUiMode = 'voice' | 'video' | 'screen'

type IncomingPending = {
  fromUserId: string
  fromUserName: string
  offerPayload: string
  wantsVideo: boolean
}

// ─── hook ─────────────────────────────────────────────────────────────────────

export function useWebRtcCall(options: {
  conversationId: string
  remoteUserId: string | null
  canCall: boolean
}) {
  const { conversationId, remoteUserId, canCall } = options
  const { subscribeWebRtcSignal, relayWebRtcSignal, reportMissedCall, reportCallEnded } = useChatContext()
  const { t } = useTranslation()
  const selfId = useAppSelector((s) => s.auth.user?.id ?? null)

  const [phase, setPhase] = useState<CallPhase>('idle')
  const [callMode, setCallMode] = useState<CallUiMode>('voice')
  const [error, setError] = useState<string | null>(null)
  const [localStream, setLocalStream] = useState<MediaStream | null>(null)
  const [remoteStream, setRemoteStream] = useState<MediaStream | null>(null)
  const [incoming, setIncoming] = useState<IncomingPending | null>(null)
  const [muted, setMuted] = useState(false)

  const pcRef = useRef<RTCPeerConnection | null>(null)
  const localStreamRef = useRef<MediaStream | null>(null)
  const iceQueueRef = useRef<RTCIceCandidateInit[]>([])
  const phaseRef = useRef<CallPhase>('idle')
  const remoteSetRef = useRef(false)
  const lastRemoteUserRef = useRef<string | null>(null)
  const connectedAtRef = useRef<number | null>(null)

  // ── primitives ──────────────────────────────────────────────────────────────

  const syncPhase = useCallback((p: CallPhase) => {
    phaseRef.current = p
    setPhase(p)
  }, [])

  const stopLocalTracks = useCallback(() => {
    localStreamRef.current?.getTracks().forEach((t) => { try { t.stop() } catch { /**/ } })
    localStreamRef.current = null
    setLocalStream(null)
  }, [])

  const closePeer = useCallback(() => {
    const pc = pcRef.current
    if (pc) {
      pc.ontrack = null
      pc.onicecandidate = null
      pc.onnegotiationneeded = null
      try { pc.close() } catch { /**/ }
      pcRef.current = null
    }
    iceQueueRef.current = []
    remoteSetRef.current = false
    setRemoteStream(null)
  }, [])

  /**
   * Reset media + peer state. Does NOT clear `error` — callers decide whether
   * to show or clear an error message.
   */
  const resetCallState = useCallback(() => {
    stopLocalTracks()
    closePeer()
    syncPhase('idle')
    connectedAtRef.current = null
    setIncoming(null)
    setMuted(false)
  }, [closePeer, stopLocalTracks, syncPhase])

  const flushIce = useCallback(async (pc: RTCPeerConnection) => {
    const q = iceQueueRef.current.splice(0)
    for (const c of q) {
      try { await pc.addIceCandidate(c) } catch { /**/ }
    }
  }, [])

  const relay = useCallback(
    async (targetUserId: string, kind: string, payload: string) => {
      await relayWebRtcSignal(conversationId, targetUserId, kind, payload)
    },
    [conversationId, relayWebRtcSignal]
  )

  const setupPeer = useCallback((targetUserId: string): RTCPeerConnection => {
    const pc = new RTCPeerConnection({ iceServers: defaultIceServers() })
    pcRef.current = pc
    lastRemoteUserRef.current = targetUserId

    pc.onicecandidate = (ev) => {
      if (!ev.candidate) return
      void relay(targetUserId, 'ice', JSON.stringify(ev.candidate.toJSON()))
    }
    pc.ontrack = (ev) => {
      if (ev.streams[0]) setRemoteStream(ev.streams[0])
    }
    return pc
  }, [relay])

  const attachRenegotiation = useCallback((pc: RTCPeerConnection, targetUserId: string) => {
    pc.onnegotiationneeded = async () => {
      if (phaseRef.current !== 'connected') return
      try {
        const offer = await pc.createOffer()
        await pc.setLocalDescription(offer)
        await relay(targetUserId, 'offer', serializeSdp(pc.localDescription!))
      } catch (e) {
        setError(extractRelayError(e, t))
      }
    }
  }, [relay, t])

  // ── outgoing call ────────────────────────────────────────────────────────────

  const startOutgoing = useCallback(async (mode: CallUiMode) => {
    if (!canCall || !remoteUserId || !selfId) return

    // --- synchronous state reset (all batched by React) ---
    resetCallState()
    setError(null)
    setCallMode(mode)
    syncPhase('outgoing')

    try {
      const stream = await (
        mode === 'screen'
          ? navigator.mediaDevices.getDisplayMedia({ video: true, audio: true })
          : navigator.mediaDevices.getUserMedia({ video: mode === 'video', audio: true })
      )
      localStreamRef.current = stream
      setLocalStream(stream)

      const pc = setupPeer(remoteUserId)
      stream.getTracks().forEach((t) => pc.addTrack(t, stream))

      const offer = await pc.createOffer()
      await pc.setLocalDescription(offer)
      await relay(remoteUserId, 'offer', serializeSdp(pc.localDescription!))
    } catch (e) {
      // Reset FIRST, then set error — React batches these; error wins (last write).
      resetCallState()
      setError(extractRelayError(e, t))
    }
  }, [canCall, remoteUserId, selfId, resetCallState, syncPhase, setupPeer, relay, t])

  // ── accept incoming ──────────────────────────────────────────────────────────

  const acceptIncoming = useCallback(async () => {
    const pending = incoming
    if (!pending || !selfId) return
    setError(null)
    setIncoming(null)

    try {
      const desc = parseSessionDescription(pending.offerPayload)
      const stream = await navigator.mediaDevices.getUserMedia({
        video: pending.wantsVideo,
        audio: true,
      })
      localStreamRef.current = stream
      setLocalStream(stream)

      const pc = setupPeer(pending.fromUserId)
      stream.getTracks().forEach((t) => pc.addTrack(t, stream))

      await pc.setRemoteDescription(desc)
      remoteSetRef.current = true
      await flushIce(pc)

      const answer = await pc.createAnswer()
      await pc.setLocalDescription(answer)
      await relay(pending.fromUserId, 'answer', serializeSdp(pc.localDescription!))

      setCallMode(pending.wantsVideo ? 'video' : 'voice')
      syncPhase('connected')
      connectedAtRef.current = Date.now()
      attachRenegotiation(pc, pending.fromUserId)
    } catch (e) {
      resetCallState()
      setError(extractRelayError(e, t))
    }
  }, [attachRenegotiation, flushIce, incoming, relay, resetCallState, selfId, setupPeer, syncPhase, t])

  // ── reject / end ─────────────────────────────────────────────────────────────

  const rejectIncoming = useCallback(async () => {
    if (!incoming) return
    try { await relay(incoming.fromUserId, 'hangup', '{}') } catch { /**/ }
    resetCallState()
    setError(null)
  }, [incoming, relay, resetCallState])

  const endCall = useCallback(async () => {
    const prevPhase = phaseRef.current
    const target = lastRemoteUserRef.current ?? remoteUserId
    if (target) {
      try { await relay(target, 'hangup', '{}') } catch { /**/ }
    }
    if (prevPhase === 'outgoing') void reportMissedCall(conversationId)
    if (prevPhase === 'connected') {
      const startedAt = connectedAtRef.current
      const durationSeconds =
        startedAt != null ? Math.max(1, Math.round((Date.now() - startedAt) / 1000)) : undefined
      void reportCallEnded(conversationId, durationSeconds)
    }
    resetCallState()
    setError(null)
  }, [conversationId, relay, remoteUserId, reportCallEnded, reportMissedCall, resetCallState])

  const dismissError = useCallback(() => setError(null), [])

  // ── media controls ───────────────────────────────────────────────────────────

  const toggleMute = useCallback(() => {
    const s = localStreamRef.current
    if (!s) return
    const next = !muted
    s.getAudioTracks().forEach((t) => { t.enabled = !next })
    setMuted(next)
  }, [muted])

  const replaceWithScreen = useCallback(async () => {
    const pc = pcRef.current
    if (!pc || phaseRef.current !== 'connected') return
    try {
      const screen = await navigator.mediaDevices.getDisplayMedia({ video: true, audio: true })
      const vt = screen.getVideoTracks()[0]
      if (!vt) return
      const old = localStreamRef.current
      old?.getVideoTracks().forEach((t) => { if (t.id !== vt.id) t.stop() })
      const sender = pc.getSenders().find((s) => s.track?.kind === 'video')
      if (sender) {
        const prev = sender.track
        await sender.replaceTrack(vt)
        prev?.stop()
      } else {
        pc.addTrack(vt, new MediaStream([vt, ...(old?.getAudioTracks() ?? [])]))
      }
      const newStream = new MediaStream([vt, ...(old?.getAudioTracks() ?? [])])
      localStreamRef.current = newStream
      setLocalStream(newStream)
      setCallMode('screen')
    } catch (e) {
      setError(e instanceof Error ? e.message : String(e))
    }
  }, [])

  const stopScreenShare = useCallback(async () => {
    const pc = pcRef.current
    if (!pc || phaseRef.current !== 'connected' || callMode !== 'screen') return
    try {
      const cam = await navigator.mediaDevices.getUserMedia({ video: true, audio: false })
      const vt = cam.getVideoTracks()[0]
      const sender = pc.getSenders().find((s) => s.track?.kind === 'video')
      const old = localStreamRef.current
      old?.getVideoTracks().forEach((t) => t.stop())
      if (sender && vt) {
        await sender.replaceTrack(vt)
        const newStream = new MediaStream([vt, ...(old?.getAudioTracks() ?? [])])
        localStreamRef.current = newStream
        setLocalStream(newStream)
        setCallMode('video')
      }
    } catch (e) {
      setError(e instanceof Error ? e.message : String(e))
    }
  }, [callMode])

  // ── signal handler ───────────────────────────────────────────────────────────

  const handleRemoteSignal = useCallback(async (sig: WebRtcSignalPayload) => {
    if (!canCall || !selfId) return
    if (sig.conversationId !== conversationId) return
    if (sig.fromUserId === selfId) return

    const peerId = sig.fromUserId
    const k = sig.kind

    if (k === 'hangup') {
      if (phaseRef.current !== 'idle') {
        resetCallState()
        setError(null)
      }
      setIncoming(null)
      return
    }

    if (k === 'ice') {
      let cand: RTCIceCandidateInit
      try { cand = JSON.parse(sig.payload) as RTCIceCandidateInit } catch { return }
      const pc = pcRef.current
      if (!pc || !remoteSetRef.current) { iceQueueRef.current.push(cand); return }
      try { await pc.addIceCandidate(cand) } catch { iceQueueRef.current.push(cand) }
      return
    }

    if (k === 'offer') {
      if (phaseRef.current === 'connected' && pcRef.current) {
        const pc = pcRef.current
        try {
          const d = parseSessionDescription(sig.payload)
          // Remote may add a video track later (e.g. start screen share from voice call).
          // Switch UI from voice-only to video mode when renegotiation offer contains video.
          if (sdpHasVideo(d.sdp ?? '') && callMode === 'voice') {
            setCallMode('video')
          }
          await pc.setRemoteDescription(d)
          remoteSetRef.current = true
          await flushIce(pc)
          const ans = await pc.createAnswer()
          await pc.setLocalDescription(ans)
          await relay(peerId, 'answer', serializeSdp(pc.localDescription!))
        } catch { /**/ }
        return
      }
      if (phaseRef.current === 'outgoing') return

      const od = parseSessionDescription(sig.payload)
      const wantsVideo = sdpHasVideo(od.sdp ?? '')
      setCallMode(wantsVideo ? 'video' : 'voice')
      setIncoming({ fromUserId: peerId, fromUserName: sig.fromUserName || peerId.slice(0, 8), offerPayload: sig.payload, wantsVideo })
      syncPhase('incoming')
      return
    }

    if (k === 'answer') {
      const pc = pcRef.current
      if (!pc) return
      try {
        const d = parseSessionDescription(sig.payload)
        await pc.setRemoteDescription(d)
        remoteSetRef.current = true
        await flushIce(pc)
        syncPhase('connected')
        connectedAtRef.current = Date.now()
        attachRenegotiation(pc, peerId)
      } catch (e) {
        resetCallState()
        setError(extractRelayError(e, t))
      }
    }
  }, [attachRenegotiation, callMode, canCall, conversationId, flushIce, relay, resetCallState, selfId, syncPhase, t])

  // ── effects ──────────────────────────────────────────────────────────────────

  useEffect(() => {
    if (!canCall || !conversationId || !selfId) return
    return subscribeWebRtcSignal((sig) => { void handleRemoteSignal(sig) })
  }, [canCall, conversationId, handleRemoteSignal, selfId, subscribeWebRtcSignal])

  // Strict Mode note: do NOT clean up RTCPeerConnection on unmount.
  // React Strict Mode does mount→unmount→mount in dev which would kill an active call.

  return {
    phase,
    callMode,
    error,
    localStream,
    remoteStream,
    incoming,
    muted,
    startVoice: () => void startOutgoing('voice'),
    startVideo: () => void startOutgoing('video'),
    startScreenCall: () => void startOutgoing('screen'),
    acceptIncoming,
    rejectIncoming,
    endCall,
    dismissError,
    toggleMute,
    replaceWithScreen,
    stopScreenShare,
    setCallMode,
  }
}
