import type { ReactNode } from 'react';

interface ChatLayoutProps {
  children: ReactNode;
}

export function ChatLayout({ children }: ChatLayoutProps) {
  return (
    <div className="w-full h-full flex flex-col bg-[#F9FAFB] overflow-hidden font-sans relative">
      {children}
    </div>
  );
}
