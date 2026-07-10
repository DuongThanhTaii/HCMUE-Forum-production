import { Search } from 'lucide-react';
import { Link, useLocation } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { selectIsAuthenticated } from '@features/auth/model/auth.slice';
import { useAppSelector } from '@shared/hooks/useAppSelector';
import { LanguageSwitcher } from '../i18n/LanguageSwitcher';
import { NotificationBell } from '@features/notifications/components/NotificationBell';
import { UserProfileDropdown } from './UserProfileDropdown';
import { InstallAppButton } from './InstallAppButton';
import { useChatContextSafe } from '@features/chat/context/ChatContext';

const MAIN_NAV = [
  { to: '/home', prefix: '/home' },
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
      className={`flex min-w-0 items-center gap-2 rounded-md px-2 py-1 text-primary no-underline hover:bg-slate-100 ${className}`.trim()}
    >
      <img src="/logohcmue-forum.png" alt="" className="h-9 w-auto shrink-0" />
      <span className="truncate text-sm font-bold leading-none">{t('forum.topbar.brand')}</span>
    </Link>
  );
}

export function ForumTopbar() {
  const { t } = useTranslation();
  const { pathname } = useLocation();
  const isAuthenticated = useAppSelector(selectIsAuthenticated);
  const chatContext = useChatContextSafe();

  return (
    <header className="fixed left-0 right-0 top-0 z-40 h-14 border-b border-slate-200 bg-white/95 backdrop-blur">
      <div className="flex h-full w-full">
        <div className="hidden h-full w-64 shrink-0 border-r border-slate-200 px-3 lg:flex lg:items-center">
          <BrandLink className="w-full" />
        </div>

        <Link
          to="/home"
          className="flex h-full shrink-0 items-center gap-1.5 px-3 sm:px-4 text-primary no-underline hover:bg-slate-50 lg:hidden"
        >
          <img src="/logohcmue-forum.png" alt="" className="h-9 w-auto shrink-0" />
          <span className="hidden max-w-[9rem] truncate text-sm font-bold leading-none sm:block sm:max-w-none">
            {t('forum.topbar.brand')}
          </span>
        </Link>

        <div className="flex min-h-0 min-w-0 flex-1 items-center justify-end gap-2 px-3 sm:px-4 md:justify-between md:px-6">
          <nav className="hidden min-w-0 overflow-x-auto items-center gap-1 xl:flex [&::-webkit-scrollbar]:hidden [-ms-overflow-style:none] [scrollbar-width:none]">
            {MAIN_NAV.map(({ to, prefix }) => {
              const active = navLinkActive(pathname, prefix);
              const labelKey = prefix === '/home' ? 'nav.home' : 'nav.chat';
              const isChat = prefix === '/chat';
              
              let badge = null;
              if (isChat && isAuthenticated) {
                // Safely render chat unread count if authenticated
                const unreadCount = chatContext?.totalUnread ?? 0;
                if (unreadCount > 0) {
                  badge = (
                    <span className="ml-1.5 inline-flex items-center justify-center rounded-full bg-red-500 px-1.5 py-0.5 text-[9px] font-bold text-white leading-none">
                      {unreadCount > 99 ? '99+' : unreadCount}
                    </span>
                  );
                }
              }

              return (
                <Link
                  key={to}
                  to={to}
                  className={`flex items-center rounded-md px-2 py-1 text-xs font-medium transition-colors ${
                    active ? 'bg-primary/10 text-primary' : 'text-slate-600 hover:bg-slate-100 hover:text-slate-900'
                  }`}
                >
                  {t(labelKey)}
                  {badge}
                </Link>
              );
            })}
          </nav>

          <div className="flex shrink-0 items-center justify-end gap-1.5 sm:gap-2">
            <InstallAppButton />
            {!isAuthenticated && (
              <div className="hidden sm:block">
                <LanguageSwitcher />
              </div>
            )}
            {isAuthenticated && <NotificationBell />}
            <label className="hidden h-8 items-center gap-2 rounded-md border border-slate-300 bg-white px-2 text-slate-500 sm:flex">
              <Search className="h-3.5 w-3.5" />
              <input
                type="search"
                placeholder={t('forum.topbar.searchPlaceholder')}
                className="w-24 border-0 bg-transparent text-xs text-slate-700 outline-none sm:w-44"
              />
            </label>
            {isAuthenticated ? (
              <UserProfileDropdown />
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
