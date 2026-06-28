import { useTranslation } from 'react-i18next';

type LanguageSwitcherProps = {
  className?: string;
};

export function LanguageSwitcher({ className = '' }: LanguageSwitcherProps) {
  const { i18n, t } = useTranslation();
  const currentLanguage = i18n.resolvedLanguage === 'en' ? 'en' : 'vi';

  return (
    <div
      className={`inline-flex h-8 items-center rounded-md border border-slate-300 bg-white p-0.5 ${className}`.trim()}
      aria-label={t('nav.language')}
    >
      <button
        type="button"
        onClick={() => void i18n.changeLanguage('vi')}
        className={`rounded px-2 py-1 text-xs transition-colors ${
          currentLanguage === 'vi' ? 'bg-primary text-white' : 'text-slate-700 hover:bg-slate-100'
        }`}
      >
        VI
      </button>
      <button
        type="button"
        onClick={() => void i18n.changeLanguage('en')}
        className={`rounded px-2 py-1 text-xs transition-colors ${
          currentLanguage === 'en' ? 'bg-primary text-white' : 'text-slate-700 hover:bg-slate-100'
        }`}
      >
        EN
      </button>
    </div>
  );
}
