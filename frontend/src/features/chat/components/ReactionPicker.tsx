import EmojiPicker from 'emoji-picker-react'

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
      className={`absolute z-50 shadow-xl ${className}`}
      onClick={(e) => e.stopPropagation()}
    >
      <EmojiPicker
        onEmojiClick={(emojiData) => onPick(emojiData.emoji)}
        width={300}
        height={400}
      />
    </div>
  )
}
