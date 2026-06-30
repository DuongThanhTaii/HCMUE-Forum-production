import type { ReactNode } from 'react';

interface ChatLayoutProps {
  children: ReactNode;
}

export function ChatLayout({ children }: ChatLayoutProps) {
  return (
    <section className="bg-[#F7F9FC] w-full min-h-[calc(100vh-80px)] flex justify-center p-4 sm:p-6 lg:p-8 font-sans transition-colors">
      <div className="flex flex-col w-full max-w-[900px] h-[calc(100vh-140px)] min-h-[600px] bg-white rounded-[24px] shadow-[0_12px_40px_-12px_rgba(0,0,0,0.08)] border border-[#E5E7EB] overflow-hidden">
        {children}
      </div>
    </section>
  );
}
