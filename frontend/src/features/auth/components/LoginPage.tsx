import { Link } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { useLoginForm } from '../hooks/useLoginForm'

export function LoginPage() {
  const { t } = useTranslation()
  const {
    email,
    password,
    errorMessage,
    isLoading,
    isAzureLoading,
    isAzureLoginAvailable,
    setEmail,
    setPassword,
    onSubmit,
    onAzureSignIn,
  } =
    useLoginForm()

  return (
    <div className="space-y-4">
      <header className="space-y-1">
        <h2 className="text-xl font-semibold text-slate-900">{t('auth.login')}</h2>
        <p className="text-sm text-slate-600">{t('auth.pages.loginDescription')}</p>
      </header>

      <form className="space-y-3" onSubmit={onSubmit}>
        <label className="block space-y-1">
          <span className="text-sm font-medium text-slate-700">{t('auth.email')}</span>
          <input
            type="email"
            value={email}
            onChange={(event) => setEmail(event.target.value)}
            placeholder={t('auth.pages.emailPlaceholder')}
            className="w-full rounded-md border border-slate-300 px-3 py-2 text-sm outline-none focus:border-primary"
            disabled={isLoading}
          />
        </label>

        <label className="block space-y-1">
          <span className="text-sm font-medium text-slate-700">{t('auth.password')}</span>
          <input
            type="password"
            value={password}
            onChange={(event) => setPassword(event.target.value)}
            placeholder={t('auth.pages.passwordPlaceholder')}
            className="w-full rounded-md border border-slate-300 px-3 py-2 text-sm outline-none focus:border-primary"
            disabled={isLoading}
          />
        </label>

        {errorMessage ? <p className="text-sm text-jasper">{errorMessage}</p> : null}

        <button
          type="submit"
          className="w-full rounded-md bg-primary px-3 py-2 text-sm font-medium text-white transition-colors hover:bg-primary-hover disabled:cursor-not-allowed disabled:opacity-70"
          disabled={isLoading || isAzureLoading}
        >
          {isLoading ? t('auth.pages.loggingIn') : t('auth.login')}
        </button>

        {isAzureLoginAvailable ? (
          <button
            type="button"
            onClick={() => void onAzureSignIn()}
            className="w-full rounded-md border border-slate-300 bg-white px-3 py-2 text-sm font-medium text-slate-700 transition-colors hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-70"
            disabled={isLoading || isAzureLoading}
          >
            {isAzureLoading ? 'Signing in with Microsoft...' : 'Sign in with Microsoft'}
          </button>
        ) : null}
      </form>

      <p className="text-center text-sm text-slate-600">
        {t('auth.pages.noAccount')}{' '}
        <Link to="/register" className="font-medium text-primary hover:underline">
          {t('auth.register')}
        </Link>
      </p>

      <div className="rounded-md border border-sky-200 bg-sky-50 px-3 py-2 text-sm text-sky-900">
        <p className="font-medium">{t('auth.pages.companyRegisterCtaTitle')}</p>
        <p className="mt-1">{t('auth.pages.companyRegisterCtaDescription')}</p>
        <Link to="/career/company-register" className="mt-2 inline-block font-medium text-sky-700 underline">
          {t('auth.pages.companyRegisterCtaButton')}
        </Link>
      </div>
    </div>
  )
}
