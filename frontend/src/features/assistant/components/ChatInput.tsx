import { Send, Paperclip } from 'lucide-react';
import React, { useRef, useEffect } from 'react';
import { useTranslation } from 'react-i18next';

interface ChatInputProps {
  input: string;
  setInput: (value: string) => void;
  onSubmit: (e: React.FormEvent) => void;
  isLoading: boolean;
}

export function ChatInput({ input, setInput, onSubmit, isLoading }: ChatInputProps) {
  const { t } = useTranslation('assistant');
  const inputRef = useRef<HTMLInputElement>(null);

  useEffect(() => {
    // Auto-focus input on mount
    inputRef.current?.focus();
  }, []);

  return (
    <div className="p-4 sm:p-6 bg-white border-t border-[#E5E7EB] z-10">
      <form onSubmit={onSubmit} className="relative flex items-center max-w-4xl mx-auto">
        <button
          type="button"
          disabled={isLoading}
          className="absolute left-4 p-2 text-[#9CA3AF] hover:text-[#6B7280] transition-colors rounded-full hover:bg-gray-100 disabled:opacity-50"
          title="Attach file (coming soon)"
        >
          <Paperclip size={20} />
        </button>
        
        <input
          ref={inputRef}
          type="text"
          value={input}
          onChange={(e) => setInput(e.target.value)}
          placeholder={t('input.placeholder', 'Ask HCMUE AI anything...')}
          disabled={isLoading}
          className="w-full pl-14 pr-16 py-4 rounded-full border border-[#E5E7EB] bg-[#F9FAFB] hover:bg-white focus:bg-white focus:ring-2 focus:ring-[#1E5EFF]/30 focus:border-[#1E5EFF] transition-all outline-none shadow-sm text-[15px] text-[#111827] placeholder:text-[#9CA3AF]"
        />
        
        <button
          type="submit"
          disabled={!input.trim() || isLoading}
          className={`absolute right-3 flex items-center justify-center w-10 h-10 rounded-full transition-all duration-200 shadow-sm
            ${input.trim() && !isLoading 
              ? 'bg-[#1E5EFF] text-white hover:bg-blue-700 active:scale-95' 
              : 'bg-[#F3F4F6] text-[#9CA3AF] cursor-not-allowed'
            }`}
        >
          <Send size={18} className={input.trim() && !isLoading ? 'ml-0.5' : ''} />
        </button>
      </form>
      <div className="text-center mt-3">
        <span className="text-xs text-[#9CA3AF] font-medium">
          {t('input.disclaimer', 'HCMUE AI can make mistakes. Verify important regulations.')}
        </span>
      </div>
    </div>
  );
}
