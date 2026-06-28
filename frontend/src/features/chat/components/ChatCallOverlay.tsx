import { useEffect, useRef, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Maximize2, Mic, MicOff, Minimize2, MonitorUp, MonitorX, PhoneOff, Video, VideoOff } from 'lucide-react'
import type { CallPhase, CallUiMode } from '../hooks/useWebRtcCall'

type Props = {
  phase: CallPhase
  callMode: CallUiMode
  remoteLabel: string
  localStream: MediaStream | null
  remoteStream: MediaStream | null
  error: string | null
  muted: boolean
  onAccept: () => void
  onReject: () => void
  onEnd: () => void
  onToggleMute: () => void
  onShareScreen: () => void
  onStopScreenShare: () => void
  onDismissError: () => void
  incomingFromName: string | null
}

export function ChatCallOverlay({
  phase,
  callMode,
  remoteLabel,
  localStream,
  remoteStream,
  error,
  muted,
  onAccept,
  onReject,
  onEnd,
  onToggleMute,
  onShareScreen,
  onStopScreenShare,
  onDismissError,
  incomingFromName,
}: Props) {
  const { t } = useTranslation()
  const [isFullscreen, setIsFullscreen] = useState(false)
  const mainStageRef = useRef<HTMLVideoElement>(null)
  const localPipRef = useRef<HTMLVideoElement>(null)
  const dialogRef = useRef<HTMLDivElement>(null)

  /**
   * Main stage shows LOCAL stream when:
   * - outgoing (ringing) with video/screen — caller preview before remote connects
   * - connected + screen share — "you are presenting" view (your screen is the focus)
   */
  const mainShowsLocal =
    (callMode !== 'voice' && phase === 'outgoing' && Boolean(localStream)) ||
    (callMode === 'screen' && phase === 'connected' && Boolean(localStream))

  const mainStream = mainShowsLocal ? localStream : remoteStream
  /**
   * PIP: when main shows local (screen share), pip shows remote video if available;
   * otherwise when main shows remote, pip shows local camera.
   */
  const pipStream = mainShowsLocal ? remoteStream : localStream
  const showPip = phase === 'connected' && Boolean(pipStream)

  useEffect(() => {
    const el = mainStageRef.current
    if (!el) return
    el.srcObject = mainStream ?? null
    void el.play().catch(() => undefined)
  }, [mainStream])

  useEffect(() => {
    const el = localPipRef.current
    if (!el) return
    el.srcObject = pipStream ?? null
    void el.play().catch(() => undefined)
  }, [pipStream])

  useEffect(() => {
    const onFsChange = () => setIsFullscreen(Boolean(document.fullscreenElement))
    document.addEventListener('fullscreenchange', onFsChange)
    return () => document.removeEventListener('fullscreenchange', onFsChange)
  }, [])

  if (phase === 'idle' && !error) return null

  const showIncoming = phase === 'incoming'
  const showActive = phase === 'outgoing' || phase === 'connected'
  const showVideoTiles = callMode !== 'voice'
  const live = phase === 'connected'

  const statusLine =
    phase === 'outgoing'
      ? t('chat.calls.statusRinging')
      : phase === 'connected'
        ? callMode === 'screen'
          ? t('chat.calls.statusSharing')
          : t('chat.calls.statusLive')
        : ''

  const normalizedRemoteLabel = (remoteLabel ?? '').trim()
  const looksUnknown = /^unknow/i.test(normalizedRemoteLabel)
  const remoteDisplayName = looksUnknown || normalizedRemoteLabel.length === 0
    ? t('chat.title')
    : normalizedRemoteLabel

  const toggleFullscreen = async () => {
    try {
      if (document.fullscreenElement) {
        await document.exitFullscreen()
        return
      }
      await dialogRef.current?.requestFullscreen()
    } catch {
      /* ignore */
    }
  }

  return (
    <div
      className="pointer-events-auto fixed inset-0 z-[100] flex flex-col justify-end sm:justify-center sm:p-4"
      role="dialog"
      aria-modal="true"
      aria-label={t('chat.calls.dialogLabel')}
    >
      <div className="absolute inset-0 bg-black/60 backdrop-blur-[2px]" aria-hidden />

      <div ref={dialogRef} className="relative z-10 mx-auto flex w-full max-w-md flex-col overflow-hidden rounded-t-3xl border border-white/10 bg-gradient-to-b from-slate-900 to-slate-950 shadow-2xl sm:rounded-3xl">
        {error && (
          <div className="flex items-start gap-2 border-b border-white/10 bg-red-950/90 px-4 py-2.5">
            <p className="min-w-0 flex-1 text-center text-xs text-red-50">{error}</p>
            <button
              type="button"
              onClick={onDismissError}
              className="shrink-0 text-red-300 hover:text-red-100"
              aria-label="Đóng lỗi"
            >
              ✕
            </button>
          </div>
        )}

        <div className="flex items-center justify-between gap-3 border-b border-white/10 px-4 py-3">
          <div className="min-w-0">
            <p className="truncate text-base font-semibold text-white">{remoteDisplayName}</p>
            {(showActive || showIncoming) && (
              <p className="truncate text-xs text-slate-400">
                {showIncoming
                  ? t('chat.calls.incomingSubtitle')
                  : showActive
                    ? statusLine
                    : ''}
              </p>
            )}
          </div>
          {live && (
            <span className="shrink-0 rounded-full bg-emerald-500/20 px-2.5 py-0.5 text-[11px] font-medium text-emerald-300">
              {t('chat.calls.badgeLive')}
            </span>
          )}
          {showActive && showVideoTiles && (
            <button
              type="button"
              onClick={() => void toggleFullscreen()}
              className="shrink-0 rounded-full bg-white/10 p-2 text-white hover:bg-white/20"
              title={isFullscreen ? 'Thoát toàn màn hình' : 'Toàn màn hình'}
              aria-label={isFullscreen ? 'Thoát toàn màn hình' : 'Toàn màn hình'}
            >
              {isFullscreen ? <Minimize2 className="h-4 w-4" /> : <Maximize2 className="h-4 w-4" />}
            </button>
          )}
        </div>

        {showIncoming && (
          <div className="border-b border-white/10 px-4 py-4">
            <p className="mb-3 text-center text-sm text-slate-200">
              {t('chat.calls.incomingTitle', { name: incomingFromName ?? remoteDisplayName })}
            </p>
            <div className="flex gap-3">
              <button
                type="button"
                className="flex-1 rounded-2xl bg-emerald-600 py-3 text-sm font-semibold text-white hover:bg-emerald-500"
                onClick={onAccept}
              >
                {t('chat.calls.accept')}
              </button>
              <button
                type="button"
                className="flex-1 rounded-2xl bg-white/10 py-3 text-sm font-semibold text-white hover:bg-white/20"
                onClick={onReject}
              >
                {t('chat.calls.decline')}
              </button>
            </div>
          </div>
        )}

        {showActive && (
          <>
            {showVideoTiles ? (
              <div
                className={`relative bg-black ${
                  phase === 'connected' ? 'aspect-video max-h-[min(50vh,280px)]' : 'aspect-video max-h-[min(45vh,260px)]'
                }`}
              >
                {/* main stage */}
                <video
                  ref={mainStageRef}
                  playsInline
                  autoPlay
                  muted={mainShowsLocal}
                  className="h-full w-full object-contain"
                />
                {/* placeholder when main stream is absent */}
                {!mainStream && (
                  <div className="absolute inset-0 flex flex-col items-center justify-center gap-1 bg-slate-950/90 text-center">
                    <span className="text-sm text-slate-300">
                      {phase === 'outgoing' ? t('chat.calls.connecting') : remoteDisplayName}
                    </span>
                  </div>
                )}
                {/* label when showing local (outgoing preview or screen share) */}
                {mainShowsLocal && (
                  <div className="pointer-events-none absolute left-3 top-3 rounded-full bg-black/60 px-2 py-0.5 text-[10px] text-white">
                    {callMode === 'screen' && phase === 'connected'
                      ? t('chat.calls.youAreSharing')
                      : t('chat.calls.previewYou')}
                  </div>
                )}
                {/* PIP */}
                {showPip && (
                  <video
                    ref={localPipRef}
                    playsInline
                    autoPlay
                    muted={!mainShowsLocal}
                    className="absolute bottom-3 right-3 h-24 w-32 overflow-hidden rounded-xl border-2 border-white/30 bg-black object-cover shadow-lg"
                  />
                )}
              </div>
            ) : (
              <div className="flex flex-col items-center justify-center gap-2 border-b border-white/10 bg-slate-900/50 px-6 py-10">
                <div className="flex h-20 w-20 items-center justify-center rounded-full bg-gradient-to-br from-indigo-500 to-violet-600 text-2xl font-bold text-white shadow-lg">
                  {remoteDisplayName.slice(0, 1).toUpperCase()}
                </div>
                <p className="text-center text-sm text-slate-200">{remoteDisplayName}</p>
                <p className="text-center text-xs text-slate-500">
                  {phase === 'outgoing' ? t('chat.calls.connecting') : t('chat.calls.voiceInProgress')}
                </p>
                {phase === 'outgoing' && (
                  <span className="mt-1 h-1 w-20 overflow-hidden rounded-full bg-slate-800">
                    <span className="block h-full w-1/2 animate-pulse rounded-full bg-indigo-500" />
                  </span>
                )}
              </div>
            )}

            {live && (
              <div className="flex items-center justify-center gap-3 px-4 py-5 sm:gap-5">
                <button
                  type="button"
                  onClick={onToggleMute}
                  className={`flex h-14 w-14 shrink-0 items-center justify-center rounded-full shadow-lg transition ${
                    muted ? 'bg-white text-slate-900' : 'bg-white/15 text-white hover:bg-white/25'
                  }`}
                  title={muted ? t('chat.calls.unmute') : t('chat.calls.mute')}
                  aria-label={muted ? t('chat.calls.unmute') : t('chat.calls.mute')}
                >
                  {muted ? <MicOff className="h-6 w-6" /> : <Mic className="h-6 w-6" />}
                </button>

                {callMode === 'screen' ? (
                  <button
                    type="button"
                    onClick={() => void onStopScreenShare()}
                    className="flex h-14 min-w-[7.5rem] shrink-0 items-center justify-center gap-2 rounded-full bg-amber-600 px-4 text-sm font-semibold text-white shadow-lg hover:bg-amber-500"
                  >
                    <MonitorX className="h-5 w-5" />
                    {t('chat.calls.stopSharing')}
                  </button>
                ) : (
                  <button
                    type="button"
                    onClick={() => void onShareScreen()}
                    className="flex h-14 min-w-[7.5rem] shrink-0 items-center justify-center gap-2 rounded-full bg-indigo-600 px-4 text-sm font-semibold text-white shadow-lg hover:bg-indigo-500"
                  >
                    <MonitorUp className="h-5 w-5" />
                    {t('chat.calls.shareScreen')}
                  </button>
                )}

                <button
                  type="button"
                  onClick={() => void onEnd()}
                  className="flex h-14 w-14 shrink-0 items-center justify-center rounded-full bg-red-600 text-white shadow-lg hover:bg-red-500"
                  title={t('chat.calls.hangUp')}
                  aria-label={t('chat.calls.hangUp')}
                >
                  <PhoneOff className="h-6 w-6" />
                </button>
              </div>
            )}

            {!live && showActive && (
              <div className="flex justify-center px-4 py-4">
                <button
                  type="button"
                  onClick={() => void onEnd()}
                  className="flex h-14 w-14 items-center justify-center rounded-full bg-red-600 text-white shadow-lg hover:bg-red-500"
                  title={t('chat.calls.cancelCall')}
                  aria-label={t('chat.calls.cancelCall')}
                >
                  <PhoneOff className="h-6 w-6" />
                </button>
              </div>
            )}

            <p className="border-t border-white/10 px-4 py-2 text-center text-[10px] leading-snug text-slate-500">
              {callMode === 'voice' && <VideoOff className="mr-0.5 inline h-3 w-3 align-middle" />}
              {callMode === 'video' && <Video className="mr-0.5 inline h-3 w-3 align-middle" />}
              {callMode === 'screen' && <MonitorUp className="mr-0.5 inline h-3 w-3 align-middle" />}
              {t('chat.calls.peerP2pHint')}
            </p>
          </>
        )}
      </div>
    </div>
  )
}
