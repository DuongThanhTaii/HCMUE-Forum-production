import { useCallback, useEffect, useMemo, useState } from 'react'
import {
  useGetMessagesQuery,
  useLazyGetMessagesQuery,
} from '../api/chat.api'
import type { MessageDto } from '../types/chat.types'

function sortMessagesAsc(items: MessageDto[]): MessageDto[] {
  return [...items].sort(
    (a, b) => new Date(a.sentAt).getTime() - new Date(b.sentAt).getTime(),
  )
}

function mergeUniqueMessages(existing: MessageDto[], incoming: MessageDto[]): MessageDto[] {
  const byId = new Map<string, MessageDto>()
  for (const m of existing) {
    byId.set(m.id, m)
  }
  for (const m of incoming) {
    byId.set(m.id, m)
  }
  return sortMessagesAsc([...byId.values()])
}

export function useConversationMessages(conversationId: string | null) {
  const [olderPage, setOlderPage] = useState(1)
  const [olderMessages, setOlderMessages] = useState<MessageDto[]>([])
  const [isLoadingOlder, setIsLoadingOlder] = useState(false)

  const { data, isLoading, isError, isFetching } = useGetMessagesQuery(
    { conversationId: conversationId ?? '', page: 1, pageSize: 50 },
    { skip: !conversationId },
  )

  const [fetchOlderPage] = useLazyGetMessagesQuery()

  useEffect(() => {
    setOlderPage(1)
    setOlderMessages([])
    setIsLoadingOlder(false)
  }, [conversationId])

  const messages = useMemo(() => {
    const latest = data?.items ?? []
    return mergeUniqueMessages(olderMessages, latest)
  }, [data?.items, olderMessages])

  const hasMoreOlder = Boolean(
    data && olderPage < data.totalPages,
  )

  const loadOlder = useCallback(async () => {
    if (!conversationId || !data || isLoadingOlder || !hasMoreOlder) {
      return
    }
    const nextPage = olderPage + 1
    setIsLoadingOlder(true)
    try {
      const result = await fetchOlderPage({
        conversationId,
        page: nextPage,
        pageSize: 50,
      }).unwrap()
      setOlderMessages((prev) => mergeUniqueMessages(prev, result.items))
      setOlderPage(nextPage)
    } finally {
      setIsLoadingOlder(false)
    }
  }, [conversationId, data, fetchOlderPage, hasMoreOlder, isLoadingOlder, olderPage])

  const ensureMessageLoaded = useCallback(
    async (messageId: string): Promise<boolean> => {
      const latest = data?.items ?? []
      let accumulated = olderMessages
      let page = olderPage

      const hasId = (list: MessageDto[]) => list.some((m) => m.id === messageId)
      if (hasId(mergeUniqueMessages(accumulated, latest))) {
        return true
      }

      if (!conversationId || !data) {
        return false
      }

      while (page < data.totalPages) {
        page += 1
        const result = await fetchOlderPage({
          conversationId,
          page,
          pageSize: 50,
        }).unwrap()
        accumulated = mergeUniqueMessages(accumulated, result.items)
        if (hasId(mergeUniqueMessages(accumulated, latest))) {
          setOlderMessages(accumulated)
          setOlderPage(page)
          return true
        }
      }

      return hasId(mergeUniqueMessages(accumulated, latest))
    },
    [conversationId, data, fetchOlderPage, olderMessages, olderPage],
  )

  return {
    messages,
    isLoading,
    isError,
    isFetching,
    isLoadingOlder,
    hasMoreOlder,
    loadOlder,
    ensureMessageLoaded,
    totalPages: data?.totalPages ?? 1,
  }
}
