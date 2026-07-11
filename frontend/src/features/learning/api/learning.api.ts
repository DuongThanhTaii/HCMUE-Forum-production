import { baseApi } from '@shared/lib/api/baseApi'
import type {
  CourseListItem,
  DocumentDetail,
  DocumentsListResult,
  FacultyListItem,
  LearningDocument,
  LearningSearchParams,
  PagedCoursesResult,
} from '@shared/types/learning'

type ApiEnvelope<T> = {
  data?: T
  Data?: T
}

type SearchPayload = {
  documents?: LearningDocument[]
  Documents?: LearningDocument[]
  items?: LearningDocument[]
  totalCount?: number
  TotalCount?: number
  pageNumber?: number
  PageNumber?: number
  pageSize?: number
  PageSize?: number
  totalPages?: number
  TotalPages?: number
}

function extractDocuments(raw: unknown): LearningDocument[] {
  if (!raw || typeof raw !== 'object') return []
  const p = raw as SearchPayload
  const list = p.documents ?? p.Documents ?? p.items
  return Array.isArray(list) ? list : []
}

function unwrapData<T>(response: unknown): T | undefined {
  if (!response || typeof response !== 'object') return undefined
  const r = response as ApiEnvelope<T>
  return (r.data ?? r.Data) as T | undefined
}

function mapCourseListItem(raw: Record<string, unknown>): CourseListItem {
  return {
    courseId: String(raw.courseId ?? raw.CourseId ?? ''),
    code: String(raw.code ?? raw.Code ?? ''),
    name: String(raw.name ?? raw.Name ?? ''),
    description: String(raw.description ?? raw.Description ?? ''),
    semester: String(raw.semester ?? raw.Semester ?? ''),
    credits: Number(raw.credits ?? raw.Credits ?? 0),
    facultyId:
      raw.facultyId != null || raw.FacultyId != null
        ? String(raw.facultyId ?? raw.FacultyId)
        : null,
    createdAt: String(raw.createdAt ?? raw.CreatedAt ?? ''),
    documentCount: Number(raw.documentCount ?? raw.DocumentCount ?? 0),
  }
}

function normalizePagedCourses(response: unknown): PagedCoursesResult {
  const inner = unwrapData<unknown>(response)
  if (!inner) {
    return { items: [], page: 1, pageSize: 20, totalCount: 0, totalPages: 0 }
  }
  if (Array.isArray(inner)) {
    const items = inner.map((x) => mapCourseListItem(x as Record<string, unknown>))
    return {
      items,
      page: 1,
      pageSize: items.length || 20,
      totalCount: items.length,
      totalPages: items.length > 0 ? 1 : 0,
    }
  }
  const o = inner as Record<string, unknown>
  const itemsRaw = o.items ?? o.Items
  const items = Array.isArray(itemsRaw)
    ? itemsRaw.map((x) => mapCourseListItem(x as Record<string, unknown>))
    : []
  const pageSize = Number(o.pageSize ?? o.PageSize ?? 20)
  const totalCount = Number(o.totalCount ?? o.TotalCount ?? items.length)
  let totalPages = Number(o.totalPages ?? o.TotalPages ?? 0)
  if (totalPages <= 0 && pageSize > 0 && totalCount > 0) {
    totalPages = Math.max(1, Math.ceil(totalCount / pageSize))
  }
  return {
    items,
    page: Number(o.page ?? o.Page ?? 1),
    pageSize,
    totalCount,
    totalPages: totalCount === 0 ? 0 : totalPages,
  }
}

function normalizeSearchResult(inner: unknown, pageFallback: number, pageSizeFallback: number): DocumentsListResult {
  if (Array.isArray(inner)) {
    return {
      documents: inner,
      totalCount: inner.length,
      pageNumber: pageFallback,
      pageSize: pageSizeFallback,
      totalPages: inner.length > 0 ? 1 : 0,
    }
  }
  if (!inner || typeof inner !== 'object') {
    return {
      documents: [],
      totalCount: 0,
      pageNumber: pageFallback,
      pageSize: pageSizeFallback,
      totalPages: 0,
    }
  }
  const p = inner as SearchPayload
  const documents = extractDocuments(inner)
  const totalCount = Number(p.totalCount ?? p.TotalCount ?? documents.length)
  const pageNumber = Number(p.pageNumber ?? p.PageNumber ?? pageFallback)
  const pageSize = Number(p.pageSize ?? p.PageSize ?? pageSizeFallback)
  let totalPages = Number(p.totalPages ?? p.TotalPages ?? 0)
  if (totalPages <= 0 && pageSize > 0) {
    totalPages = Math.max(1, Math.ceil(totalCount / pageSize))
  }
  return {
    documents,
    totalCount,
    pageNumber,
    pageSize,
    totalPages: totalCount === 0 ? 0 : totalPages,
  }
}

