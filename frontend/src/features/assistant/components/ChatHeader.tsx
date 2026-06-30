import { Bot, Sparkles } from 'lucide-react';

export function ChatHeader() {
  return (
    <div className="flex items-center justify-between px-6 py-4 border-b border-[#E5E7EB] bg-white/50 backdrop-blur-md z-10 sticky top-0">
      <div className="flex items-center space-x-4">
        <div className="relative flex items-center justify-center w-12 h-12 rounded-full bg-gradient-to-tr from-[#1E5EFF] to-[#EDF4FF] text-white shadow-sm">
          <Bot size={24} strokeWidth={2.5} />
          <div className="absolute bottom-0 right-0 w-3.5 h-3.5 bg-green-500 border-2 border-white rounded-full"></div>
        </div>
        <div>
          <h2 className="text-lg font-semibold text-[#111827] flex items-center gap-1.5">
            HCMUE AI Assistant
            <Sparkles size={16} className="text-[#1E5EFF]" />
          </h2>
          <p className="text-xs font-medium text-[#6B7280]">
            Ask anything about university regulations, study, departments, schedules and more.
          </p>
        </div>
      </div>
      <div className="hidden sm:flex items-center px-3 py-1 bg-green-50 rounded-full border border-green-100">
        <span className="w-1.5 h-1.5 bg-green-500 rounded-full mr-2 animate-pulse"></span>
        <span className="text-[11px] font-semibold text-green-700 uppercase tracking-wide">Online</span>
      </div>
    </div>
  );
}
