import { useTranslation } from 'react-i18next'
import { useGetJobsQuery } from '../api/career.api'
import { parseCareerDescription } from '../lib/jobDescription'

function formatDate(value: string | undefined, locale: string) {
  if (!value) return '-'
  const date = new Date(value)
  return Number.isNaN(date.getTime()) ? '-' : date.toLocaleDateString(locale)
}

function formatSalary(
  min: number | null | undefined,
  max: number | null | undefined,
  currency: string | null | undefined,
  t: (key: string) => string,
) {
  if (min == null && max == null) return t('career.common.negotiable')
  const unit = currency ?? 'VND'
  if (min != null && max != null) return `${min.toLocaleString()} - ${max.toLocaleString()} ${unit}`
  if (min != null) return `${t('career.common.from')} ${min.toLocaleString()} ${unit}`
  return `${t('career.common.upTo')} ${max?.toLocaleString()} ${unit}`
}

function summarizeDescription(value: string | null | undefined) {
  if (!value) return ''
  const parsed = parseCareerDescription(value)
  const candidate = parsed.overview[0] ?? value
  const normalized = candidate.replace(/\s+/g, ' ').trim()
  if (normalized.length <= 160) return normalized
  return `${normalized.slice(0, 157)}...`
}

export function useCareerJobsPage() {
  const { i18n, t } = useTranslation()
  const { data, isLoading, isError } = useGetJobsQuery({ page: 1, pageSize: 20 })
  const locale = i18n.resolvedLanguage === 'vi' ? 'vi-VN' : 'en-US'

  const jobs =
    data?.map((job) => ({
      ...job,
      displayCompany: job.companyName ?? t('career.common.unknownCompany'),
      displayCity: job.city ?? t('career.common.unknownCity'),
      displayWorkMode: job.isRemote ? t('career.common.remote') : t('career.common.onSite'),
      displaySalary: formatSalary(job.salaryMin, job.salaryMax, job.currency, t),
      displayPostedAt: formatDate(job.createdAt, locale),
      displayDescription: summarizeDescription(job.description),
    })) ?? []

  return {
    t,
    jobs,
    isLoading,
    isError,
    isEmpty: !isLoading && !isError && jobs.length === 0,
  }
}
