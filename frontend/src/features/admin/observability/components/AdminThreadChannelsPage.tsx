import { Fragment, useCallback, useMemo, useState } from 'react'
import { Pencil, Plus } from 'lucide-react'
import { useAdminThreadChannelsPage } from '../hooks/useAdminThreadChannelsPage'
import type { ThreadChannelDto, UpsertThreadChannelRequest } from '../../types/admin.types'

type DraftOverrides = Partial<UpsertThreadChannelRequest>

const emptyCreate: UpsertThreadChannelRequest = {
  code: '',
  name: '',
  description: null,
  displayOrder: 0,
  isActive: true,
  allowPinnedComments: true,
  allowAcceptedAnswers: true,
  allowModeratorActions: true,
}

function toUpsert(channel: ThreadChannelDto): UpsertThreadChannelRequest {
  return {
    code: channel.code,
    name: channel.name,
    description: channel.description || null,
    displayOrder: channel.displayOrder,
    isActive: channel.isActive,
    allowPinnedComments: channel.allowPinnedComments,
    allowAcceptedAnswers: channel.allowAcceptedAnswers,
    allowModeratorActions: channel.allowModeratorActions,
  }
}

type DiffRow = { field: string; before: string; after: string }

function buildDiff(before: UpsertThreadChannelRequest, after: UpsertThreadChannelRequest, labels: Record<string, string>): DiffRow[] {
  const keys = Object.keys(before) as (keyof UpsertThreadChannelRequest)[]
  const rows: DiffRow[] = []
  for (const key of keys) {
    const b = before[key]
    const a = after[key]
    const bv = typeof b === 'boolean' ? (b ? 'true' : 'false') : String(b ?? '')
    const av = typeof a === 'boolean' ? (a ? 'true' : 'false') : String(a ?? '')
    if (bv !== av) rows.push({ field: labels[key] ?? String(key), before: bv, after: av })
  }
  return rows
}

function formatUpsertValue(v: unknown): string {
  if (typeof v === 'boolean') return v ? 'true' : 'false'
  if (v === null || v === undefined) return ''
  return String(v)
}

function buildCreatePreview(after: UpsertThreadChannelRequest, labels: Record<string, string>): DiffRow[] {
  const keys = Object.keys(after) as (keyof UpsertThreadChannelRequest)[]
  return keys.map((key) => ({
    field: labels[key] ?? String(key),
    before: '—',
    after: formatUpsertValue(after[key]) || '—',
  }))
}

function normalizeUpsert(u: UpsertThreadChannelRequest): UpsertThreadChannelRequest {
  return {
    ...u,
    code: u.code.trim(),
    name: u.name.trim(),
    description: u.description?.trim() || null,
  }
}

type ConfirmState =
  | { mode: 'idle' }
  | { mode: 'update'; id: string; name: string; before: UpsertThreadChannelRequest; after: UpsertThreadChannelRequest; diffs: DiffRow[] }
  | { mode: 'create'; after: UpsertThreadChannelRequest; diffs: DiffRow[] }

function PolicyStatusBadge({
  on,
  onLabel,
  offLabel,
}: {
  on: boolean
  onLabel: string
  offLabel: string
}) {
  return (
    <span
      className={`inline-flex min-w-[3.25rem] justify-center rounded-full px-2 py-0.5 text-[11px] font-semibold ${
        on ? 'bg-emerald-100 text-emerald-800' : 'bg-slate-100 text-slate-500'
      }`}
    >
      {on ? onLabel : offLabel}
    </span>
  )
}

function ChannelEditor({
  draft,
  fieldLabels,
  onChange,
}: {
  draft: UpsertThreadChannelRequest
  fieldLabels: Record<string, string>
  onChange: <K extends keyof UpsertThreadChannelRequest>(key: K, value: UpsertThreadChannelRequest[K]) => void
}) {
  return (
    <div className="grid gap-3 border-t border-slate-100 bg-slate-50/50 p-4 md:grid-cols-2 lg:grid-cols-3">
      <label className="flex flex-col gap-1 text-xs font-medium text-slate-600">
        {fieldLabels.code}
        <input value={draft.code} onChange={(e) => onChange('code', e.target.value)} className="rounded border border-slate-300 px-2 py-1.5 text-sm" />
      </label>
      <label className="flex flex-col gap-1 text-xs font-medium text-slate-600">
        {fieldLabels.name}
        <input value={draft.name} onChange={(e) => onChange('name', e.target.value)} className="rounded border border-slate-300 px-2 py-1.5 text-sm" />
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
      <label className="col-span-full flex flex-col gap-1 text-xs font-medium text-slate-600 md:col-span-2">
        {fieldLabels.description}
        <textarea
          value={draft.description ?? ''}
          onChange={(e) => onChange('description', e.target.value || null)}
          rows={2}
          className="rounded border border-slate-300 px-2 py-1.5 text-sm"
        />
      </label>
      <label className="flex items-center gap-2 text-sm text-slate-700">
        <input type="checkbox" checked={draft.isActive} onChange={(e) => onChange('isActive', e.target.checked)} />
        {fieldLabels.isActive}
      </label>
      <label className="flex items-center gap-2 text-sm text-slate-700">
        <input type="checkbox" checked={draft.allowPinnedComments} onChange={(e) => onChange('allowPinnedComments', e.target.checked)} />
        {fieldLabels.allowPinnedComments}
      </label>
      <label className="flex items-center gap-2 text-sm text-slate-700">
        <input type="checkbox" checked={draft.allowAcceptedAnswers} onChange={(e) => onChange('allowAcceptedAnswers', e.target.checked)} />
        {fieldLabels.allowAcceptedAnswers}
      </label>
      <label className="flex items-center gap-2 text-sm text-slate-700">
        <input type="checkbox" checked={draft.allowModeratorActions} onChange={(e) => onChange('allowModeratorActions', e.target.checked)} />
        {fieldLabels.allowModeratorActions}
      </label>
    </div>
  )
}

