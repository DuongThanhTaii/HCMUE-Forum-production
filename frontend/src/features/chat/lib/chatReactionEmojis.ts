/** Subset aligned with backend AddReactionCommandValidator. */
export const CHAT_QUICK_REACTIONS = ['👍', '❤️', '😂', '😮', '😢', '🎉', '🔥', '👏'] as const

export type ChatQuickReaction = (typeof CHAT_QUICK_REACTIONS)[number]
