import { Fragment, useCallback, useMemo, useState } from 'react'
import { Pencil, Plus, Trash2 } from 'lucide-react'
import { useAdminCategoriesPage } from '../hooks/useAdminCategoriesPage'
import type { ForumCategoryOption } from '../../../forum/api/forum.list.api'

type UpsertCategoryRequest = Omit<ForumCategoryOption, 'id' | 'postCount'> & { isActive: boolean }
type DraftOverrides = Partial<UpsertCategoryRequest>

const emptyCreate: UpsertCategoryRequest = {
  name: '',
  description: '',
  slug: '',
  displayOrder: 0,
  isActive: true,
  parentCategoryId: null,
}

function toUpsert(category: ForumCategoryOption): UpsertCategoryRequest {
  return {
    name: category.name,
    description: category.description,
    slug: category.slug,
    displayOrder: category.displayOrder,
    isActive: true, // Assuming true by default if not available in API, adjust if needed
    parentCategoryId: category.parentCategoryId,
  }
}

type DiffRow = { field: string; before: string; after: string }

function buildDiff(before: UpsertCategoryRequest, after: UpsertCategoryRequest, labels: Record<string, string>): DiffRow[] {
  const keys = Object.keys(before) as (keyof UpsertCategoryRequest)[]
  const rows: DiffRow[] = []
  for (const key of keys) {
    const b = before[key]
    const a = after[key]
    const bv = typeof b === 'boolean' ? (b ? 'true' : 'false') : String(b ?? '')
    const av = typeof a === 'boolean' ? (a ? 'true' : 'false') : String(a ?? '')
    if (bv !== av) rows.push({ field: labels[key as string] ?? String(key), before: bv, after: av })
  }
  return rows
}

function formatUpsertValue(v: unknown): string {
  if (typeof v === 'boolean') return v ? 'true' : 'false'
  if (v === null || v === undefined) return ''
  return String(v)
}

function buildCreatePreview(after: UpsertCategoryRequest, labels: Record<string, string>): DiffRow[] {
  const keys = Object.keys(after) as (keyof UpsertCategoryRequest)[]
  return keys.map((key) => ({
    field: labels[key as string] ?? String(key),
    before: '—',
    after: formatUpsertValue(after[key]) || '—',
  }))
}

function normalizeUpsert(u: UpsertCategoryRequest): UpsertCategoryRequest {
  return {
    ...u,
    slug: u.slug.trim(),
    name: u.name.trim(),
    description: u.description?.trim() || '',
  }
}

type ConfirmState =
  | { mode: 'idle' }
  | { mode: 'update'; id: string; name: string; before: UpsertCategoryRequest; after: UpsertCategoryRequest; diffs: DiffRow[] }
  | { mode: 'create'; after: UpsertCategoryRequest; diffs: DiffRow[] }
  | { mode: 'delete'; id: string; name: string }

function CategoryEditor({
  draft,
  fieldLabels,
  parentOptions,
  onChange,
}: {
  draft: UpsertCategoryRequest
  fieldLabels: Record<string, string>
  parentOptions: ForumCategoryOption[]
  onChange: <K extends keyof UpsertCategoryRequest>(key: K, value: UpsertCategoryRequest[K]) => void
}) {
  return (
    <div className="grid gap-3 border-t border-slate-100 bg-slate-50/50 p-4 md:grid-cols-2 lg:grid-cols-3">
      <label className="flex flex-col gap-1 text-xs font-medium text-slate-600">
        {fieldLabels.name}
        <input value={draft.name} onChange={(e) => onChange('name', e.target.value)} className="rounded border border-slate-300 px-2 py-1.5 text-sm" />
      </label>
      <label className="flex flex-col gap-1 text-xs font-medium text-slate-600">
        {fieldLabels.slug}
        <input value={draft.slug} onChange={(e) => onChange('slug', e.target.value)} className="rounded border border-slate-300 px-2 py-1.5 text-sm" />
      </label>
      <label className="flex flex-col gap-1 text-xs font-medium text-slate-600">
        {fieldLabels.displayOrder}
        <input
          type="number"
          value={draft.displayOrder}
          onChange={(e) => onChange('displayOrder', Number(e.target.value) || 0)}
          className="rounded border border-slate-300 px-2 py-1.5 text-sm"
        />
      </label>
      <label className="flex flex-col gap-1 text-xs font-medium text-slate-600">
        {fieldLabels.parentCategoryId}
        <select
          value={draft.parentCategoryId || ''}
          onChange={(e) => onChange('parentCategoryId', e.target.value || null)}
          className="rounded border border-slate-300 px-2 py-1.5 text-sm"
        >
          <option value="">(Không có - Nhóm gốc)</option>
          {parentOptions.map((p: ForumCategoryOption) => (
            <option key={p.id} value={p.id}>{p.name}</option>
          ))}
        </select>
      </label>
      <label className="col-span-full flex flex-col gap-1 text-xs font-medium text-slate-600 md:col-span-2">
        {fieldLabels.description}
        <textarea
          value={draft.description ?? ''}
          onChange={(e) => onChange('description', e.target.value || '')}
          rows={2}
          className="rounded border border-slate-300 px-2 py-1.5 text-sm"
        />
      </label>
      <label className="flex items-center gap-2 text-sm text-slate-700">
        <input type="checkbox" checked={draft.isActive} onChange={(e) => onChange('isActive', e.target.checked)} />
        {fieldLabels.isActive}
      </label>
    </div>
  )
}

