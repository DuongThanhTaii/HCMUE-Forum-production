export const EMOJI_ASSETS: Record<string, string> = {
  '❤️': 'https://raw.githubusercontent.com/Tarikul-Islam-Anik/Animated-Fluent-Emojis/master/Emojis/Smilies/Red%20Heart.png',
  '😆': 'https://raw.githubusercontent.com/Tarikul-Islam-Anik/Animated-Fluent-Emojis/master/Emojis/Smilies/Grinning%20Squinting%20Face.png',
  '😲': 'https://raw.githubusercontent.com/Tarikul-Islam-Anik/Animated-Fluent-Emojis/master/Emojis/Smilies/Astonished%20Face.png',
  '😢': 'https://raw.githubusercontent.com/Tarikul-Islam-Anik/Animated-Fluent-Emojis/master/Emojis/Smilies/Crying%20Face.png',
  '😡': 'https://raw.githubusercontent.com/Tarikul-Islam-Anik/Animated-Fluent-Emojis/master/Emojis/Smilies/Pouting%20Face.png',
  '👍': 'https://raw.githubusercontent.com/Tarikul-Islam-Anik/Animated-Fluent-Emojis/master/Emojis/Hand%20gestures/Thumbs%20Up.png',
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
      className={`absolute bottom-full mb-1 z-50 shadow-lg bg-white rounded-full border border-slate-100 flex items-center gap-1.5 p-1.5 ${className}`}
      onClick={(e) => e.stopPropagation()}
    >
      {EMOJIS.map((emoji) => (
        <button
          key={emoji}
          type="button"
          onClick={() => onPick(emoji)}
          className="hover:scale-125 transition-transform origin-bottom text-xl leading-none flex items-center justify-center w-8 h-8 rounded-full hover:bg-slate-50"
        >
          <img src={EMOJI_ASSETS[emoji]} alt={emoji} className="w-7 h-7 object-contain" />
        </button>
      ))}
    </div>
  )
}
