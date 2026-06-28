import { useEffect, useState } from 'react'
import { useSearchConversationMessagesQuery } from '../api/chat.api'
import type { ConversationMessageSearchFilter } from '../types/chat.types'

export function useConversationSearch(
  conversationId: string | null,
  filter: ConversationMessageSearchFilter = 'text',
) {
  const [q, setQ] = useState('')
  const [debouncedQ, setDebouncedQ] = useState('')

  useEffect(() => {
    const handle = window.setTimeout(() => setDebouncedQ(q.trim()), 300)
    return () => window.clearTimeout(handle)
  }, [q])

  const canSearch = debouncedQ.length >= 2

  const { data, isFetching, isError } = useSearchConversationMessagesQuery(
    {
      conversationId: conversationId ?? '',
      q: debouncedQ,
      filter,
      page: 1,
      pageSize: 20,
    },
    { skip: !conversationId || !canSearch },
  )

  return {
    q,
    setQ,
    debouncedQ,
    canSearch,
    results: data?.items ?? [],
    isFetching,
    isError,
    totalCount: data?.totalCount ?? 0,
  }
}
