/** Scroll so `target` is visible inside `container` without moving the document/window. */
export function scrollElementWithinContainer(
  container: HTMLElement,
  target: HTMLElement,
  block: ScrollLogicalPosition = 'center',
): void {
  const containerRect = container.getBoundingClientRect()
  const targetRect = target.getBoundingClientRect()
  const targetTop = targetRect.top - containerRect.top + container.scrollTop
  let nextTop = targetTop

  if (block === 'center') {
    nextTop = targetTop - container.clientHeight / 2 + targetRect.height / 2
  } else if (block === 'end') {
    nextTop = targetTop - container.clientHeight + targetRect.height
  }

  const maxTop = Math.max(0, container.scrollHeight - container.clientHeight)
  container.scrollTop = Math.min(maxTop, Math.max(0, nextTop))
}
