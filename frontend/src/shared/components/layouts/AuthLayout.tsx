import { Link, Outlet } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { LanguageSwitcher } from '../i18n/LanguageSwitcher';

// Layout Tree:
// AuthLayout [min-h-screen flex items-center justify-center p-6 bg-gradient-to-br from-primary to-primary-hover]
// └── Card [w-full max-w-md rounded-2xl bg-white p-8 shadow-xl]
export function AuthLayout() {
  const { t } = useTranslation();

  return (
    <div className="relative min-h-screen flex items-center justify-center p-6 bg-gradient-to-br from-primary to-primary-hover">
      <div className="absolute right-4 top-4">
        <LanguageSwitcher />
      </div>
      <div className="w-full max-w-md rounded-2xl bg-white p-8 shadow-xl">
        <Link
          to="/home"
          className="mb-6 flex items-center gap-3 rounded-md text-slate-900 no-underline outline-none transition-opacity hover:opacity-90 focus-visible:ring-2 focus-visible:ring-primary focus-visible:ring-offset-2"
        >
          <img src="/logohcmue-forum.png" alt="" className="h-10 w-auto" />
          <h1 className="text-xl font-semibold">{t('auth.layout.brand')}</h1>
        </Link>
        <Outlet />
      </div>
    </div>
  );
}

