import { useState } from 'react'
import { PanelRightOpen } from 'lucide-react'
import { useModPostsPage } from '../hooks/useModPostsPage'
import { ModPostPreviewDrawer } from './ModPostPreviewDrawer'

function RejectModal({
  isOpen,
  onClose,
  onConfirm,
  isRejecting,
  isDelete,
}: {
  isOpen: boolean
  onClose: () => void
  onConfirm: (reason: string) => void
  isRejecting: boolean
  isDelete?: boolean
}) {
  const [reason, setReason] = useState('')

  if (!isOpen) return null

  if (isDelete) {
    return (
      <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4 backdrop-blur-sm">
        <div className="w-full max-w-md rounded-xl bg-white p-6 shadow-2xl">
          <h3 className="text-lg font-semibold text-slate-900">Xóa bài viết</h3>
          <p className="mt-2 text-sm text-slate-600">
            Bạn có chắc chắn muốn xóa cứng các bài viết đã chọn khỏi hệ thống không? Hành động này không thể hoàn tác.
          </p>
          <div className="mt-6 flex justify-end gap-3">
            <button
              type="button"
              onClick={onClose}
              disabled={isRejecting}
              className="rounded-lg px-4 py-2 text-sm font-medium text-slate-700 hover:bg-slate-100 disabled:opacity-50"
            >
              Hủy
            </button>
            <button
              type="button"
              onClick={() => onConfirm('')}
              disabled={isRejecting}
              className="rounded-lg bg-rose-600 px-4 py-2 text-sm font-medium text-white hover:bg-rose-700 disabled:opacity-50"
            >
              Xác nhận xóa
            </button>
          </div>
        </div>
      </div>
    )
  }

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4 backdrop-blur-sm">
      <div className="w-full max-w-md rounded-xl bg-white p-6 shadow-2xl">
        <h3 className="text-lg font-semibold text-slate-900">Từ chối bài viết</h3>
        <p className="mt-2 text-sm text-slate-600">
          Vui lòng cung cấp lý do từ chối bài viết này. Tác giả sẽ nhận được thông báo kèm theo lý do của bạn.
        </p>
        <div className="mt-4">
          <textarea
            value={reason}
            onChange={(e) => setReason(e.target.value)}
            className="w-full rounded-lg border border-slate-300 p-3 text-sm focus:border-rose-500 focus:outline-none focus:ring-1 focus:ring-rose-500"
            rows={4}
            placeholder="Ví dụ: Bài viết vi phạm nội quy diễn đàn..."
          />
        </div>
        <div className="mt-6 flex justify-end gap-3">
          <button
            type="button"
            onClick={onClose}
            disabled={isRejecting}
            className="rounded-lg px-4 py-2 text-sm font-medium text-slate-700 hover:bg-slate-100 disabled:opacity-50"
          >
            Hủy
          </button>
          <button
            type="button"
            onClick={() => onConfirm(reason)}
            disabled={isRejecting || !reason.trim()}
            className="rounded-lg bg-rose-600 px-4 py-2 text-sm font-medium text-white hover:bg-rose-700 disabled:opacity-50"
          >
            Xác nhận từ chối
          </button>
        </div>
      </div>
    </div>
  )
}

function formatDate(value: string) {
  const d = new Date(value)
  if (Number.isNaN(d.getTime())) return value
  return d.toLocaleDateString()
}

