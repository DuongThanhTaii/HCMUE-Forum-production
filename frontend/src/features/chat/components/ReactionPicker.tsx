const EMOJIS = ['👍', '❤️', '😂', '😲', '😢', '😡']

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
      className={`absolute bottom-full mb-1 z-50 shadow-lg bg-white rounded-full border border-slate-100 flex items-center gap-1 p-1.5 ${className}`}
      onClick={(e) => e.stopPropagation()}
    >
      {EMOJIS.map((emoji) => (
        <button
          key={emoji}
          type="button"
          onClick={() => onPick(emoji)}
          className="hover:scale-125 transition-transform origin-bottom text-xl leading-none"
        >
          {emoji}
        </button>
      ))}
    </div>
  )
}
