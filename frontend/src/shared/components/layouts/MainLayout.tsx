import { Outlet } from 'react-router-dom';
import { ForumTopbar } from './ForumTopbar';
import { ForumSidebar } from '@shared/components/layouts/ForumSidebar';
import { AUTH_BYPASS_IN_DEV } from '@/app/authDevBypass';
import { useAuth } from '@features/auth/context/useAuth';
import { ChatProvider } from '@features/chat/context/ChatContext';
import { ChatDock } from '@features/chat/components/ChatDock';
import { NotificationProvider } from '@features/notifications/context/NotificationContext';

export function MainLayout() {
  const { isAuthenticated } = useAuth();
  const includeChatShell = isAuthenticated || AUTH_BYPASS_IN_DEV;

  const layout = (
    <>
      <ForumTopbar />
      <ForumSidebar />
      <div className="pt-14 lg:pl-64">
        <main className="mx-auto w-full max-w-7xl px-4 py-5 md:px-6 md:py-6">
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
            {layout}
            <ChatDock />
          </ChatProvider>
        ) : (
          layout
        )
      )}
    </div>
  );
}

