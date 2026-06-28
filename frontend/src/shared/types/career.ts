export type CareerJob = {
  id: string
  title: string
  description?: string | null
  status?: string | null
  companyName?: string | null
  companyLogoUrl?: string | null
  city?: string | null
  isRemote?: boolean
  createdAt?: string
  salaryMin?: number | null
  salaryMax?: number | null
  currency?: string | null
}

export type CareerSearchParams = {
  page?: number
  pageSize?: number
  searchTerm?: string
  city?: string
  companyId?: string
}

