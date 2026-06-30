import { useTranslation } from 'react-i18next';

export function ChatHeader() {
  const { t } = useTranslation('assistant');

  return (
    <div className="flex items-center justify-between p-4 sm:p-6 bg-transparent z-10 flex-shrink-0">
      <div className="flex items-center gap-2">
        <img src="/logochatbot.png" alt="HCMUE AI Logo" className="w-5 h-5 rounded-full object-cover" />
        <h2 className="text-sm font-semibold text-[#6B7280]">
          {t('header.title', 'HCMUE AI Assistant')}
        </h2>
      </div>
    </div>
  );
}
