import { Link } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { useRegisterForm } from '../hooks/useRegisterForm'

export function RegisterPage() {
  const { t } = useTranslation()
  const {
    fullName,
    email,
    password,
    confirmPassword,
    errorMessage,
    isLoading,
    setFullName,
    setEmail,
    setPassword,
    setConfirmPassword,
    onSubmit,
  } = useRegisterForm()

  return (
    <div className="space-y-4">
      <header className="space-y-1">
        <h2 className="text-xl font-semibold text-slate-900">{t('auth.register')}</h2>
        <p className="text-sm text-slate-600">{t('auth.pages.registerDescription')}</p>
      </header>

      <form className="space-y-3" onSubmit={onSubmit}>
        <label className="block space-y-1">
          <span className="text-sm font-medium text-slate-700">{t('auth.fullName')}</span>
          <input
            type="text"
            value={fullName}
            onChange={(event) => setFullName(event.target.value)}
            placeholder={t('auth.pages.fullNamePlaceholder')}
            className="w-full rounded-md border border-slate-300 px-3 py-2 text-sm outline-none focus:border-primary"
            disabled={isLoading}
          />
        </label>

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

        <label className="block space-y-1">
          <span className="text-sm font-medium text-slate-700">{t('auth.confirmPassword')}</span>
          <input
            type="password"
            value={confirmPassword}
            onChange={(event) => setConfirmPassword(event.target.value)}
            placeholder={t('auth.pages.passwordPlaceholder')}
            className="w-full rounded-md border border-slate-300 px-3 py-2 text-sm outline-none focus:border-primary"
            disabled={isLoading}
          />
        </label>

        {errorMessage ? <p className="text-sm text-jasper">{errorMessage}</p> : null}

        <button
          type="submit"
          className="w-full rounded-md bg-primary px-3 py-2 text-sm font-medium text-white transition-colors hover:bg-primary-hover disabled:cursor-not-allowed disabled:opacity-70"
          disabled={isLoading}
        >
          {isLoading ? t('auth.pages.registering') : t('auth.register')}
        </button>
      </form>

      <p className="text-center text-sm text-slate-600">
        {t('auth.pages.haveAccount')}{' '}
        <Link to="/login" className="font-medium text-primary hover:underline">
          {t('auth.login')}
        </Link>
      </p>
    </div>
  )
}
