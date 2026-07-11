import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import {
  useApproveDocumentMutation,
  useGetDocumentByIdQuery,
  useGetDocumentsQuery,
  useRejectDocumentMutation,
  useRequestRevisionDocumentMutation,
  useBulkApproveDocumentsMutation,
  useBulkRejectDocumentsMutation,
  useBulkDeleteDocumentsMutation,
} from '../api/learning.api'

const btnSecondary =
  'rounded border border-slate-200 px-2 py-1 text-xs font-medium text-slate-700 hover:bg-slate-50 disabled:opacity-50'
const btnPrimary =
  'rounded border border-slate-800 px-2 py-1 text-xs font-medium text-slate-900 hover:bg-slate-50 disabled:opacity-50'
const btnMuted =
  'rounded border border-slate-200 px-2 py-1 text-xs font-medium text-slate-600 hover:bg-slate-50 disabled:opacity-50'

export function ModLearningApprovalsPage() {
  const { t } = useTranslation()
  const [statusFilter, setStatusFilter] = useState<number>(2)
  const { data, isLoading, isError } = useGetDocumentsQuery({ pageNumber: 1, pageSize: 50, status: statusFilter })
  const [approveDocument, { isLoading: approving }] = useApproveDocumentMutation()
  const [rejectDocument, { isLoading: rejecting }] = useRejectDocumentMutation()
  const [requestRevision, { isLoading: requesting }] = useRequestRevisionDocumentMutation()
  const [bulkApproveDocuments, { isLoading: bulkApproving }] = useBulkApproveDocumentsMutation()
  const [bulkRejectDocuments, { isLoading: bulkRejecting }] = useBulkRejectDocumentsMutation()
  const [bulkDeleteDocuments, { isLoading: bulkDeleting }] = useBulkDeleteDocumentsMutation()
  const docs = data?.documents ?? []
  const busy = approving || rejecting || requesting || bulkApproving || bulkRejecting || bulkDeleting
  const [selectedDocumentId, setSelectedDocumentId] = useState<string | null>(null)
  const [selectedDocumentIds, setSelectedDocumentIds] = useState<Set<string>>(new Set())
  const [reasonDialog, setReasonDialog] = useState<{ mode: 'reject' | 'revision' | 'bulk-reject' | 'bulk-delete'; documentId: string } | null>(null)
  const [reasonInput, setReasonInput] = useState('')
  const [actionError, setActionError] = useState<string | null>(null)
  const { data: selectedDocument, isLoading: loadingDetail } = useGetDocumentByIdQuery(selectedDocumentId ?? '', {
    skip: !selectedDocumentId,
  })

  async function onApprove(id: string) {
    setActionError(null)
    await approveDocument({ documentId: id }).unwrap()
  }
  async function onBulkApprove() {
    const ids = Array.from(selectedDocumentIds)
    if (ids.length === 0) return
    setActionError(null)
    await bulkApproveDocuments({ documentIds: ids }).unwrap()
    setSelectedDocumentIds(new Set())
  }
  function openReasonDialog(mode: 'reject' | 'revision' | 'bulk-reject' | 'bulk-delete', documentId: string) {
    setActionError(null)
    setReasonDialog({ mode, documentId })
    setReasonInput('')
  }
  function toggleAll(checked: boolean) {
    if (checked) {
      setSelectedDocumentIds(new Set(docs.map((d) => d.id)))
    } else {
      setSelectedDocumentIds(new Set())
    }
  }
  function toggleOne(id: string) {
    const next = new Set(selectedDocumentIds)
    if (next.has(id)) next.delete(id)
    else next.add(id)
    setSelectedDocumentIds(next)
  }
  async function submitReasonDialog() {
    if (!reasonDialog) return
    const reason = reasonInput.trim()
    if (reasonDialog.mode !== 'bulk-delete' && reason.length < 10) {
      setActionError(t('mod.learning.reasonMinLength', 'Lý do quá ngắn'))
      return
    }
    if (reasonDialog.mode === 'reject') {
      await rejectDocument({ documentId: reasonDialog.documentId, reason }).unwrap()
    } else if (reasonDialog.mode === 'revision') {
      await requestRevision({ documentId: reasonDialog.documentId, reason }).unwrap()
    } else if (reasonDialog.mode === 'bulk-reject') {
      const ids = Array.from(selectedDocumentIds)
      await bulkRejectDocuments({ documentIds: ids, reason }).unwrap()
      setSelectedDocumentIds(new Set())
    } else if (reasonDialog.mode === 'bulk-delete') {
      const ids = Array.from(selectedDocumentIds)
      await bulkDeleteDocuments({ documentIds: ids }).unwrap()
      setSelectedDocumentIds(new Set())
    }
    setReasonDialog(null)
    setReasonInput('')
    setActionError(null)
  }

  if (isLoading) {
    return (
      <div className="rounded-lg border border-slate-200 bg-white p-4 text-sm text-slate-600">
        {t('common.loading')}
      </div>
    )
  }
  if (isError) {
    return (
      <div className="rounded-lg border border-rose-200 bg-rose-50 p-4 text-sm text-rose-800">
        {t('mod.learning.loadError')}
      </div>
    )
  }

  return (
    <section className="space-y-3">
      <header className="flex items-center justify-between rounded-lg border border-slate-200 bg-white p-4 shadow-sm">
        <div>
          <h2 className="text-base font-semibold text-slate-900">{t('mod.learning.title', 'Kiểm duyệt tài liệu')}</h2>
          <p className="mt-1 text-sm text-slate-600">{t('mod.learning.totalCount', 'Tổng cộng')} {docs.length} tài liệu</p>
        </div>
        <div>
          <select
            value={statusFilter}
            onChange={(e) => {
              setStatusFilter(Number(e.target.value))
              setSelectedDocumentIds(new Set())
            }}
            className="rounded-lg border border-slate-300 bg-white px-3 py-1.5 text-sm text-slate-700 focus:border-slate-500 focus:outline-none focus:ring-1 focus:ring-slate-500"
          >
            <option value={2}>Chờ duyệt (Pending)</option>
            <option value={3}>Đã duyệt (Approved)</option>
            <option value={4}>Bị từ chối (Rejected)</option>
          </select>
        </div>
      </header>

      <div className="rounded-lg border border-emerald-200 bg-emerald-50/90 px-4 py-3 text-[13px] leading-relaxed text-emerald-950 shadow-sm">
        <p className="font-semibold text-emerald-900">{t('mod.learning.scopeTitle')}</p>
        <p className="mt-1 text-emerald-900/90">{t('mod.learning.scopeBody')}</p>
      </div>

      <div className="flex items-center gap-2">
        <button
          type="button"
          disabled={busy || selectedDocumentIds.size === 0}
          onClick={() => void onBulkApprove()}
          className={btnPrimary}
        >
          {t('mod.learning.bulkApprove', 'Duyệt mục đã chọn')} ({selectedDocumentIds.size})
        </button>
        <button
          type="button"
          disabled={busy || selectedDocumentIds.size === 0}
          onClick={() => openReasonDialog('bulk-reject', '')}
          className={btnMuted}
        >
          {t('mod.learning.bulkReject', 'Từ chối mục đã chọn')} ({selectedDocumentIds.size})
        </button>
        <button
          type="button"
          disabled={busy || selectedDocumentIds.size === 0}
          onClick={() => openReasonDialog('bulk-delete', '')}
          className="rounded border border-rose-300 px-2 py-1 text-xs font-medium text-rose-700 hover:bg-rose-50 disabled:opacity-50"
        >
          Xóa mục đã chọn ({selectedDocumentIds.size})
        </button>
      </div>

      <div className="overflow-hidden rounded-lg border border-slate-200 bg-white shadow-sm">
        <table className="w-full table-auto text-left text-sm">
          <thead className="bg-slate-50 text-slate-600">
            <tr>
              <th className="px-3 py-2 w-10">
                <input
                  type="checkbox"
                  checked={docs.length > 0 && selectedDocumentIds.size === docs.length}
                  onChange={(e) => toggleAll(e.target.checked)}
                  className="rounded border-slate-300 text-slate-900 focus:ring-slate-900"
                />
              </th>
              <th className="px-3 py-2">{t('mod.learning.columns.title')}</th>
              <th className="px-3 py-2">{t('mod.learning.columns.uploader')}</th>
              <th className="px-3 py-2">{t('mod.learning.columns.created')}</th>
              <th className="px-3 py-2">{t('mod.learning.columns.status')}</th>
              <th className="px-3 py-2">{t('mod.learning.columns.actions')}</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-100">
            {docs.map((doc) => (
              <tr key={doc.id}>
                <td className="px-3 py-2">
                  <input
                    type="checkbox"
                    checked={selectedDocumentIds.has(doc.id)}
                    onChange={() => toggleOne(doc.id)}
                    className="rounded border-slate-300 text-slate-900 focus:ring-slate-900"
                  />
                </td>
                <td className="px-3 py-2">{doc.title}</td>
                <td className="px-3 py-2">{doc.uploaderName ?? doc.uploaderId ?? '-'}</td>
                <td className="px-3 py-2">{doc.createdAt ? new Date(doc.createdAt).toLocaleString() : '-'}</td>
                <td className="px-3 py-2">{doc.status ?? '-'}</td>
                <td className="px-3 py-2">
                  <div className="flex flex-wrap gap-2">
                    <button type="button" onClick={() => setSelectedDocumentId(doc.id)} className={btnSecondary}>
                      {t('mod.learning.view')}
                    </button>
                    <button type="button" disabled={busy} onClick={() => void onApprove(doc.id)} className={btnPrimary}>
                      {t('mod.learning.approve')}
                    </button>
                    <button
                      type="button"
                      disabled={busy}
                      onClick={() => openReasonDialog('reject', doc.id)}
                      className={btnMuted}
                    >
                      {t('mod.learning.reject')}
                    </button>
                    <button
                      type="button"
                      disabled={busy}
                      onClick={() => openReasonDialog('revision', doc.id)}
                      className={btnMuted}
                    >
                      {t('mod.learning.requestRevision')}
                    </button>
                  </div>
                </td>
              </tr>
            ))}
            {docs.length === 0 ? (
              <tr>
                <td colSpan={6} className="px-3 py-6 text-center text-sm text-slate-500">
                  {t('mod.learning.noPending')}
                </td>
              </tr>
            ) : null}
          </tbody>
        </table>
      </div>
      {selectedDocumentId ? (
        <div
          className="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/40 p-4"
          onClick={() => setSelectedDocumentId(null)}
        >
          <div
            className="w-full max-w-2xl rounded-lg border border-slate-200 bg-white p-4 shadow-xl"
            onClick={(e) => e.stopPropagation()}
          >
            <div className="flex items-start justify-between gap-2">
              <h3 className="text-base font-semibold text-slate-900">{t('mod.learning.detailTitle')}</h3>
              <button
                type="button"
                onClick={() => setSelectedDocumentId(null)}
                className="rounded border border-slate-200 px-2 py-1 text-xs text-slate-600 hover:bg-slate-50"
              >
                {t('mod.learning.close')}
              </button>
            </div>
            {loadingDetail ? (
              <p className="mt-3 text-sm text-slate-500">{t('mod.learning.loading')}</p>
            ) : selectedDocument ? (
              <div className="mt-3 space-y-2 text-sm">
                <p>
                  <span className="font-medium">{t('mod.learning.columns.title')}:</span> {selectedDocument.title}
                </p>
                <p>
                  <span className="font-medium">{t('mod.learning.descriptionLabel')}:</span>{' '}
                  {selectedDocument.description || '-'}
                </p>
                <p>
                  <span className="font-medium">{t('mod.learning.columns.uploader')}:</span>{' '}
                  {selectedDocument.uploaderDisplayName || selectedDocument.uploaderId}
                </p>
                <p>
                  <span className="font-medium">{t('mod.learning.fileNameLabel')}:</span> {selectedDocument.fileName}
                </p>
                <p>
                  <span className="font-medium">{t('mod.learning.columns.status')}:</span> {selectedDocument.status}
                </p>
                <p>
                  <span className="font-medium">{t('mod.learning.columns.created')}:</span>{' '}
                  {new Date(selectedDocument.createdAt).toLocaleString()}
                </p>
                <div className="flex flex-wrap gap-2 pt-2">
                  <button type="button" disabled={busy} onClick={() => void onApprove(selectedDocument.id)} className={btnPrimary}>
                    {t('mod.learning.approve')}
                  </button>
                  <button
                    type="button"
                    disabled={busy}
                    onClick={() => openReasonDialog('reject', selectedDocument.id)}
                    className={btnMuted}
                  >
                    {t('mod.learning.reject')}
                  </button>
                  <button
                    type="button"
                    disabled={busy}
                    onClick={() => openReasonDialog('revision', selectedDocument.id)}
                    className={btnMuted}
                  >
                    {t('mod.learning.requestRevision')}
                  </button>
                </div>
              </div>
            ) : (
              <p className="mt-3 text-sm text-rose-700">{t('mod.learning.detailLoadError')}</p>
            )}
          </div>
        </div>
      ) : null}
      {reasonDialog ? (
        <div
          className="fixed inset-0 z-[60] flex items-center justify-center bg-slate-900/40 p-4"
          onClick={() => setReasonDialog(null)}
        >
          <div
            className="w-full max-w-lg rounded-lg border border-slate-200 bg-white p-4 shadow-xl"
            onClick={(e) => e.stopPropagation()}
          >
            <h3 className="text-base font-semibold text-slate-900">
              {reasonDialog.mode === 'bulk-delete' 
                ? 'Xóa tài liệu'
                : reasonDialog.mode === 'reject' || reasonDialog.mode === 'bulk-reject'
                ? t('mod.learning.reasonReject', 'Lý do từ chối')
                : t('mod.learning.reasonRevision', 'Yêu cầu chỉnh sửa')}
            </h3>
            {reasonDialog.mode === 'bulk-delete' ? (
              <p className="mt-2 text-sm text-slate-600">Bạn có chắc chắn muốn xóa cứng các tài liệu đã chọn khỏi hệ thống không? Hành động này không thể hoàn tác.</p>
            ) : (
              <textarea
                value={reasonInput}
                onChange={(e) => setReasonInput(e.target.value)}
                rows={5}
                className="mt-3 w-full rounded-md border border-slate-300 px-3 py-2 text-sm focus:border-slate-500 focus:outline-none focus:ring-1 focus:ring-slate-400"
                placeholder={
                  reasonDialog.mode === 'reject' || reasonDialog.mode === 'bulk-reject'
                    ? t('mod.learning.reasonPlaceholderReject', 'Nhập lý do...')
                    : t('mod.learning.reasonPlaceholderRevision', 'Nhập yêu cầu...')
                }
              />
            )}
            {actionError ? <p className="mt-2 text-sm text-rose-700">{actionError}</p> : null}
            <div className="mt-3 flex justify-end gap-2">
              <button
                type="button"
                onClick={() => setReasonDialog(null)}
                className="rounded-md border border-slate-200 px-3 py-1.5 text-sm text-slate-700 hover:bg-slate-50"
              >
                {t('mod.learning.cancel')}
              </button>
              <button
                type="button"
                disabled={busy}
                onClick={() => void submitReasonDialog()}
                className="rounded-md bg-slate-900 px-3 py-1.5 text-sm font-medium text-white hover:bg-slate-800 disabled:opacity-60"
              >
                {busy ? t('mod.learning.processing') : t('mod.learning.confirm')}
              </button>
            </div>
          </div>
        </div>
      ) : null}
    </section>
  )
}
