import { Link, Outlet } from 'react-router-dom';
import { useTranslation } from 'react-i18next';

// Layout Tree:
// ModLayout [min-h-screen bg-slate-50]
// ├── Header [border-b border-slate-200 bg-white]
// └── Body [p-6]
export function ModLayout() {
  const { t } = useTranslation();

  const navLinkClass =
    'rounded-md px-2 py-1 text-[13px] font-medium text-slate-600 hover:bg-slate-100 hover:text-slate-900';

  return (
    <div className="min-h-screen bg-slate-50">
      <header className="flex min-h-14 flex-wrap items-center justify-between gap-3 border-b border-slate-200 bg-white px-4 py-2 sm:px-6 sm:py-0">
        <h1 className="text-base font-semibold tracking-tight text-slate-900">{t('mod.layout.title')}</h1>
        <nav className="flex flex-wrap items-center gap-1 sm:gap-2">
          <Link to="/home" className={navLinkClass}>
            {t('mod.layout.home')}
          </Link>
          <Link to="/forum" className={navLinkClass}>
            {t('mod.layout.forum')}
          </Link>
          <Link to="/mod/reports" className={navLinkClass}>
            {t('mod.layout.reports')}
          </Link>
          <Link to="/mod/dashboard" className={navLinkClass}>
            {t('mod.layout.dashboard')}
          </Link>
          <Link to="/mod/posts" className={navLinkClass}>
            {t('mod.layout.pendingPosts')}
          </Link>
          <Link to="/mod/thread-channels" className={navLinkClass}>
            {t('mod.layout.threadChannels')}
          </Link>
          <Link to="/mod/learning" className={navLinkClass}>
            {t('mod.layout.learningApproval')}
          </Link>
        </nav>
      </header>
      <main className="p-6">
        <Outlet />
      </main>
    </div>
  );
}

