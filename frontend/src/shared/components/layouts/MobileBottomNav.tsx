import { Link, useLocation } from 'react-router-dom';
import { Home, LayoutGrid, Bot, MessageSquare, Menu } from 'lucide-react';
import { useTranslation } from 'react-i18next';

const NAV_ITEMS = [
  { to: '/home', icon: Home, labelKey: 'nav.home' },
  { to: '/explore', icon: LayoutGrid, labelKey: 'nav.explore' },
  { to: '/assistant', icon: Bot, labelKey: 'forum.topbar.assistant' },
  { to: '/chat', icon: MessageSquare, labelKey: 'nav.chat' },
  // Menu có thể mở một drawer hoặc navigate tới trang menu riêng. Tạm thời map tới /menu hoặc /home.
  { to: '/menu', icon: Menu, labelKey: 'Menu' }, // Cần xử lý drawer ở bước sau
] as const;

export function MobileBottomNav() {
  const { pathname } = useLocation();
  const { t } = useTranslation();

  return (
    <nav className="fixed bottom-0 left-0 right-0 z-50 flex h-14 items-center justify-around border-t border-slate-200 bg-white pb-safe lg:hidden shadow-[0_-1px_3px_rgba(0,0,0,0.05)]">
      {NAV_ITEMS.map(({ to, icon: Icon, labelKey }) => {
        const isActive = pathname.startsWith(to) && (to !== '/home' || pathname === '/home');
        // Đối với 'Menu', tạm thời không active hoặc so sánh /menu
        const isMenu = to === '/menu';

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
                fill={isActive && !isMenu ? 'currentColor' : 'none'}
              />
            </div>
          </Link>
        );
      })}
    </nav>
  );
}
