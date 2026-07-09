export const EMOJI_ASSETS: Record<string, string> = {
  '❤️': '/emojis/Red Heart.png',
  '😆': '/emojis/Grinning Squinting Face.png',
  '😲': '/emojis/Astonished Face.png',
  '😢': '/emojis/Crying Face.png',
  '😡': '/emojis/Enraged Face.png',
  '👍': '/emojis/Thumbs Up.png',
}

export const EMOJIS = Object.keys(EMOJI_ASSETS)

export function ReactionPicker({
  onPick,
  className = '',
}: {
  onPick: (emoji: string) => void
  className?: string
}) {
  return (
    <div
      role="toolbar"
      aria-label="Reactions"
      className={`absolute w-max bottom-full mb-1 z-50 shadow-lg bg-surface rounded-full border border-border flex items-center gap-1.5 p-1.5 ${className}`}
      onClick={(e) => e.stopPropagation()}
    >
      {EMOJIS.map((emoji) => (
        <button
          key={emoji}
          type="button"
          onClick={() => onPick(emoji)}
          className="shrink-0 hover:scale-125 transition-transform origin-bottom text-xl leading-none flex items-center justify-center w-8 h-8 rounded-full hover:bg-background"
        >
          <img src={EMOJI_ASSETS[emoji]} alt={emoji} className="w-7 h-7 object-contain" />
        </button>
      ))}
    </div>
  )
}
