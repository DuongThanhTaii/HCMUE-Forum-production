import { useTranslation } from 'react-i18next'
import { useGetFacultiesQuery } from '../api/learning.api'

export function useLearningFacultiesPage() {
  const { t } = useTranslation()
  const { data = [], isLoading, isError } = useGetFacultiesQuery()

  return {
    t,
    faculties: data,
    isLoading,
    isError,
    isEmpty: !isLoading && !isError && data.length === 0,
  }
}