export function ModPostsPage() {
  const {
    t,
    posts,
    isLoading,
    isError,
    isEmpty,
    onApprove,
    onReject,
    onApproveBulk,
    onDeleteBulk,
    isPublishing,
    feedback,
    selectedIds,
    toggleSelection,
    toggleAll,
    statusFilter,
    setStatusFilter,
  } = useModPostsPage()
  
  const [previewPostId, setPreviewPostId] = useState<string | null>(null)
  const [rejectingPostId, setRejectingPostId] = useState<string | null>(null)
  const [isDeletingBulk, setIsDeletingBulk] = useState(false)

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
      <header className="flex flex-wrap items-center justify-between gap-4 rounded-lg border border-slate-200 bg-white p-4">
        <div>
          <h2 className="text-base font-semibold text-slate-900">{t('forum.mod.pendingPostsTitle', 'Quản lý Bài viết')}</h2>
          <p className="mt-1 text-[13px] text-slate-600">{t('forum.mod.pendingPostsHint', 'Duyệt, từ chối hoặc xóa các bài viết từ cộng đồng.')}</p>
          {feedback ? <p className="mt-2 text-sm font-medium text-emerald-600">{feedback}</p> : null}
        </div>
        
        <div className="flex items-center gap-2">
          <select
            value={statusFilter}
            onChange={(e) => {
              setStatusFilter(Number(e.target.value))
              if (selectedIds.length > 0) toggleAll() // clear selection
            }}
            className="rounded-lg border border-slate-300 bg-white px-3 py-1.5 text-sm text-slate-700 focus:border-slate-500 focus:outline-none focus:ring-1 focus:ring-slate-500"
          >
            <option value={1}>Chờ duyệt (Pending)</option>
            <option value={2}>Đã đăng (Published)</option>
            <option value={3}>Bị từ chối (Rejected)</option>
          </select>
          
          {selectedIds.length > 0 && (
            <>
              <button
                type="button"
                disabled={isPublishing}
                onClick={() => void onApproveBulk()}
                className="inline-flex items-center gap-2 rounded-lg bg-emerald-600 px-4 py-2 text-sm font-medium text-white hover:bg-emerald-700 disabled:opacity-50"
              >
                Duyệt {selectedIds.length} bài
              </button>
              <button
                type="button"
                disabled={isPublishing}
                onClick={() => setIsDeletingBulk(true)}
                className="inline-flex items-center gap-2 rounded-lg border border-rose-300 px-4 py-2 text-sm font-medium text-rose-700 hover:bg-rose-50 disabled:opacity-50"
              >
                Xóa {selectedIds.length} bài
              </button>
            </>
          )}
        </div>
      </header>
      <div className="overflow-hidden rounded-lg border border-slate-200 bg-white">
        <table className="w-full table-auto text-left text-sm">
          <thead className="bg-slate-50 text-slate-600">
            <tr>
              <th className="px-3 py-3 w-10 text-center">
                <input 
                  type="checkbox" 
                  className="rounded border-slate-300 text-slate-900 focus:ring-slate-900"
                  checked={posts.length > 0 && selectedIds.length === posts.length}
                  onChange={toggleAll}
                />
              </th>
              <th className="px-3 py-2">{t('forum.mod.columns.title', 'Tiêu đề')}</th>
              <th className="px-3 py-2">{t('forum.mod.columns.author')}</th>
              <th className="px-3 py-2">{t('forum.mod.columns.category')}</th>
              <th className="px-3 py-2">{t('forum.mod.columns.createdAt')}</th>
              <th className="px-3 py-2">{t('forum.mod.columns.comments')}</th>
              <th className="px-3 py-2 whitespace-nowrap">{t('forum.mod.columns.actions')}</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-100">
            {posts.map((post) => (
              <tr key={post.id} className={selectedIds.includes(post.id) ? 'bg-slate-50' : ''}>
                <td className="px-3 py-2 text-center">
                  <input 
                    type="checkbox" 
                    className="rounded border-slate-300 text-slate-900 focus:ring-slate-900"
                    checked={selectedIds.includes(post.id)}
                    onChange={() => toggleSelection(post.id)}
                  />
                </td>
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
                      className="rounded-md border border-slate-300 bg-white px-2 py-1 text-[12px] font-medium text-emerald-700 hover:border-emerald-400 hover:bg-emerald-50 disabled:opacity-50"
                    >
                      {t('forum.mod.actions.approve', 'Duyệt & đăng')}
                    </button>
                    <button
                      type="button"
                      disabled={isPublishing}
                      onClick={() => setRejectingPostId(post.id)}
                      className="rounded-md border border-slate-300 bg-white px-2 py-1 text-[12px] font-medium text-rose-700 hover:border-rose-400 hover:bg-rose-50 disabled:opacity-50"
                    >
                      Từ chối
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

      <RejectModal
        isOpen={rejectingPostId !== null || isDeletingBulk}
        onClose={() => {
          setRejectingPostId(null)
          setIsDeletingBulk(false)
        }}
        isRejecting={isPublishing}
        isDelete={isDeletingBulk}
        onConfirm={(reason) => {
          if (isDeletingBulk) {
            void onDeleteBulk()
            setIsDeletingBulk(false)
          } else if (rejectingPostId) {
            void onReject(rejectingPostId, reason)
            setRejectingPostId(null)
          }
        }}
      />
    </section>
  )
}
