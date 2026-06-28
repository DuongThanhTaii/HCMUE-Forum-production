/** Khoảng cách tới đáy (px) vẫn coi là "đang ở cuối". */
export const BOTTOM_THRESHOLD_PX = 48

/** Kéo gần đỉnh danh sách thì gọi load tin cũ hơn. */
export const LOAD_MORE_TOP_PX = 80

export const LOAD_OLDER_DEBOUNCE_MS = 300

export type ScrollAnchorState = {
  atBottom: boolean
  pendingNewCount: number
}

export function isAtBottom(
  scrollTop: number,
  scrollHeight: number,
  clientHeight: number,
  threshold = BOTTOM_THRESHOLD_PX,
): boolean {
  return scrollHeight - scrollTop - clientHeight <= threshold
}
