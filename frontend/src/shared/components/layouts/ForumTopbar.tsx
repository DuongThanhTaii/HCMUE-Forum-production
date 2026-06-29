import { Search } from 'lucide-react';
import { Link, useLocation } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { logout, selectAuth, selectIsAuthenticated } from '@features/auth/model/auth.slice';
import { useAppDispatch } from '@shared/hooks/useAppDispatch';
import { useAppSelector } from '@shared/hooks/useAppSelector';
import { LanguageSwitcher } from '../i18n/LanguageSwitcher';
import { NotificationBell } from '@features/notifications/components/NotificationBell';

const MAIN_NAV = [
  { to: '/home', prefix: '/home' },
  { to: '/forum', prefix: '/forum' },
  { to: '/forum/threads', prefix: '/forum/threads' },
  { to: '/forum/saved', prefix: '/forum/saved' },
  { to: '/learning/documents', prefix: '/learning' },
  { to: '/career/jobs', prefix: '/career' },
  { to: '/assistant', prefix: '/assistant' },
  { to: '/chat', prefix: '/chat' },
] as const;

function navLinkActive(pathname: string, prefix: string) {
  if (prefix === '/home') return pathname === '/home';
  return pathname === prefix || pathname.startsWith(`${prefix}/`);
}

/** Same link padding as `ForumSidebar` items (`px-2 py-1.5`) inside `p-3` aside. */
function BrandLink({ className = '' }: { className?: string }) {
  const { t } = useTranslation();
  return (
    <Link
      to="/home"
      className={`flex min-w-0 items-center gap-2 rounded-md px-2 py-1.5 text-primary no-underline hover:bg-slate-100 ${className}`.trim()}
    >
      <img src="/logohcmue-forum.png" alt="" className="h-7 w-auto shrink-0" />
      <span className="truncate text-xs font-semibold leading-none">{t('forum.topbar.brand')}</span>
    </Link>
  );
}

export function ForumTopbar() {
  const { t } = useTranslation();
  const { pathname } = useLocation();
  const dispatch = useAppDispatch();
  const isAuthenticated = useAppSelector(selectIsAuthenticated);
  const auth = useAppSelector(selectAuth);

  const displayName = auth.user?.fullName || auth.user?.email || 'User';

  const normalizedRoles = (auth.user?.roles ?? []).map((r) => r.toLowerCase());
  const showModerationLink = normalizedRoles.some((r) => r === 'moderator');
  const showAdminLink = normalizedRoles.some((r) => r === 'admin');

  const onLogout = () => {
    dispatch(logout());
  };

  return (
    <header className="fixed left-0 right-0 top-0 z-40 h-14 border-b border-slate-200 bg-white/95 backdrop-blur">
      <div className="flex h-full w-full">
        <div className="hidden h-full w-64 shrink-0 border-r border-slate-200 px-3 lg:flex lg:items-center">
          <BrandLink className="w-full" />
        </div>

        <Link
          to="/home"
          className="flex h-full shrink-0 items-center gap-2 px-4 text-primary no-underline hover:bg-slate-50 lg:hidden"
        >
          <img src="/logohcmue-forum.png" alt="" className="h-7 w-auto shrink-0" />
          <span className="max-w-[9rem] truncate text-xs font-semibold leading-none sm:max-w-none">
            {t('forum.topbar.brand')}
          </span>
        </Link>

        <div className="flex min-h-0 min-w-0 flex-1 items-center justify-end gap-2 px-3 sm:px-4 md:justify-between md:px-6">
          <nav className="hidden min-w-0 flex-wrap items-center gap-1 md:flex">
            {MAIN_NAV.map(({ to, prefix }) => {
              const active = navLinkActive(pathname, prefix);
              const labelKey =
                prefix === '/home'
                  ? 'nav.home'
                  : prefix === '/forum'
                    ? 'nav.forum'
                    : prefix === '/forum/threads'
                      ? 'forum.topbar.threads'
                    : prefix === '/forum/saved'
                      ? 'forum.topbar.saved'
                    : prefix === '/learning'
                      ? 'nav.learning'
                      : prefix === '/career'
                        ? 'nav.career'
                        : prefix === '/assistant'
                          ? 'forum.topbar.assistant'
                          : 'nav.chat';
              return (
                <Link
                  key={to}
                  to={to}
                  className={`rounded-md px-2 py-1 text-xs font-medium transition-colors ${
                    active ? 'bg-primary/10 text-primary' : 'text-slate-600 hover:bg-slate-100 hover:text-slate-900'
                  }`}
                >
                  {t(labelKey)}
                </Link>
              );
            })}
          </nav>

          <div className="flex shrink-0 flex-wrap items-center justify-end gap-2">
            {isAuthenticated && (showModerationLink || showAdminLink) ? (
              <div className="flex items-center gap-1 rounded-md border border-slate-200 bg-slate-50 p-0.5">
                {showModerationLink ? (
                  <Link
                    to="/mod/reports"
                    className={`rounded px-2 py-1 text-[11px] font-semibold ${
                      pathname.startsWith('/mod') ? 'bg-amber-100 text-amber-900' : 'text-amber-800 hover:bg-amber-50'
                    }`}
                  >
                    {t('forum.topbar.moderation')}
                  </Link>
                ) : null}
                {showAdminLink ? (
                  <Link
                    to="/admin/users"
                    className={`rounded px-2 py-1 text-[11px] font-semibold ${
                      pathname.startsWith('/admin') ? 'bg-rose-100 text-rose-900' : 'text-slate-700 hover:bg-white'
                    }`}
                  >
                    {t('forum.topbar.admin')}
                  </Link>
                ) : null}
              </div>
            ) : null}
            <LanguageSwitcher />
            {isAuthenticated && <NotificationBell />}
            <label className="flex h-8 items-center gap-2 rounded-md border border-slate-300 bg-white px-2 text-slate-500">
              <Search className="h-3.5 w-3.5" />
              <input
                type="search"
                placeholder={t('forum.topbar.searchPlaceholder')}
                className="w-24 border-0 bg-transparent text-xs text-slate-700 outline-none sm:w-44"
              />
            </label>
            {isAuthenticated ? (
              <>
                <span className="hidden max-w-40 truncate text-xs font-medium text-slate-700 sm:inline">
                  {displayName}
                </span>
                <button
                  type="button"
                  onClick={onLogout}
                  className="inline-flex h-8 items-center rounded-md border border-slate-300 px-3 text-xs font-medium text-slate-700 hover:bg-slate-100"
                >
                  {t('auth.logout')}
                </button>
              </>
            ) : (
              <Link
                to="/login"
                className="inline-flex h-8 items-center rounded-md border border-primary/20 px-3 text-xs font-medium text-primary hover:bg-primary/5"
              >
                {t('auth.login')}
              </Link>
            )}
          </div>
        </div>
      </div>
    </header>
  );
}
