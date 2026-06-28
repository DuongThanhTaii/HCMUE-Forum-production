import { useTranslation } from 'react-i18next'

export type ForumFilterTab = 'category' | 'tag' | 'latest' | 'hot'

type ForumFiltersRowProps = {
  activeTab: ForumFilterTab
  onTabChange: (value: ForumFilterTab) => void
}

export function ForumFiltersRow({
  activeTab,
  onTabChange,
}: ForumFiltersRowProps) {
  const { t } = useTranslation()
  const tabBaseClass =
    'cursor-pointer rounded-md border px-3 py-1.5 text-[13px] font-medium transition-colors'
  const activeTabClass = 'border-primary bg-primary text-white'
  const inactiveTabClass = 'border-slate-200 bg-white text-slate-600 hover:border-primary hover:text-primary'

  return (
    <section className="forum-compact-card flex flex-wrap items-center gap-2 px-3 py-2.5">
      <button
        type="button"
        className={`${tabBaseClass} ${activeTab === 'category' ? activeTabClass : inactiveTabClass}`}
        onClick={() => onTabChange('category')}
      >
        {t('forum.categories')}
      </button>
      <button
        type="button"
        className={`${tabBaseClass} ${activeTab === 'tag' ? activeTabClass : inactiveTabClass}`}
        onClick={() => onTabChange('tag')}
      >
        {t('forum.tags')}
      </button>
      <button
        type="button"
        className={`${tabBaseClass} ${activeTab === 'latest' ? activeTabClass : inactiveTabClass}`}
        onClick={() => onTabChange('latest')}
      >
        {t('forum.recent')}
      </button>
      <button
        type="button"
        className={`${tabBaseClass} ${activeTab === 'hot' ? activeTabClass : inactiveTabClass}`}
        onClick={() => onTabChange('hot')}
      >
        {t('forum.trending')}
      </button>
    </section>
  )
}
