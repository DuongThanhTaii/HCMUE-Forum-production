import { useState } from 'react'
import { PanelRightOpen } from 'lucide-react'
import { useModPostsPage } from '../hooks/useModPostsPage'
import { ModPostPreviewDrawer } from './ModPostPreviewDrawer'

function formatDate(value: string) {
  const d = new Date(value)
  if (Number.isNaN(d.getTime())) return value
  return d.toLocaleDateString()
}

export function ModPostsPage() {
  const { t, posts, isLoading, isError, isEmpty, onApprove, isPublishing, feedback } = useModPostsPage()
  const [previewPostId, setPreviewPostId] = useState<string | null>(null)

  if (isLoading) {
    return <div className="rounded-lg border border-slate-200 bg-white p-4 text-sm text-slate-600">{t('common.loading')}</div>
  }

  if (isError) {
    return (
      <div className="rounded-lg border border-rose-200 bg-rose-50 p-4 text-sm text-rose-700">
        {t('forum.mod.feedback.loadFailed')}
      </div>
    )
  }

  if (isEmpty) {
    return <div className="rounded-lg border border-slate-200 bg-white p-4 text-sm text-slate-600">{t('forum.mod.noPendingPosts')}</div>
  }

  return (
    <section className="space-y-3">
      <header className="rounded-lg border border-slate-200 bg-white p-4">
        <h2 className="text-base font-semibold text-slate-900">{t('forum.mod.pendingPostsTitle')}</h2>
        <p className="mt-1 text-[13px] text-slate-600">{t('forum.mod.pendingPostsHint')}</p>
        {feedback ? <p className="mt-2 text-sm text-slate-600">{feedback}</p> : null}
      </header>
      <div className="overflow-hidden rounded-lg border border-slate-200 bg-white">
        <table className="w-full table-auto text-left text-sm">
          <thead className="bg-slate-50 text-slate-600">
            <tr>
              <th className="px-3 py-2">{t('forum.mod.columns.title')}</th>
              <th className="px-3 py-2">{t('forum.mod.columns.author')}</th>
              <th className="px-3 py-2">{t('forum.mod.columns.category')}</th>
              <th className="px-3 py-2">{t('forum.mod.columns.createdAt')}</th>
              <th className="px-3 py-2">{t('forum.mod.columns.comments')}</th>
              <th className="px-3 py-2 whitespace-nowrap">{t('forum.mod.columns.actions')}</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-100">
            {posts.map((post) => (
              <tr key={post.id}>
                <td className="px-3 py-2 font-medium text-slate-900">{post.title}</td>
                <td className="px-3 py-2 text-slate-600">{post.authorName ?? t('common.noData')}</td>
                <td className="px-3 py-2 text-slate-600">{post.categoryName ?? t('common.noData')}</td>
                <td className="px-3 py-2 text-slate-500">{formatDate(post.createdAt)}</td>
                <td className="px-3 py-2 text-slate-500">{post.commentCount}</td>
                <td className="px-3 py-2">
                  <div className="flex flex-wrap items-center gap-1.5">
                    <button
                      type="button"
                      disabled={isPublishing}
                      onClick={() => setPreviewPostId(post.id)}
                      className="inline-flex items-center gap-1 rounded-md border border-slate-800 bg-slate-900 px-2 py-1 text-[12px] font-medium text-white hover:bg-slate-800 disabled:opacity-50"
                      title={t('forum.mod.pendingPostsHint')}
                    >
                      <PanelRightOpen className="h-3.5 w-3.5 shrink-0" aria-hidden />
                      {t('forum.mod.actions.viewDetail')}
                    </button>
                    <button
                      type="button"
                      disabled={isPublishing}
                      onClick={() => void onApprove(post.id)}
                      className="rounded-md border border-slate-300 bg-white px-2 py-1 text-[12px] font-medium text-slate-900 hover:border-slate-400 hover:bg-slate-50 disabled:opacity-50"
                    >
                      {t('forum.mod.actions.approve')}
                    </button>
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {previewPostId ? (
        <ModPostPreviewDrawer
          postId={previewPostId}
          onClose={() => setPreviewPostId(null)}
          onApprove={(id) => {
            void onApprove(id)
            setPreviewPostId(null)
          }}
          isPublishing={isPublishing}
        />
      ) : null}
    </section>
  )
}
