import { useCallback } from 'react'
import { useTranslation } from 'react-i18next'
import {
  useGetForumCategoriesQuery,
} from '../../../forum/api/forum.list.api'
import {
  useCreateCategoryMutation,
  useUpdateCategoryMutation,
  useDeleteCategoryMutation,
} from '../../api/admin.categories.api'
import type { ForumCategoryOption } from '../../../forum/api/forum.list.api'

export function useAdminCategoriesPage() {
  const { t } = useTranslation()
  const { data: categories = [], isLoading, isError } = useGetForumCategoriesQuery()
  const [createCategory, { isLoading: isCreating }] = useCreateCategoryMutation()
  const [updateCategory, { isLoading: isUpdating }] = useUpdateCategoryMutation()
  const [deleteCategory, { isLoading: isDeleting }] = useDeleteCategoryMutation()

  const submitCreate = useCallback(
    async (body: Omit<ForumCategoryOption, 'id' | 'postCount'> & { isActive: boolean }) => {
      return createCategory(body).unwrap()
    },
    [createCategory],
  )

  const submitUpdate = useCallback(
    async (id: string, body: Omit<ForumCategoryOption, 'id' | 'postCount'> & { isActive: boolean }) => {
      return updateCategory({ id, body }).unwrap()
    },
    [updateCategory],
  )

  const submitDelete = useCallback(
    async (id: string) => {
      return deleteCategory(id).unwrap()
    },
    [deleteCategory],
  )

  return {
    t,
    categories,
    isLoading,
    isError,
    submitCreate,
    submitUpdate,
    submitDelete,
    isCreating,
    isUpdating,
    isDeleting,
  }
}
