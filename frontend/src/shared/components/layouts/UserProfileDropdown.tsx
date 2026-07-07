import { useState, useRef, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { Link, useLocation } from 'react-router-dom';
import { ChevronDown, LogOut, ShieldAlert, ShieldCheck } from 'lucide-react';
import { useAppDispatch } from '@shared/hooks/useAppDispatch';
import { useAppSelector } from '@shared/hooks/useAppSelector';
import { logout, selectAuth } from '@features/auth/model/auth.slice';
import { LanguageSwitcher } from '../i18n/LanguageSwitcher';

export function UserProfileDropdown() {
  const { t } = useTranslation();
  const { pathname } = useLocation();
  const dispatch = useAppDispatch();
  const auth = useAppSelector(selectAuth);
  const [isOpen, setIsOpen] = useState(false);
  const dropdownRef = useRef<HTMLDivElement>(null);

  const user = auth.user;
  if (!user) return null;

  const displayName = user.fullName || user.email || 'User';
  const initial = displayName.charAt(0).toUpperCase();

  const normalizedRoles = (user.roles ?? []).map((r) => r.toLowerCase());
  const showModerationLink = normalizedRoles.some((r) => r === 'moderator');
  const showAdminLink = normalizedRoles.some((r) => r === 'admin');

  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (dropdownRef.current && !dropdownRef.current.contains(event.target as Node)) {
        setIsOpen(false);
      }
    };
    if (isOpen) {
      document.addEventListener('mousedown', handleClickOutside);
    }
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, [isOpen]);

  // Đóng dropdown khi chuyển trang
  useEffect(() => {
    setIsOpen(false);
  }, [pathname]);

  return (
    <div className="relative" ref={dropdownRef}>
      <button
        type="button"
        onClick={() => setIsOpen(!isOpen)}
        className="flex h-9 items-center gap-2 rounded-full border border-transparent px-1.5 py-1 text-sm font-medium text-slate-700 hover:bg-slate-100 focus:outline-none focus:ring-2 focus:ring-primary/20 transition-all"
      >
        <div className="flex h-7 w-7 items-center justify-center rounded-full bg-primary/10 text-xs font-bold text-primary ring-1 ring-primary/20">
          {initial}
        </div>
        <span className="hidden max-w-32 truncate font-semibold sm:inline">{displayName}</span>
        <ChevronDown className={`h-4 w-4 text-slate-400 transition-transform duration-200 ${isOpen ? 'rotate-180' : ''}`} />
      </button>

      {isOpen && (
        <div className="absolute right-0 top-full mt-2 w-64 origin-top-right rounded-xl border border-slate-200 bg-white shadow-xl animate-in fade-in zoom-in-95 duration-200">
          {/* Header */}
          <div className="px-4 py-3 border-b border-slate-100 bg-slate-50/50 rounded-t-xl">
            <p className="truncate text-sm font-bold text-slate-900">{displayName}</p>
            <p className="truncate text-xs text-slate-500">{user.email}</p>
          </div>
          
          <div className="p-1.5 space-y-0.5">
            {/* Admin / Mod Links */}
            {showModerationLink && (
              <Link
                to="/mod/reports"
                className="flex w-full items-center gap-2.5 rounded-lg px-3 py-2 text-sm font-medium text-amber-700 hover:bg-amber-50"
              >
                <ShieldAlert className="h-4 w-4" />
                {t('forum.topbar.moderation')}
              </Link>
            )}
            
            {showAdminLink && (
              <Link
                to="/admin/users"
                className="flex w-full items-center gap-2.5 rounded-lg px-3 py-2 text-sm font-medium text-rose-700 hover:bg-rose-50"
              >
                <ShieldCheck className="h-4 w-4" />
                {t('forum.topbar.admin')}
              </Link>
            )}

            {(showModerationLink || showAdminLink) && (
              <div className="my-1 border-b border-slate-100 px-3" />
            )}

            {/* Language Switcher */}
            <div className="flex items-center justify-between rounded-lg px-3 py-2 hover:bg-slate-50">
              <span className="text-sm font-medium text-slate-700">{t('nav.language')}</span>
              <LanguageSwitcher />
            </div>

            <div className="my-1 border-b border-slate-100 px-3" />

            {/* Logout */}
            <button
              type="button"
              onClick={() => dispatch(logout())}
              className="flex w-full items-center gap-2.5 rounded-lg px-3 py-2 text-sm font-medium text-slate-600 hover:bg-red-50 hover:text-red-600 transition-colors"
            >
              <LogOut className="h-4 w-4" />
              {t('auth.logout')}
            </button>
          </div>
        </div>
      )}
    </div>
  );
}
