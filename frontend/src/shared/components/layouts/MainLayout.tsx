import { Outlet, useLocation } from 'react-router-dom';
import { ForumTopbar } from './ForumTopbar';
import { ForumSidebar } from '@shared/components/layouts/ForumSidebar';
import { AUTH_BYPASS_IN_DEV } from '@/app/authDevBypass';
import { useAuth } from '@features/auth/context/useAuth';
import { ChatProvider } from '@features/chat/context/ChatContext';
import { ChatDock } from '@features/chat/components/ChatDock';
import { NotificationProvider } from '@features/notifications/context/NotificationContext';

import { CallProvider } from '@features/chat/context/CallContext';
import { GlobalCallOverlay } from '@features/chat/components/GlobalCallOverlay';

import { MobileBottomNav } from './MobileBottomNav';

export function MainLayout() {
  const { isAuthenticated } = useAuth();
  const includeChatShell = isAuthenticated || AUTH_BYPASS_IN_DEV;
  const location = useLocation();
  const isChatPage = location.pathname.startsWith('/chat');
  
  const isFullWidthPage = location.pathname.includes('/assistant') || location.pathname.includes('/chat/ai');
  const isChatThreadOpen = location.pathname.startsWith('/chat') && new URLSearchParams(location.search).has('conversation');

  const layout = (
    <>
      <ForumTopbar />
      <ForumSidebar />
      {!isChatThreadOpen && <MobileBottomNav />}
      <div className={`pt-14 ${isChatThreadOpen ? 'pb-0' : 'pb-14'} lg:pb-0 lg:pl-64 ${isFullWidthPage ? 'h-[100dvh] flex flex-col' : ''}`}>
        <main className={isFullWidthPage ? 'w-full h-full flex-1 flex flex-col' : `mx-auto w-full max-w-7xl ${isChatPage ? 'p-0 md:px-6 md:py-6' : 'px-4 py-5 md:px-6 md:py-6'}`}>
          <Outlet />
        </main>
      </div>
    </>
  );

  // Always provide context to avoid runtime crash in children (e.g. topbar bell)
  // when auth sources are temporarily out of sync during bootstrap.
  const withNotifications = (content: React.ReactNode) => (
    <NotificationProvider>{content}</NotificationProvider>
  );

  return (
    <div className="min-h-screen bg-slate-50">
      {withNotifications(
        includeChatShell ? (
          <ChatProvider>
            <CallProvider>
              {layout}
              <ChatDock />
              <GlobalCallOverlay />
            </CallProvider>
          </ChatProvider>
        ) : (
          layout
        )
      )}
    </div>
  );
}

