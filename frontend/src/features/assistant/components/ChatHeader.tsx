import { useTranslation } from 'react-i18next';

export function ChatHeader() {
  const { t } = useTranslation('assistant');

  return (
    <div className="flex items-center justify-between p-4 sm:p-6 bg-transparent z-10 flex-shrink-0">
      <div className="flex items-center gap-2.5">
        <img src="/logochatbot.png" alt="HCMUE AI Logo" className="w-6 h-6 object-contain" />
        <h2 className="text-[15px] font-semibold text-[#4B5563]">
          {t('header.title', 'HCMUE AI Assistant')}
        </h2>
      </div>
    </div>
  );
}
