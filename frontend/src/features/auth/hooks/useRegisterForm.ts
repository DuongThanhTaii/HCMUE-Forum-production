import { useState, type FormEvent } from 'react'
import { useNavigate } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { useRegisterMutation } from '../api/auth.api'

export function useRegisterForm() {
  const { t } = useTranslation()
  const navigate = useNavigate()
  const [register, { isLoading }] = useRegisterMutation()

  const [fullName, setFullName] = useState('')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [confirmPassword, setConfirmPassword] = useState('')
  const [errorMessage, setErrorMessage] = useState('')

  const onSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    setErrorMessage('')

    if (!fullName.trim() || !email.trim() || !password.trim()) {
      setErrorMessage(t('auth.pages.validation.registerRequired'))
      return
    }

    if (password !== confirmPassword) {
      setErrorMessage(t('auth.passwordMismatch'))
      return
    }

    try {
      await register({
        fullName: fullName.trim(),
        email: email.trim(),
        password,
      }).unwrap()
      navigate('/login')
    } catch {
      setErrorMessage(t('auth.pages.registerFailed'))
    }
  }

  return {
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
  }
}
