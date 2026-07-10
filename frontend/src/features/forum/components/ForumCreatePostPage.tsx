import { Link } from 'react-router-dom'
import { PenSquare } from 'lucide-react'
import { featureFlags } from '@shared/config/featureFlags'
import { useForumCreatePostPage } from '../hooks/useForumCreatePostPage'

export function ForumCreatePostPage() {
  const {
    t,
    categories,
    loadingCategories,
    threadChannels,
    loadingThreadChannels,
    popularTags,
    loadingTags,
    title,
    setTitle,
    content,
    setContent,
    type,
    setType,
    categoryId,
    setCategoryId,
    threadChannelId,
    setThreadChannelId,
    selectedTagNames,
    customTags,
    addTagDraft,
    setAddTagDraft,
    toggleTag,
    onAddTagKeyDown,
    removeCustomTag,
    typeOptions,
    onSubmit,
    isSubmitting,
    errorMessage,
    attachments,
    setAttachments,
    isUploadingAttachments,
    copilotError,
    copilotSuggestion,
    copilotRewrite,
    onSuggestTitleTags,
    onRewriteContent,
    isSuggestingTitleTags,
    isRewritingContent,
  } = useForumCreatePostPage()

  return (
    <div className="mx-auto max-w-4xl">
      {/* Breadcrumb */}
      <nav className="flex items-center text-[13px] font-medium text-slate-500 mb-6" aria-label="Breadcrumb">
        <Link
          to="/explore"
          className="hover:text-primary transition-colors"
        >
          ← {t('forum.createPost.backToList')}
        </Link>
      </nav>

      <div className="bg-white rounded-xl border border-slate-200 shadow-sm mb-6">
        <div className="p-6 md:p-8 border-b border-slate-100">
          <h1 className="text-xl font-bold text-slate-900">{t('forum.createPost.title')}</h1>
          <p className="mt-1.5 text-sm text-slate-500">{t('forum.createPost.subtitle')}</p>
        </div>

        <form onSubmit={onSubmit} className="space-y-6 p-6 md:p-8">
          <label className="flex flex-col gap-2">
          <span className="text-xs font-medium uppercase tracking-wide text-slate-500">
            {t('forum.createPost.fields.title')}
          </span>
          <input
            type="text"
            value={title}
            onChange={(e) => setTitle(e.target.value)}
            className="rounded-md border border-slate-200 px-3 py-2 text-sm outline-none ring-primary focus:ring-2"
            maxLength={300}
            autoComplete="off"
          />
          {featureFlags.copilotActionsEnabled ? (
            <div className="mt-1.5 flex flex-wrap gap-2">
              <button
                type="button"
                onClick={() => void onSuggestTitleTags()}
                disabled={isSuggestingTitleTags}
                className="cursor-pointer rounded-md border border-indigo-200 px-2.5 py-1 text-[12px] font-medium text-indigo-700 hover:border-indigo-400 hover:text-indigo-800 disabled:opacity-60"
              >
                {isSuggestingTitleTags ? 'Suggesting...' : 'AI suggest title/tags'}
              </button>
              <button
                type="button"
                onClick={() => void onRewriteContent()}
                disabled={isRewritingContent}
                className="cursor-pointer rounded-md border border-emerald-200 px-2.5 py-1 text-[12px] font-medium text-emerald-700 hover:border-emerald-400 hover:text-emerald-800 disabled:opacity-60"
              >
                {isRewritingContent ? 'Rewriting...' : 'AI rewrite content'}
              </button>
            </div>
          ) : null}
        </label>

        <label className="flex flex-col gap-1.5">
          <span className="text-xs font-medium uppercase tracking-wide text-slate-500">
            {t('forum.createPost.fields.type')}
          </span>
          <select
            value={type}
            onChange={(e) => setType(Number(e.target.value))}
            className="rounded-md border border-slate-200 bg-white px-3 py-2 text-sm outline-none ring-primary focus:ring-2"
          >
            {typeOptions.map((opt) => (
              <option key={opt.value} value={opt.value}>
                {opt.label}
              </option>
            ))}
          </select>
        </label>

        <label className="flex flex-col gap-1.5">
          <span className="text-xs font-medium uppercase tracking-wide text-slate-500">
            {t('forum.createPost.fields.category')}
          </span>
          <select
            value={categoryId}
            onChange={(e) => setCategoryId(e.target.value)}
            disabled={loadingCategories}
            className="rounded-md border border-slate-200 bg-white px-3 py-2 text-sm outline-none ring-primary focus:ring-2 disabled:opacity-60"
          >
            <option value="">{t('forum.createPost.fields.categoryPlaceholder')}</option>
            {categories.map((c) => (
              <option key={c.id} value={c.id}>
                {c.name}
              </option>
            ))}
          </select>
          <p className="text-[11px] text-slate-500">Bắt buộc: Hãy chọn danh mục phù hợp nhất với nội dung bài viết.</p>
        </label>

        <label className="flex flex-col gap-1.5">
          <span className="text-xs font-medium uppercase tracking-wide text-slate-500">
            Kênh thảo luận (Thread Channel)
          </span>
          <select
            value={threadChannelId}
            onChange={(e) => setThreadChannelId(e.target.value)}
            disabled={loadingThreadChannels}
            className="rounded-md border border-slate-200 bg-white px-3 py-2 text-sm outline-none ring-primary focus:ring-2 disabled:opacity-60"
          >
            <option value="">Không thuộc kênh nào (Bài viết thường)</option>
            {threadChannels.map((channel) => (
              <option key={channel.id} value={channel.id}>
                {channel.name}
              </option>
            ))}
          </select>
          <p className="text-[11px] text-slate-500">Chọn một kênh nếu bài viết thuộc chủ đề chuyên sâu, hoặc để trống.</p>
        </label>

        <label className="flex flex-col gap-1.5">
          <span className="text-xs font-medium uppercase tracking-wide text-slate-500">
            {t('forum.createPost.fields.content')}
          </span>
          <textarea
            value={content}
            onChange={(e) => setContent(e.target.value)}
            rows={10}
            className="rounded-md border border-slate-200 px-3 py-2 text-sm leading-relaxed outline-none ring-primary focus:ring-2"
          />
          {featureFlags.copilotActionsEnabled && copilotSuggestion ? (
            <p className="text-[12px] text-indigo-700">
              AI rationale:
              {' '}
              {copilotSuggestion.rationale}
            </p>
          ) : null}
          {featureFlags.copilotActionsEnabled && copilotRewrite ? (
            <p className="text-[12px] text-emerald-700">
              Content rewritten in style:
              {' '}
              {copilotRewrite.style}
            </p>
          ) : null}
        </label>
        <label className="flex flex-col gap-1.5">
          <span className="text-xs font-medium uppercase tracking-wide text-slate-500">Tệp đính kèm (Attachments)</span>
          <input
            type="file"
            multiple
            onChange={(e) => setAttachments(Array.from(e.target.files ?? []))}
            className="rounded-md border border-slate-200 px-3 py-2 text-sm outline-none ring-primary file:mr-3 file:rounded file:border-0 file:bg-slate-100 file:px-2 file:py-1 file:text-xs file:font-medium focus:ring-2"
          />
          {attachments.length > 0 ? (
            <p className="text-[12px] text-slate-500">{attachments.length} file(s) selected</p>
          ) : null}
        </label>

        <div className="flex flex-col gap-2">
          <span className="text-xs font-medium uppercase tracking-wide text-slate-500">
            {t('forum.createPost.fields.tagPicker')}
          </span>
          {loadingTags ? (
            <p className="text-sm text-slate-500">{t('common.loading')}</p>
          ) : popularTags.length === 0 ? (
            <p className="text-[13px] text-slate-500">{t('forum.createPost.fields.noSuggestedTags')}</p>
          ) : (
            <div className="flex flex-wrap gap-1.5" role="group" aria-label={t('forum.createPost.fields.tagPicker')}>
              {popularTags.map((tag) => {
                const pressed = selectedTagNames.some(
                  (x) => x.toLowerCase() === tag.name.toLowerCase(),
                )
                return (
                  <button
                    key={tag.name}
                    type="button"
                    aria-pressed={pressed}
                    onClick={() => toggleTag(tag.name)}
                    className={`rounded-full border px-2.5 py-1 text-[12px] font-medium transition-colors ${
                      pressed
                        ? 'border-primary bg-primary/10 text-primary'
                        : 'border-slate-200 bg-white text-slate-700 hover:border-slate-300'
                    }`}
                  >
                    {tag.name}
                    {tag.postCount > 0 ? (
                      <span className="ml-1 tabular-nums text-slate-400">({tag.postCount})</span>
                    ) : null}
                  </button>
                )
              })}
            </div>
          )}
          {customTags.length > 0 ? (
            <div className="flex flex-wrap gap-1.5 pt-1">
              {customTags.map((tag) => (
                <span
                  key={tag}
                  className="inline-flex items-center gap-1 rounded-full border border-emerald-200 bg-emerald-50 px-2 py-0.5 text-[12px] text-emerald-900"
                >
                  {tag}
                  <button
                    type="button"
                    className="rounded p-0.5 hover:bg-emerald-100"
                    onClick={() => removeCustomTag(tag)}
                    aria-label={t('forum.createPost.fields.removeTag')}
                  >
                    ×
                  </button>
                </span>
              ))}
            </div>
          ) : null}
          <label className="flex flex-col gap-1">
            <span className="text-[11px] text-slate-500">{t('forum.createPost.fields.addCustomTags')}</span>
            <input
              type="text"
              value={addTagDraft}
              onChange={(e) => setAddTagDraft(e.target.value)}
              onKeyDown={onAddTagKeyDown}
              placeholder={t('forum.createPost.fields.addCustomTagsPlaceholder')}
              className="rounded-md border border-slate-200 px-3 py-2 text-sm outline-none ring-primary focus:ring-2"
            />
          </label>
        </div>

        <div className="space-y-1.5 rounded-md border border-slate-100 bg-slate-50 p-3 text-[12px] leading-relaxed text-slate-600">
          <p>{t('forum.createPost.noticeAttachments')}</p>
          <p>{t('forum.createPost.noticeMentions')}</p>
          <p>{t('forum.createPost.noticeChat')}</p>
        </div>

        {errorMessage ? (
          <p className="text-sm text-rose-600" role="alert">
            {errorMessage}
          </p>
        ) : null}
        {featureFlags.copilotActionsEnabled && copilotError ? (
          <p className="text-sm text-rose-600" role="alert">
            {copilotError}
          </p>
        ) : null}

        <p className="text-[13px] leading-relaxed text-slate-500">{t('forum.createPost.pendingNote')}</p>

        <div className="flex flex-wrap items-center gap-2 pt-2">
          <button
            type="submit"
            disabled={isSubmitting || loadingCategories || loadingThreadChannels}
            className="inline-flex items-center gap-2 rounded-lg border border-primary bg-primary px-4 py-2.5 text-sm font-semibold text-white shadow-sm transition-colors hover:bg-primary/90 disabled:cursor-not-allowed disabled:opacity-60"
          >
            <PenSquare className="h-4 w-4" aria-hidden />
            {isSubmitting || isUploadingAttachments ? t('common.loading') : t('forum.createPost.submit')}
          </button>
          <Link
            to="/explore"
            className="rounded-lg border border-slate-300 bg-white px-4 py-2.5 text-sm font-semibold text-slate-700 shadow-sm hover:bg-slate-50 transition-colors"
          >
            {t('common.cancel')}
          </Link>
        </div>
        </form>
      </div>
    </div>
  )
}
