import { baseApi } from '@shared/lib/api/baseApi'
import { unwrapApiData } from './admin.api'
import type { ForumCategoryOption } from '../../forum/api/forum.list.api'

type UpsertCategoryRequest = {
  name: string
  description: string
  slug: string
  displayOrder: number
  isActive: boolean
  parentCategoryId?: string | null
  moderatorIds?: string[]
}

export const adminCategoriesApi = baseApi.injectEndpoints({
  endpoints: (builder) => ({
    createCategory: builder.mutation<ForumCategoryOption, UpsertCategoryRequest>({
      query: (body) => ({
        url: '/api/v1/categories',
        method: 'POST',
        body,
      }),
      transformResponse: (response: unknown) => {
        const payload = unwrapApiData<ForumCategoryOption>(response)
        if (!payload) throw new Error('MISSING_CATEGORY')
        return payload
      },
      invalidatesTags: [{ type: 'ForumCategory', id: 'LIST' }],
    }),

    updateCategory: builder.mutation<ForumCategoryOption, { id: string; body: UpsertCategoryRequest }>({
      query: ({ id, body }) => ({
        url: `/api/v1/categories/${id}`,
        method: 'PUT',
        body,
      }),
      transformResponse: (response: unknown) => {
        const payload = unwrapApiData<ForumCategoryOption>(response)
        if (!payload) throw new Error('MISSING_CATEGORY')
        return payload
      },
      invalidatesTags: [{ type: 'ForumCategory', id: 'LIST' }],
    }),

    deleteCategory: builder.mutation<void, string>({
      query: (id) => ({
        url: `/api/v1/categories/${id}`,
        method: 'DELETE',
      }),
      invalidatesTags: [{ type: 'ForumCategory', id: 'LIST' }],
    }),
  }),
})

export const {
  useCreateCategoryMutation,
  useUpdateCategoryMutation,
  useDeleteCategoryMutation,
} = adminCategoriesApi
