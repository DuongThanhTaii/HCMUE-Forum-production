import { useTranslation } from 'react-i18next'
import {
  useCreateThreadChannelMutation,
  useGetAdminThreadChannelsQuery,
  useUpdateThreadChannelMutation,
} from '../../api/admin.observability.api'
import type { UpsertThreadChannelRequest } from '../../types/admin.types'

export function useAdminThreadChannelsPage() {
  const { t } = useTranslation()
  const { data: channels = [], isLoading, isError, refetch } = useGetAdminThreadChannelsQuery()
  const [createChannel, { isLoading: isCreating }] = useCreateThreadChannelMutation()
  const [updateChannel, { isLoading: isUpdating }] = useUpdateThreadChannelMutation()

  const submitCreate = (body: UpsertThreadChannelRequest) => createChannel(body).unwrap()
  const submitUpdate = (id: string, body: UpsertThreadChannelRequest) =>
    updateChannel({ id, body }).unwrap()

  return {
    t,
    channels,
    isLoading,
    isError,
    refetch,
    submitCreate,
    submitUpdate,
    isCreating,
    isUpdating,
  }
}