export function AdminCategoriesPage() {
  const { t, categories, isLoading, isError, submitCreate, submitUpdate, submitDelete, isCreating, isUpdating, isDeleting } =
    useAdminCategoriesPage()
  const [overrides, setOverrides] = useState<Record<string, DraftOverrides>>({})
  const [createDraft, setCreateDraft] = useState<UpsertCategoryRequest>(emptyCreate)
  const [feedback, setFeedback] = useState<string | null>(null)
  const [confirm, setConfirm] = useState<ConfirmState>({ mode: 'idle' })
  const [editingId, setEditingId] = useState<string | 'create' | null>(null)

  const fieldLabels = useMemo(
    () => ({
      name: t('admin.categoriesPage.fields.name', 'Tên danh mục'),
      slug: t('admin.categoriesPage.fields.slug', 'Slug (Đường dẫn)'),
      description: t('admin.categoriesPage.fields.description', 'Mô tả'),
      displayOrder: t('admin.categoriesPage.fields.displayOrder', 'Thứ tự hiển thị'),
      isActive: t('admin.categoriesPage.fields.isActive', 'Kích hoạt'),
      parentCategoryId: t('admin.categoriesPage.fields.parentCategoryId', 'Nhóm / Khu vực'),
    }),
    [t],
  )

  const orderedCategories = useMemo(() => {
    const parents = categories.filter((c: ForumCategoryOption) => !c.parentCategoryId).sort((a: ForumCategoryOption, b: ForumCategoryOption) => a.displayOrder - b.displayOrder || a.name.localeCompare(b.name, 'vi'))
    
    const result: { category: ForumCategoryOption; isChild: boolean; parentName?: string }[] = []
    
    parents.forEach(parent => {
      result.push({ category: parent, isChild: false })
      const children = categories.filter(c => c.parentCategoryId === parent.id).sort((a,b) => a.displayOrder - b.displayOrder || a.name.localeCompare(b.name, 'vi'))
      children.forEach(child => {
        result.push({ category: child, isChild: true, parentName: parent.name })
      })
    })

    // Orphans
    const assignedIds = new Set(result.map(r => r.category.id))
    const orphans = categories.filter(c => !assignedIds.has(c.id)).sort((a,b) => a.displayOrder - b.displayOrder || a.name.localeCompare(b.name, 'vi'))
    orphans.forEach(orphan => {
      result.push({ category: orphan, isChild: false })
    })

    return result
  }, [categories])

  const getMerged = useCallback(
    (category: ForumCategoryOption): UpsertCategoryRequest => ({
      ...toUpsert(category),
      ...(overrides[category.id] ?? {}),
    }),
    [overrides],
  )

  const isCategoryDirty = (category: ForumCategoryOption) => {
    const merged = getMerged(category)
    return buildDiff(toUpsert(category), merged, fieldLabels).length > 0
  }

  const openUpdateConfirm = (category: ForumCategoryOption) => {
    const before = toUpsert(category)
    const after = normalizeUpsert(getMerged(category))
    const diffs = buildDiff(before, after, fieldLabels)
    if (diffs.length === 0) {
      setFeedback(t('admin.categoriesPage.messages.noChanges', 'Chưa có thay đổi nào được thực hiện.'))
      return
    }
    if (!after.slug.trim() || !after.name.trim()) {
      setFeedback(t('admin.categoriesPage.messages.slugNameRequired', 'Tên và Slug không được để trống.'))
      return
    }
    setConfirm({ mode: 'update', id: category.id, name: category.name, before, after, diffs })
  }

  const openCreateConfirm = () => {
    const after = normalizeUpsert(createDraft)
    if (!after.slug.trim() || !after.name.trim()) {
      setFeedback(t('admin.categoriesPage.messages.slugNameRequired', 'Tên và Slug không được để trống.'))
      return
    }
    setConfirm({ mode: 'create', after, diffs: buildCreatePreview(after, fieldLabels) })
  }

  const openDeleteConfirm = (category: ForumCategoryOption) => {
    setConfirm({ mode: 'delete', id: category.id, name: category.name })
  }

  const runConfirmed = async () => {
    if (confirm.mode === 'idle') return
    setFeedback(null)
    try {
      if (confirm.mode === 'update') {
        await submitUpdate(confirm.id, confirm.after)
        setOverrides((prev) => ({ ...prev, [confirm.id]: {} }))
        setEditingId(null)
        setFeedback(t('admin.categoriesPage.messages.saved', { name: confirm.after.name, defaultValue: `Đã lưu danh mục ${confirm.after.name}` }))
      } else if (confirm.mode === 'create') {
        await submitCreate(confirm.after)
        setCreateDraft(emptyCreate)
        setEditingId(null)
        setFeedback(t('admin.categoriesPage.messages.created', 'Đã tạo danh mục thành công.'))
      } else if (confirm.mode === 'delete') {
        await submitDelete(confirm.id)
        setEditingId(null)
        setFeedback(t('admin.categoriesPage.messages.deleted', 'Đã xóa danh mục thành công.'))
      }
      setConfirm({ mode: 'idle' })
    } catch {
      setFeedback(t('admin.categoriesPage.messages.saveFailed', 'Thao tác thất bại.'))
    }
  }

  const setOverride = <K extends keyof UpsertCategoryRequest>(
    id: string,
    key: K,
    value: UpsertCategoryRequest[K],
  ) => {
    setOverrides((prev) => ({ ...prev, [id]: { ...(prev[id] ?? {}), [key]: value } }))
  }

  if (isLoading) {
    return <div className="rounded-xl border border-slate-200 bg-white p-6 text-sm text-slate-600">{t('common.loading', 'Đang tải...')}</div>
  }

  if (isError) {
    return (
      <div className="rounded-xl border border-rose-200 bg-rose-50 p-6 text-sm text-rose-700">
        {t('admin.categoriesPage.messages.loadError', 'Không thể tải danh sách danh mục.')}
      </div>
    )
  }

  return (
    <div className="space-y-4">
      <header className="flex flex-wrap items-end justify-between gap-3 rounded-xl border border-slate-200 bg-white px-4 py-3">
        <div>
          <h1 className="text-lg font-semibold text-slate-900">{t('admin.categoriesPage.title', 'Quản lý Danh mục')}</h1>
          <p className="mt-0.5 text-sm text-slate-600">{t('admin.categoriesPage.subtitle', 'Thêm, sửa, xóa và sắp xếp các danh mục trên Sidebar.')}</p>
        </div>
        <button
          type="button"
          onClick={() => setEditingId((c) => (c === 'create' ? null : 'create'))}
          className="inline-flex cursor-pointer items-center gap-1.5 rounded-md border border-slate-300 px-3 py-2 text-sm font-medium text-slate-800 transition-colors hover:bg-slate-50"
        >
          <Plus className="h-4 w-4" aria-hidden />
          {t('admin.categoriesPage.create.title', 'Thêm Danh mục')}
        </button>
      </header>

      {feedback ? (
        <p className="rounded-lg border border-slate-200 bg-slate-50 px-3 py-2 text-sm text-slate-800">{feedback}</p>
      ) : null}

      <section className="overflow-x-auto rounded-xl border border-slate-200 bg-white">
        <table className="min-w-full text-sm">
          <thead className="bg-slate-50 text-left text-slate-600">
            <tr className="text-xs font-semibold uppercase tracking-wide text-slate-500">
              <th className="border-b border-slate-200 px-4 py-3 align-bottom">
                {t('admin.categoriesPage.fields.displayOrder', 'Thứ tự')}
              </th>
              <th className="border-b border-slate-200 px-4 py-3 align-bottom">
                {t('admin.categoriesPage.fields.name', 'Tên danh mục')}
              </th>
              <th className="border-b border-slate-200 px-4 py-3 align-bottom">
                {t('admin.categoriesPage.fields.slug', 'Slug')}
              </th>
              <th className="border-b border-slate-200 px-4 py-3 align-bottom">
                {t('admin.categoriesPage.fields.parentCategoryId', 'Khu vực / Nhóm')}
              </th>
              <th className="border-b border-slate-200 px-4 py-3 align-bottom">
                Số bài
              </th>
              <th className="border-b border-slate-200 px-4 py-3 text-right align-bottom">
                {t('admin.usersPage.table.actions', 'Thao tác')}
              </th>
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-100">
            {orderedCategories.map(({ category, isChild, parentName }) => {
              const draft = getMerged(category)
              const dirty = isCategoryDirty(category)
              const isOpen = editingId === category.id
              // valid parents for this category are all categories EXCEPT itself
              const parentOptions = categories.filter((c: ForumCategoryOption) => c.id !== category.id && !c.parentCategoryId)

              return (
                <Fragment key={category.id}>
                  <tr className={dirty ? 'bg-amber-50/50' : undefined}>
                    <td className="px-4 py-3 tabular-nums text-slate-700">{draft.displayOrder}</td>
                    <td className={`px-4 py-3 font-medium text-slate-900 ${isChild ? 'pl-8' : ''}`}>
                      {isChild ? (
                         <div className="flex items-start gap-2">
                           <span className="text-slate-300 mt-0.5">↳</span>
                           <div>
                             {draft.name}
                             <p className="mt-0.5 font-mono text-[11px] font-normal text-slate-400">{category.id}</p>
                           </div>
                         </div>
                      ) : (
                         <div>
                           {draft.name}
                           <p className="mt-0.5 font-mono text-[11px] font-normal text-slate-400">{category.id}</p>
                         </div>
                      )}
                    </td>
                    <td className="px-4 py-3 font-mono text-xs text-slate-600">{draft.slug}</td>
                    <td className="px-4 py-3 text-xs text-slate-600">
                      {isChild ? (
                        <span className="inline-flex items-center gap-1 rounded-md bg-slate-100 px-2 py-1 font-medium text-slate-700">
                          {parentName}
                        </span>
                      ) : (
                         <span className="text-slate-400">—</span>
                      )}
                      {draft.parentCategoryId && !parentName ? (
                         <span className="mt-1 block font-mono text-[10px] text-rose-500" title="Danh mục cha không hợp lệ">{draft.parentCategoryId}</span>
                      ) : null}
                    </td>
                    <td className="px-4 py-3 text-slate-600">{category.postCount}</td>
                    <td className="px-4 py-3 text-right">
                      <button
                        type="button"
                        onClick={() => setEditingId((c) => (c === category.id ? null : category.id))}
                        className="inline-flex cursor-pointer items-center gap-1 rounded-md border border-slate-300 px-2.5 py-1 text-xs font-medium text-slate-700 hover:bg-slate-50"
                      >
                        <Pencil className="h-3.5 w-3.5" aria-hidden />
                        {isOpen ? t('common.close', 'Đóng') : t('admin.categoriesPage.actions.edit', 'Sửa')}
                      </button>
                      <button
                        type="button"
                        onClick={() => openDeleteConfirm(category)}
                        className="ml-2 inline-flex cursor-pointer items-center gap-1 rounded-md border border-rose-200 bg-rose-50 px-2.5 py-1 text-xs font-medium text-rose-700 hover:bg-rose-100"
                      >
                        <Trash2 className="h-3.5 w-3.5" aria-hidden />
                        Xóa
                      </button>
                      {dirty ? (
                        <button
                          type="button"
                          disabled={isUpdating}
                          onClick={() => openUpdateConfirm(category)}
                          className="ml-2 cursor-pointer rounded-md bg-slate-900 px-2.5 py-1 text-xs font-medium text-white hover:bg-slate-800 disabled:opacity-50"
                        >
                          {t('admin.categoriesPage.actions.reviewSave', 'Lưu thay đổi')}
                        </button>
                      ) : null}
                    </td>
                  </tr>
                  {isOpen ? (
                    <tr>
                      <td colSpan={6} className="p-0">
                        <CategoryEditor
                          draft={draft}
                          fieldLabels={fieldLabels}
                          parentOptions={parentOptions}
                          onChange={(key, value) => setOverride(category.id, key, value)}
                        />
                      </td>
                    </tr>
                  ) : null}
                </Fragment>
              )
            })}
          </tbody>
        </table>
      </section>

      {editingId === 'create' ? (
        <section className="rounded-xl border border-slate-200 bg-white">
          <div className="border-b border-slate-100 px-4 py-3">
            <h2 className="text-sm font-semibold text-slate-900">{t('admin.categoriesPage.create.title', 'Thêm Danh mục mới')}</h2>
          </div>
          <CategoryEditor
            draft={createDraft}
            fieldLabels={fieldLabels}
            parentOptions={categories.filter((c: ForumCategoryOption) => !c.parentCategoryId)}
            onChange={(key, value) => setCreateDraft((p) => ({ ...p, [key]: value }))}
          />
          <div className="flex justify-end border-t border-slate-100 px-4 py-3">
            <button
              type="button"
              disabled={isCreating}
              onClick={() => openCreateConfirm()}
              className="cursor-pointer rounded-md bg-slate-900 px-4 py-2 text-sm font-medium text-white hover:bg-slate-800 disabled:opacity-50"
            >
              {t('admin.categoriesPage.create.review', 'Lưu danh mục')}
            </button>
          </div>
        </section>
      ) : null}

      {confirm.mode !== 'idle' ? (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4" role="dialog" aria-modal="true">
          <div className="max-h-[90vh] w-full max-w-lg overflow-y-auto rounded-xl border border-slate-200 bg-white p-5 shadow-xl">
            <h2 className="text-lg font-semibold text-slate-900">
              {confirm.mode === 'delete' ? 'Xác nhận xóa danh mục' : 
               confirm.mode === 'update'
                ? t('admin.categoriesPage.confirm.updateTitle', { name: confirm.name, defaultValue: `Cập nhật: ${confirm.name}` })
                : t('admin.categoriesPage.confirm.createTitle', 'Thêm Danh mục mới')}
            </h2>
            <p className="mt-1 text-sm text-slate-600">
              {confirm.mode === 'delete' ? 'Bạn có chắc chắn muốn xóa danh mục này? Hành động này không thể hoàn tác và có thể ảnh hưởng đến các bài viết thuộc danh mục này.' : 
              t('admin.categoriesPage.confirm.subtitle', 'Vui lòng kiểm tra lại thông tin thay đổi.')}
            </p>
            
            {confirm.mode !== 'delete' && (
              <div className="mt-4 overflow-hidden rounded-lg border border-slate-200">
                <table className="w-full text-left text-sm">
                  <thead className="bg-slate-50 text-xs font-semibold uppercase text-slate-500">
                    <tr>
                      <th className="px-3 py-2">{t('admin.categoriesPage.confirm.field', 'Trường')}</th>
                      <th className="px-3 py-2">{t('admin.categoriesPage.confirm.before', 'Trước')}</th>
                      <th className="px-3 py-2">{t('admin.categoriesPage.confirm.after', 'Sau')}</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-slate-100">
                    {confirm.diffs.map((row) => (
                      <tr key={row.field}>
                        <td className="px-3 py-2 font-medium text-slate-800">{row.field}</td>
                        <td className="px-3 py-2 text-slate-500">{row.before || '—'}</td>
                        <td className="px-3 py-2 font-medium text-slate-900">{row.after || '—'}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
            
            <div className="mt-5 flex justify-end gap-2">
              <button
                type="button"
                onClick={() => setConfirm({ mode: 'idle' })}
                className="cursor-pointer rounded-lg border border-slate-300 px-4 py-2 text-sm font-medium text-slate-700 hover:bg-slate-50"
              >
                {t('common.cancel', 'Hủy')}
              </button>
              <button
                type="button"
                disabled={isCreating || isUpdating || isDeleting}
                onClick={() => void runConfirmed()}
                className={`cursor-pointer rounded-lg px-4 py-2 text-sm font-medium text-white disabled:opacity-50 ${confirm.mode === 'delete' ? 'bg-rose-600 hover:bg-rose-700' : 'bg-slate-900 hover:bg-slate-800'}`}
              >
                {confirm.mode === 'delete' ? 'Xác nhận xóa' : t('admin.categoriesPage.confirm.apply', 'Áp dụng')}
              </button>
            </div>
          </div>
        </div>
      ) : null}
    </div>
  )
}
