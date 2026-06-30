import type { ReactNode } from 'react';

interface ChatLayoutProps {
  children: ReactNode;
}

export function ChatLayout({ children }: ChatLayoutProps) {
  return (
    <div className="w-full h-full flex flex-col bg-white overflow-hidden font-sans">
      {children}
    </div>
  );
}
