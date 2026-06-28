import { Link, useLocation } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { Home, MessageSquare, BookOpen, Briefcase, MessageCircle, Bot } from 'lucide-react';
import type { ComponentType } from 'react';

type NavItem = {
  to: string;
  key: string;
  icon: ComponentType<{ className?: string }>;
};

const MAIN_ITEMS: NavItem[] = [
  { to: '/forum', key: 'nav.forum', icon: MessageSquare },
  { to: '/learning/documents', key: 'nav.learning', icon: BookOpen },
  { to: '/career/jobs', key: 'nav.career', icon: Briefcase },
  { to: '/chat', key: 'nav.chat', icon: MessageCircle },
];

const SECONDARY_ITEMS: NavItem[] = [{ to: '/chat/ai', key: 'nav.aiAssistant', icon: Bot }];

// Layout Tree:
// AppSidebar [fixed inset-y-0 left-0 z-40 flex w-60 flex-col bg-white border-r border-slate-200]
// ├── Header [h-14 flex items-center px-4 border-b border-slate-200]
// ├── MainNav [flex-1 overflow-y-auto p-2 space-y-1]
// └── Secondary [p-2 border-t border-slate-200]
export function AppSidebar() {
  const { t } = useTranslation();
  const { pathname } = useLocation();

  return (
    <aside className="fixed inset-y-0 left-0 z-40 flex w-60 flex-col bg-white border-r border-slate-200">
      <div className="h-14 flex items-center gap-2 px-4 border-b border-slate-200">
        <div className="w-7 h-7 rounded-md bg-primary text-white flex items-center justify-center">
          <Home className="w-4 h-4" />
        </div>
        <div className="text-sm font-semibold">{t('common.brand')}</div>
      </div>

      <nav className="flex-1 overflow-y-auto p-2 space-y-1">
        {MAIN_ITEMS.map((item) => {
          const active = pathname === item.to || pathname.startsWith(`${item.to}/`);
          const Icon = item.icon;
          return (
            <Link
              key={item.to}
              to={item.to}
              className={`flex items-center gap-3 rounded-lg px-3 py-2 text-sm transition-colors ${
                active ? 'bg-primary/10 text-primary font-medium' : 'text-slate-700 hover:bg-slate-100'
              }`}
            >
              <Icon className="w-4 h-4" />
              <span>{t(item.key)}</span>
            </Link>
          );
        })}
      </nav>

      <div className="p-2 border-t border-slate-200">
        {SECONDARY_ITEMS.map((item) => {
          const active = pathname === item.to || pathname.startsWith(`${item.to}/`);
          const Icon = item.icon;
          return (
            <Link
              key={item.to}
              to={item.to}
              className={`flex items-center gap-3 rounded-lg px-3 py-2 text-sm transition-colors ${
                active ? 'bg-primary/10 text-primary font-medium' : 'text-slate-700 hover:bg-slate-100'
              }`}
            >
              <Icon className="w-4 h-4" />
              <span>{t(item.key)}</span>
            </Link>
          );
        })}
      </div>
    </aside>
  );
}

