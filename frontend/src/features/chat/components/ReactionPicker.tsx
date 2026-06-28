import { CHAT_QUICK_REACTIONS } from '../lib/chatReactionEmojis'

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
      className={`flex flex-wrap gap-0.5 rounded-lg border border-slate-200 bg-white p-1 shadow-lg ${className}`}
    >
      {CHAT_QUICK_REACTIONS.map((emoji) => (
        <button
          key={emoji}
          type="button"
          className="cursor-pointer rounded-md px-1.5 py-0.5 text-lg leading-none hover:bg-slate-100"
          onClick={() => onPick(emoji)}
        >
          {emoji}
        </button>
      ))}
    </div>
  )
}
