import { ArrowUp, Command } from 'lucide-react';
import React, { useRef, useEffect } from 'react';
import { useTranslation } from 'react-i18next';

interface ChatInputProps {
  input: string;
  setInput: (value: string) => void;
  onSubmit: (e: React.FormEvent) => void;
  isLoading: boolean;
  isCentered?: boolean;
}

export function ChatInput({ input, setInput, onSubmit, isLoading, isCentered = false }: ChatInputProps) {
  const { t } = useTranslation('assistant');
  const inputRef = useRef<HTMLInputElement>(null);

  useEffect(() => {
    inputRef.current?.focus();
  }, []);

  return (
    <div className={`w-full ${isCentered ? '' : 'p-4 pb-6 sm:p-6 sm:pb-8 bg-transparent z-10 flex-shrink-0'}`}>
      <form onSubmit={onSubmit} className="relative flex items-center max-w-3xl mx-auto w-full">
        <div className="absolute left-4 flex gap-2">
          <button
            type="button"
            disabled={isLoading}
            className="p-1.5 text-[#9CA3AF] hover:text-[#6B7280] transition-colors rounded-lg hover:bg-black/5 disabled:opacity-50"
            title="Attach file"
          >
            <span className="text-lg leading-none">+</span>
          </button>
          <button
            type="button"
            disabled={isLoading}
            className="p-1.5 text-[#9CA3AF] hover:text-[#6B7280] transition-colors rounded-lg hover:bg-black/5 disabled:opacity-50"
            title="Command"
          >
            <Command size={18} />
          </button>
        </div>
        
        <input
          ref={inputRef}
          type="text"
          value={input}
          onChange={(e) => setInput(e.target.value)}
          placeholder={t('input.placeholder', 'Hỏi bất cứ điều gì về học vụ, điểm số, học bổng...')}
          disabled={isLoading}
          className={`w-full pl-24 pr-16 py-4 rounded-2xl border bg-white outline-none shadow-[0_2px_12px_rgba(0,0,0,0.04)] text-[15px] text-[#111827] placeholder:text-[#9CA3AF] transition-all
            ${isCentered 
              ? 'border-[#CF373D] focus:ring-1 focus:ring-[#CF373D]' 
              : 'border-[#E5E7EB] hover:border-[#D1D5DB] focus:border-[#CF373D] focus:ring-1 focus:ring-[#CF373D]'
            }`}
        />
        
        <button
          type="submit"
          disabled={!input.trim() || isLoading}
          className={`absolute right-3 flex items-center justify-center w-8 h-8 rounded-full transition-all duration-200
            ${input.trim() && !isLoading 
              ? 'bg-[#CF373D] text-white shadow-md active:scale-95' 
              : 'bg-[#F3F4F6] text-[#9CA3AF] cursor-not-allowed'
            }`}
        >
          <ArrowUp size={16} strokeWidth={3} />
        </button>
      </form>
    </div>
  );
}