export function AdminThreadChannelsPage() {
  const { t, channels, isLoading, isError, submitCreate, submitUpdate, isCreating, isUpdating } =
    useAdminThreadChannelsPage()
  const [overrides, setOverrides] = useState<Record<string, DraftOverrides>>({})
  const [createDraft, setCreateDraft] = useState<UpsertThreadChannelRequest>(emptyCreate)
  const [feedback, setFeedback] = useState<string | null>(null)
  const [confirm, setConfirm] = useState<ConfirmState>({ mode: 'idle' })
  const [editingId, setEditingId] = useState<string | 'create' | null>(null)

  const fieldLabels = useMemo(
    () => ({
      code: t('admin.threadChannelsPage.fields.code'),
      name: t('admin.threadChannelsPage.fields.name'),
      description: t('admin.threadChannelsPage.fields.description'),
      displayOrder: t('admin.threadChannelsPage.fields.displayOrder'),
      isActive: t('admin.threadChannelsPage.fields.isActive'),
      allowPinnedComments: t('admin.threadChannelsPage.fields.allowPin'),
      allowAcceptedAnswers: t('admin.threadChannelsPage.fields.allowAccepted'),
      allowModeratorActions: t('admin.threadChannelsPage.fields.allowMod'),
    }),
    [t],
  )

  const getMerged = useCallback(
    (channel: ThreadChannelDto): UpsertThreadChannelRequest => ({
      ...toUpsert(channel),
      ...(overrides[channel.id] ?? {}),
    }),
    [overrides],
  )

  const isChannelDirty = (channel: ThreadChannelDto) => {
    const merged = getMerged(channel)
    return buildDiff(toUpsert(channel), merged, fieldLabels).length > 0
  }

  const openUpdateConfirm = (channel: ThreadChannelDto) => {
    const before = toUpsert(channel)
    const after = normalizeUpsert(getMerged(channel))
    const diffs = buildDiff(before, after, fieldLabels)
    if (diffs.length === 0) {
      setFeedback(t('admin.threadChannelsPage.messages.noChanges'))
      return
    }
    if (!after.code.trim() || !after.name.trim()) {
      setFeedback(t('admin.threadChannelsPage.messages.codeNameRequired'))
      return
    }
    setConfirm({ mode: 'update', id: channel.id, name: channel.name, before, after, diffs })
  }

  const openCreateConfirm = () => {
    const after = normalizeUpsert(createDraft)
    if (!after.code.trim() || !after.name.trim()) {
      setFeedback(t('admin.threadChannelsPage.messages.codeNameRequired'))
      return
    }
    setConfirm({ mode: 'create', after, diffs: buildCreatePreview(after, fieldLabels) })
  }

  const runConfirmed = async () => {
    if (confirm.mode === 'idle') return
    setFeedback(null)
    try {
      if (confirm.mode === 'update') {
        await submitUpdate(confirm.id, confirm.after)
        setOverrides((prev) => ({ ...prev, [confirm.id]: {} }))
        setEditingId(null)
        setFeedback(t('admin.threadChannelsPage.messages.saved', { name: confirm.after.name }))
      } else {
        await submitCreate(confirm.after)
        setCreateDraft(emptyCreate)
        setEditingId(null)
        setFeedback(t('admin.threadChannelsPage.messages.created'))
      }
      setConfirm({ mode: 'idle' })
    } catch {
      setFeedback(t('admin.threadChannelsPage.messages.saveFailed'))
    }
  }

  const setOverride = <K extends keyof UpsertThreadChannelRequest>(
    id: string,
    key: K,
    value: UpsertThreadChannelRequest[K],
  ) => {
    setOverrides((prev) => ({ ...prev, [id]: { ...(prev[id] ?? {}), [key]: value } }))
  }

  if (isLoading) {
    return <div className="rounded-xl border border-slate-200 bg-white p-6 text-sm text-slate-600">{t('common.loading')}</div>
  }

  if (isError) {
    return (
      <div className="rounded-xl border border-rose-200 bg-rose-50 p-6 text-sm text-rose-700">
        {t('admin.threadChannelsPage.messages.loadError')}
      </div>
    )
  }

  return (
    <div className="space-y-4">
      <header className="flex flex-wrap items-end justify-between gap-3 rounded-xl border border-slate-200 bg-white px-4 py-3">
        <div>
          <h1 className="text-lg font-semibold text-slate-900">{t('admin.threadChannelsPage.title')}</h1>
          <p className="mt-0.5 text-sm text-slate-600">{t('admin.threadChannelsPage.subtitle')}</p>
        </div>
        <button
          type="button"
          onClick={() => setEditingId((c) => (c === 'create' ? null : 'create'))}
          className="inline-flex cursor-pointer items-center gap-1.5 rounded-md border border-slate-300 px-3 py-2 text-sm font-medium text-slate-800 transition-colors hover:bg-slate-50"
        >
          <Plus className="h-4 w-4" aria-hidden />
          {t('admin.threadChannelsPage.create.title')}
        </button>
      </header>

      {feedback ? (
        <p className="rounded-lg border border-slate-200 bg-slate-50 px-3 py-2 text-sm text-slate-800">{feedback}</p>
      ) : null}

      <p className="text-xs text-slate-500">{t('admin.threadChannelsPage.table.policyLegend')}</p>

      <section className="overflow-x-auto rounded-xl border border-slate-200 bg-white">
        <table className="min-w-full text-sm">
          <thead className="bg-slate-50 text-left text-slate-600">
            <tr className="text-xs font-semibold uppercase tracking-wide text-slate-500">
              <th rowSpan={2} className="border-b border-slate-200 px-4 py-3 align-bottom">
                {t('admin.threadChannelsPage.fields.displayOrder')}
              </th>
              <th rowSpan={2} className="border-b border-slate-200 px-4 py-3 align-bottom">
                {t('admin.threadChannelsPage.fields.code')}
              </th>
              <th rowSpan={2} className="border-b border-slate-200 px-4 py-3 align-bottom">
                {t('admin.threadChannelsPage.fields.name')}
              </th>
              <th rowSpan={2} className="border-b border-slate-200 px-4 py-3 align-bottom">
                {t('admin.threadChannelsPage.fields.isActive')}
              </th>
              <th
                colSpan={3}
                className="border-b border-l border-slate-200 px-4 py-2 text-center normal-case tracking-normal"
              >
                {t('admin.threadChannelsPage.table.policiesGroup')}
              </th>
              <th rowSpan={2} className="border-b border-slate-200 px-4 py-3 text-right align-bottom">
                {t('admin.usersPage.table.actions')}
              </th>
            </tr>
            <tr className="border-b border-slate-200 text-[11px] font-semibold normal-case leading-snug text-slate-600">
              <th className="border-l border-slate-200 px-3 py-2 text-center">
                {t('admin.threadChannelsPage.policyLabels.pin')}
              </th>
              <th className="px-3 py-2 text-center">{t('admin.threadChannelsPage.policyLabels.accepted')}</th>
              <th className="px-3 py-2 text-center">{t('admin.threadChannelsPage.policyLabels.mod')}</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-100">
            {channels.map((channel) => {
              const draft = getMerged(channel)
              const dirty = isChannelDirty(channel)
              const isOpen = editingId === channel.id
              return (
                <Fragment key={channel.id}>
                  <tr className={dirty ? 'bg-amber-50/50' : undefined}>
                    <td className="px-4 py-3 tabular-nums text-slate-700">{draft.displayOrder}</td>
                    <td className="px-4 py-3 font-mono text-xs text-slate-600">{draft.code}</td>
                    <td className="px-4 py-3 font-medium text-slate-900">{draft.name}</td>
                    <td className="px-4 py-3">
                      <span
                        className={`inline-flex rounded px-1.5 py-0.5 text-xs font-medium ${
                          draft.isActive ? 'bg-emerald-100 text-emerald-800' : 'bg-slate-100 text-slate-600'
                        }`}
                      >
                        {draft.isActive
                          ? t('admin.threadChannelsPage.badges.active')
                          : t('admin.threadChannelsPage.badges.inactive')}
                      </span>
                    </td>
                    <td className="border-l border-slate-100 px-3 py-3 text-center">
                      <PolicyStatusBadge
                        on={draft.allowPinnedComments}
                        onLabel={t('admin.threadChannelsPage.policy.on')}
                        offLabel={t('admin.threadChannelsPage.policy.off')}
                      />
                    </td>
                    <td className="px-3 py-3 text-center">
                      <PolicyStatusBadge
                        on={draft.allowAcceptedAnswers}
                        onLabel={t('admin.threadChannelsPage.policy.on')}
                        offLabel={t('admin.threadChannelsPage.policy.off')}
                      />
                    </td>
                    <td className="px-3 py-3 text-center">
                      <PolicyStatusBadge
                        on={draft.allowModeratorActions}
                        onLabel={t('admin.threadChannelsPage.policy.on')}
                        offLabel={t('admin.threadChannelsPage.policy.off')}
                      />
                    </td>
                    <td className="px-4 py-3 text-right">
                      <button
                        type="button"
                        onClick={() => setEditingId((c) => (c === channel.id ? null : channel.id))}
                        className="inline-flex cursor-pointer items-center gap-1 rounded-md border border-slate-300 px-2.5 py-1 text-xs font-medium text-slate-700 hover:bg-slate-50"
                      >
                        <Pencil className="h-3.5 w-3.5" aria-hidden />
                        {isOpen ? t('common.close') : t('admin.threadChannelsPage.actions.edit')}
                      </button>
                      {dirty ? (
                        <button
                          type="button"
                          disabled={isUpdating}
                          onClick={() => openUpdateConfirm(channel)}
                          className="ml-2 cursor-pointer rounded-md bg-slate-900 px-2.5 py-1 text-xs font-medium text-white hover:bg-slate-800 disabled:opacity-50"
                        >
                          {t('admin.threadChannelsPage.actions.reviewSave')}
                        </button>
                      ) : null}
                    </td>
                  </tr>
                  {isOpen ? (
                    <tr>
                      <td colSpan={8} className="p-0">
                        <ChannelEditor
                          draft={draft}
                          fieldLabels={fieldLabels}
                          onChange={(key, value) => setOverride(channel.id, key, value)}
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
            <h2 className="text-sm font-semibold text-slate-900">{t('admin.threadChannelsPage.create.title')}</h2>
            <p className="text-xs text-slate-500">{t('admin.threadChannelsPage.create.hint')}</p>
          </div>
          <ChannelEditor
            draft={createDraft}
            fieldLabels={fieldLabels}
            onChange={(key, value) => setCreateDraft((p) => ({ ...p, [key]: value }))}
          />
          <div className="flex justify-end border-t border-slate-100 px-4 py-3">
            <button
              type="button"
              disabled={isCreating}
              onClick={() => openCreateConfirm()}
              className="cursor-pointer rounded-md bg-slate-900 px-4 py-2 text-sm font-medium text-white hover:bg-slate-800 disabled:opacity-50"
            >
              {t('admin.threadChannelsPage.create.review')}
            </button>
          </div>
        </section>
      ) : null}

      {confirm.mode !== 'idle' ? (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4" role="dialog" aria-modal="true">
          <div className="max-h-[90vh] w-full max-w-lg overflow-y-auto rounded-xl border border-slate-200 bg-white p-5 shadow-xl">
            <h2 className="text-lg font-semibold text-slate-900">
              {confirm.mode === 'update'
                ? t('admin.threadChannelsPage.confirm.updateTitle', { name: confirm.name })
                : t('admin.threadChannelsPage.confirm.createTitle')}
            </h2>
            <p className="mt-1 text-sm text-slate-600">{t('admin.threadChannelsPage.confirm.subtitle')}</p>
            <div className="mt-4 overflow-hidden rounded-lg border border-slate-200">
              <table className="w-full text-left text-sm">
                <thead className="bg-slate-50 text-xs font-semibold uppercase text-slate-500">
                  <tr>
                    <th className="px-3 py-2">{t('admin.threadChannelsPage.confirm.field')}</th>
                    <th className="px-3 py-2">{t('admin.threadChannelsPage.confirm.before')}</th>
                    <th className="px-3 py-2">{t('admin.threadChannelsPage.confirm.after')}</th>
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
            <div className="mt-5 flex justify-end gap-2">
              <button
                type="button"
                onClick={() => setConfirm({ mode: 'idle' })}
                className="cursor-pointer rounded-lg border border-slate-300 px-4 py-2 text-sm font-medium text-slate-700 hover:bg-slate-50"
              >
                {t('common.cancel')}
              </button>
              <button
                type="button"
                disabled={isCreating || isUpdating}
                onClick={() => void runConfirmed()}
                className="cursor-pointer rounded-lg bg-slate-900 px-4 py-2 text-sm font-medium text-white hover:bg-slate-800 disabled:opacity-50"
              >
                {t('admin.threadChannelsPage.confirm.apply')}
              </button>
            </div>
          </div>
        </div>
      ) : null}
    </div>
  )
}
