export type LearningDocument = {
  id: string
  title: string
  description?: string | null
  documentType?: string
  status?: string
  fileName?: string
  uploaderName?: string | null
  uploaderId?: string
  averageRating?: number
  /** Search API uses `downloadCount` */
  downloadCount?: number
  totalDownloads?: number
  ratingCount?: number
  viewCount?: number
  courseId?: string | null
  createdAt?: string
  updatedAt?: string
}

export type LearningSearchParams = {
  pageNumber?: number
  pageSize?: number
  searchTerm?: string
  facultyId?: string
  courseId?: string
  status?: number
}

/** Normalized from GET /api/v1/documents search envelope */
export type DocumentsListResult = {
  documents: LearningDocument[]
  totalCount: number
  pageNumber: number
  pageSize: number
  totalPages: number
}

export type FacultyListItem = {
  facultyId: string
  code: string
  name: string
  description: string
  status: string
  courseCount: number
  createdAt: string
}

export type CourseListItem = {
  courseId: string
  code: string
  name: string
  description: string
  semester: string
  credits: number
  facultyId: string | null
  createdAt: string
  documentCount: number
}

/** GET /api/v1/courses paged envelope */
export type PagedCoursesResult = {
  items: CourseListItem[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
}

export type DocumentDetail = {
  id: string
  title: string
  description: string
  documentType: string
  status: string
  fileName: string
  filePath: string
  fileSize: number
  contentType: string
  averageRating: number
  ratingCount: number
  viewCount: number
  downloadCount: number
  uploaderId: string
  uploaderDisplayName?: string | null
  courseId: string | null
  courseName?: string | null
  reviewerId: string | null
  reviewerDisplayName?: string | null
  reviewComment: string | null
  rejectionReason: string | null
  createdAt: string
  updatedAt: string | null
  submittedAt: string | null
  reviewedAt: string | null
}
