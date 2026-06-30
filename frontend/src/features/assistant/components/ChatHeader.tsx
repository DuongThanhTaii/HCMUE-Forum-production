import { Bot } from 'lucide-react';
import { useTranslation } from 'react-i18next';

export function ChatHeader() {
  const { t } = useTranslation('assistant');

  return (
    <div className="flex items-center justify-between p-4 sm:p-6 bg-white border-b border-[#E5E7EB] z-10 flex-shrink-0">
      <div className="flex items-center gap-3.5">
        <div className="relative">
          <div className="w-12 h-12 bg-gradient-to-tr from-[#1E5EFF] to-[#8CAFFF] rounded-[14px] flex items-center justify-center text-white shadow-sm">
            <Bot size={24} />
          </div>
          <div className="absolute -bottom-1 -right-1 w-3.5 h-3.5 bg-[#10B981] border-2 border-white rounded-full"></div>
        </div>
        
        <div>
          <h2 className="text-base font-bold text-[#111827] leading-tight flex items-center gap-1.5">
            {t('header.title', 'HCMUE AI Assistant')}
            <span className="text-xl leading-none">✨</span>
          </h2>
          <p className="text-[13px] text-[#6B7280] mt-0.5 line-clamp-1">
            {t('header.subtitle', 'Ask anything about university regulations, study, departments, schedules and more.')}
          </p>
        </div>
      </div>
      
      <div className="hidden sm:flex items-center gap-2 px-3 py-1.5 bg-[#ECFDF5] rounded-full border border-[#D1FAE5]">
        <div className="w-1.5 h-1.5 bg-[#10B981] rounded-full animate-pulse"></div>
        <span className="text-[11px] font-bold tracking-wider text-[#059669] uppercase">
          {t('header.online', 'ONLINE')}
        </span>
      </div>
    </div>
  );
}
