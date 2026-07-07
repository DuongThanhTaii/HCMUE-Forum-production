import { Link, useLocation } from 'react-router-dom';
import { Home, LayoutGrid, Bot, MessageSquare } from 'lucide-react';

const NAV_ITEMS = [
  { to: '/home', icon: Home, labelKey: 'nav.home' },
  { to: '/explore', icon: LayoutGrid, labelKey: 'nav.explore' },
  { to: '/assistant', icon: Bot, labelKey: 'forum.topbar.assistant' },
  { to: '/chat', icon: MessageSquare, labelKey: 'nav.chat' },
] as const;

export function MobileBottomNav() {
  const { pathname } = useLocation();

  return (
    <nav className="fixed bottom-0 left-0 right-0 z-50 flex h-14 items-center justify-around border-t border-slate-200 bg-white pb-safe lg:hidden shadow-[0_-1px_3px_rgba(0,0,0,0.05)]">
      {NAV_ITEMS.map(({ to, icon: Icon }) => {
        const isActive = pathname.startsWith(to) && (to !== '/home' || pathname === '/home');

        return (
          <Link
            key={to}
            to={to}
            className={`flex flex-col items-center justify-center w-full h-full space-y-1 transition-colors ${
              isActive ? 'text-primary' : 'text-slate-500 hover:text-slate-800'
            }`}
          >
            <div className={`relative flex items-center justify-center p-1 rounded-full ${isActive ? 'bg-primary/10' : ''}`}>
              <Icon
                className={`h-6 w-6 transition-transform ${
                  isActive ? 'scale-110' : ''
                }`}
                strokeWidth={isActive ? 2.5 : 2}
                fill={isActive ? 'currentColor' : 'none'}
              />
            </div>
          </Link>
        );
      })}
    </nav>
  );
}
