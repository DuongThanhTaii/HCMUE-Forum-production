import i18n from 'i18next';
import { initReactI18next } from 'react-i18next';
import vi from './locales/vi';
import en from './locales/en';

export const LANGUAGE_STORAGE_KEY = 'unihub.language';

const resources = {
  vi: { translation: vi },
  en: { translation: en },
} as const;

const getInitialLanguage = () => {
  if (typeof window === 'undefined') {
    return 'vi';
  }

  const savedLanguage = window.localStorage.getItem(LANGUAGE_STORAGE_KEY);
  if (savedLanguage === 'vi' || savedLanguage === 'en') {
    return savedLanguage;
  }

  const browserLanguage = window.navigator.language.toLowerCase();
  return browserLanguage.startsWith('en') ? 'en' : 'vi';
};

void i18n.use(initReactI18next).init({
  resources,
  lng: getInitialLanguage(),
  fallbackLng: 'vi',
  interpolation: { escapeValue: false },
});

i18n.on('languageChanged', (language) => {
  if (typeof window === 'undefined') {
    return;
  }
  if (language === 'vi' || language === 'en') {
    window.localStorage.setItem(LANGUAGE_STORAGE_KEY, language);
  }
});

export default i18n;