export const learningApi = baseApi.injectEndpoints({
  endpoints: (builder) => ({
    getDocuments: builder.query<DocumentsListResult, LearningSearchParams | void>({
      query: (params) => {
        const p = (params ?? {}) as LearningSearchParams
        const pageNumber = p.pageNumber ?? 1
        const pageSize = p.pageSize ?? 20
        return {
          url: '/api/v1/documents',
          params: {
            pageNumber,
            pageSize,
            ...(p.searchTerm ? { searchTerm: p.searchTerm } : {}),
            ...(p.facultyId ? { facultyId: p.facultyId } : {}),
            ...(p.courseId ? { courseId: p.courseId } : {}),
            ...(typeof p.status === 'number' ? { status: p.status } : {}),
          },
        }
      },
      transformResponse: (response: unknown, _meta, arg) => {
        const p = (arg ?? {}) as LearningSearchParams
        const pageFallback = p.pageNumber ?? 1
        const pageSizeFallback = p.pageSize ?? 20
        const inner = unwrapData<SearchPayload | LearningDocument[]>(response)
        return normalizeSearchResult(inner, pageFallback, pageSizeFallback)
      },
      providesTags: (result) =>
        result?.documents?.length
          ? [
              ...result.documents.map((doc) => ({ type: 'Document' as const, id: doc.id })),
              { type: 'Document' as const, id: 'LIST' },
            ]
          : [{ type: 'Document' as const, id: 'LIST' }],
    }),

    getDocumentById: builder.query<DocumentDetail, string>({
      query: (id) => `/api/v1/documents/${id}`,
      transformResponse: (response: unknown) => {
        const raw = unwrapData<Record<string, unknown>>(response)
        if (!raw || typeof raw !== 'object') throw new Error('MISSING_DOCUMENT')
        const d = raw as DocumentDetail & Record<string, unknown>
        const pickStr = (a: string, b: string) => {
          const v = d[a] ?? d[b]
          return typeof v === 'string' && v.trim().length > 0 ? v : null
        }
        return {
          ...d,
          uploaderDisplayName: pickStr('uploaderDisplayName', 'UploaderDisplayName'),
          courseName: pickStr('courseName', 'CourseName'),
          reviewerDisplayName: pickStr('reviewerDisplayName', 'ReviewerDisplayName'),
        }
      },
      providesTags: (_result, _err, id) => [{ type: 'Document', id }],
    }),

    getFaculties: builder.query<FacultyListItem[], void>({
      query: () => '/api/v1/faculties',
      transformResponse: (response: unknown) => unwrapData<FacultyListItem[]>(response) ?? [],
    }),

    getCourses: builder.query<
      PagedCoursesResult,
      {
        facultyId?: string
        semester?: string
        searchTerm?: string
        page?: number
        pageSize?: number
      } | void
    >({
      query: (args) => {
        const a = (args ?? {}) as {
          facultyId?: string
          semester?: string
          searchTerm?: string
          page?: number
          pageSize?: number
        }
        const page = a.page ?? 1
        const pageSize = a.pageSize ?? 20
        return {
          url: '/api/v1/courses',
          params: {
            page,
            pageSize,
            ...(a.facultyId ? { facultyId: a.facultyId } : {}),
            ...(a.semester ? { semester: a.semester } : {}),
            ...(a.searchTerm ? { searchTerm: a.searchTerm } : {}),
          },
        }
      },
      transformResponse: (response: unknown) => normalizePagedCourses(response),
    }),

    getCourseSemesters: builder.query<string[], { facultyId?: string } | void>({
      query: (args) => {
        const facultyId = args && 'facultyId' in args ? args.facultyId : undefined
        return {
          url: '/api/v1/courses/semesters',
          params: facultyId ? { facultyId } : {},
        }
      },
      transformResponse: (response: unknown) => unwrapData<string[]>(response) ?? [],
    }),

    rateDocument: builder.mutation<
      unknown,
      { documentId: string; userId: string; rating: number }
    >({
      query: ({ documentId, userId, rating }) => ({
        url: `/api/v1/documents/${documentId}/rate`,
        method: 'POST',
        body: { userId, rating },
      }),
      invalidatesTags: (_r, _e, { documentId }) => [
        { type: 'Document', id: documentId },
        { type: 'Document', id: 'LIST' },
      ],
    }),

    downloadDocument: builder.mutation<unknown, { documentId: string; userId: string }>({
      query: ({ documentId, userId }) => ({
        url: `/api/v1/documents/${documentId}/download`,
        method: 'POST',
        body: { userId },
      }),
      invalidatesTags: (_r, _e, { documentId }) => [
        { type: 'Document', id: documentId },
        { type: 'Document', id: 'LIST' },
      ],
    }),
    uploadDocument: builder.mutation<
      unknown,
      { title: string; description?: string; file?: File | null; driveUrl?: string; documentType: number; courseId?: string }
    >({
      query: ({ title, description, file, driveUrl, documentType, courseId }) => {
        const formData = new FormData()
        formData.append('title', title)
        if (description?.trim()) formData.append('description', description.trim())
        if (file) formData.append('file', file)
        if (driveUrl?.trim()) formData.append('driveUrl', driveUrl.trim())
        formData.append('documentType', String(documentType))
        if (courseId?.trim()) formData.append('courseId', courseId.trim())
        return {
          url: '/api/v1/documents/upload',
          method: 'POST',
          body: formData,
        }
      },
      invalidatesTags: [{ type: 'Document', id: 'LIST' }],
    }),
    approveDocument: builder.mutation<unknown, { documentId: string; comment?: string }>({
      query: ({ documentId, comment }) => ({
        url: `/api/v1/documents/${documentId}/approve`,
        method: 'POST',
        body: { comment },
      }),
      invalidatesTags: (_r, _e, { documentId }) => [
        { type: 'Document', id: documentId },
        { type: 'Document', id: 'LIST' },
      ],
    }),
    rejectDocument: builder.mutation<unknown, { documentId: string; reason: string }>({
      query: ({ documentId, reason }) => ({
        url: `/api/v1/documents/${documentId}/reject`,
        method: 'POST',
        body: { reason },
      }),
      invalidatesTags: (_r, _e, { documentId }) => [
        { type: 'Document', id: documentId },
        { type: 'Document', id: 'LIST' },
      ],
    }),
    requestRevisionDocument: builder.mutation<unknown, { documentId: string; reason: string }>({
      query: ({ documentId, reason }) => ({
        url: `/api/v1/documents/${documentId}/request-revision`,
        method: 'POST',
        body: { reason },
      }),
      invalidatesTags: (_r, _e, { documentId }) => [
        { type: 'Document', id: documentId },
        { type: 'Document', id: 'LIST' },
      ],
    }),
    bulkApproveDocuments: builder.mutation<unknown, { documentIds: string[] }>({
      query: ({ documentIds }) => ({
        url: `/api/v1/documents/bulk/approve`,
        method: 'POST',
        body: { documentIds },
      }),
      invalidatesTags: [{ type: 'Document', id: 'LIST' }],
    }),
    bulkRejectDocuments: builder.mutation<unknown, { documentIds: string[]; reason: string }>({
      query: ({ documentIds, reason }) => ({
        url: `/api/v1/documents/bulk/reject`,
        method: 'POST',
        body: { documentIds, reason },
      }),
      invalidatesTags: [{ type: 'Document', id: 'LIST' }],
    }),
  }),
})

export const {
  useGetDocumentsQuery,
  useGetDocumentByIdQuery,
  useGetFacultiesQuery,
  useGetCoursesQuery,
  useGetCourseSemestersQuery,
  useRateDocumentMutation,
  useDownloadDocumentMutation,
  useUploadDocumentMutation,
  useApproveDocumentMutation,
  useRejectDocumentMutation,
  useRequestRevisionDocumentMutation,
  useBulkApproveDocumentsMutation,
  useBulkRejectDocumentsMutation,
} = learningApi
