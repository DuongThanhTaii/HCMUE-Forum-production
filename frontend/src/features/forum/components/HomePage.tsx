import { useState, type FormEvent } from 'react'
import { Link } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { StudentDashboard } from './StudentDashboard'
import { MessageSquare } from 'lucide-react'
import { useAppSelector } from '@shared/hooks/useAppSelector'
import { selectIsAuthenticated, selectUserRole } from '@features/auth/model/auth.slice'
import {
  useBroadcastHomeAnnouncementMutation,
  useGetNotificationsQuery,
} from '@features/notifications/api/notifications.api'

export function HomePage() {
  const { t } = useTranslation()
  const isAuthenticated = useAppSelector(selectIsAuthenticated)
  const roles = useAppSelector(selectUserRole)
  const canBroadcastAnnouncement = roles.includes('Admin') || roles.includes('Moderator')
  const [title, setTitle] = useState('')
  const [message, setMessage] = useState('')
  const [sendEmail, setSendEmail] = useState(false)
  const [broadcastMsg, setBroadcastMsg] = useState<string | null>(null)
  const [broadcastAnnouncement, { isLoading: isBroadcasting }] = useBroadcastHomeAnnouncementMutation()
  const { data: notifications } = useGetNotificationsQuery(
    { pageNumber: 1, pageSize: 50 },
    { skip: !isAuthenticated },
  )

  const announcements = (notifications?.notifications ?? []).filter(
    (item) => item.actionUrl?.includes('announcement=true') && item.channel === 'InApp',
  )

  async function onBroadcast(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setBroadcastMsg(null)
    try {
      const result = await broadcastAnnouncement({
        title: title.trim(),
        message: message.trim(),
        sendEmail,
      }).unwrap()
      setBroadcastMsg(`Đã gửi: In-app ${result.sentInApp} người dùng${sendEmail ? ` · Email ${result.sentEmail}` : ''}`)
      setTitle('')
      setMessage('')
      setSendEmail(false)
    } catch {
      setBroadcastMsg('Gửi thông báo thất bại. Vui lòng thử lại.')
    }
  }

  return (
    <div className="space-y-8">
      {canBroadcastAnnouncement ? (
        <section className="forum-compact-card p-5 transition-colors md:p-6">
          <h2 className="text-base font-semibold text-slate-900">Đăng thông báo trang chủ (Admin/Mod)</h2>
          <form className="mt-3 grid gap-2" onSubmit={onBroadcast}>
            <input
              className="rounded-md border border-slate-300 px-3 py-2 text-sm"
              placeholder="Tiêu đề thông báo"
              value={title}
              onChange={(e) => setTitle(e.target.value)}
              required
            />
            <textarea
              className="rounded-md border border-slate-300 px-3 py-2 text-sm"
              rows={3}
              placeholder="Nội dung thông báo"
              value={message}
              onChange={(e) => setMessage(e.target.value)}
              required
            />
            <label className="inline-flex items-center gap-2 text-sm text-slate-600">
              <input type="checkbox" checked={sendEmail} onChange={(e) => setSendEmail(e.target.checked)} />
              Gửi thêm qua email
            </label>
            <button
              type="submit"
              disabled={isBroadcasting}
              className="w-fit rounded-md bg-primary px-4 py-2 text-sm font-medium text-white disabled:opacity-60"
            >
              {isBroadcasting ? t('common.loading') : 'Gửi thông báo'}
            </button>
            {broadcastMsg ? <p className="text-sm text-slate-600">{broadcastMsg}</p> : null}
          </form>
        </section>
      ) : null}

      {announcements.length > 0 ? (
        <section className="forum-compact-card p-5 transition-colors md:p-6">
          <h2 className="text-xs font-semibold uppercase tracking-wide text-slate-500">Thông báo từ quản trị</h2>
          <div className="mt-3 space-y-3">
            {announcements.slice(0, 5).map((item) => (
              <article key={item.id} className="rounded-md border border-amber-200 bg-amber-50 px-3 py-2">
                <h3 className="text-sm font-semibold text-amber-900">{item.subject}</h3>
                <p className="mt-1 text-sm text-amber-800">{item.body}</p>
              </article>
            ))}
          </div>
        </section>
      ) : null}

      <section className="forum-compact-card p-5 transition-colors md:p-6">
        <h1 className="text-xl font-semibold text-slate-900 md:text-2xl">
          {t('home.welcomeTitle')}
        </h1>
        <p className="mt-2 max-w-2xl text-sm leading-relaxed text-slate-600 md:text-base">
          {t('home.welcomeSubtitle')}
        </p>
        <Link
          to="/explore"
          className="mt-5 inline-flex cursor-pointer items-center gap-2 rounded-md border border-primary bg-primary px-4 py-2 text-sm font-medium text-white transition-colors duration-200 hover:bg-primary-hover"
        >
          <MessageSquare className="h-4 w-4" aria-hidden />
          {t('home.gotoForum')}
        </Link>
      </section>

      <StudentDashboard />
    </div>
  )
}
