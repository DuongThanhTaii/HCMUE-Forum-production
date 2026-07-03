import { parseForumRichContent } from '../../../lib/parseForumRichContent'

interface ThreadContentProps {
  content: string
  t: (key: string) => string
}

export function ThreadContent({ content, t }: ThreadContentProps) {
  const parsedPost = parseForumRichContent(content)

  return (
    <div className="mt-8 prose prose-slate max-w-none prose-p:text-[16px] md:prose-p:text-[17px] prose-p:leading-[1.7] prose-headings:font-bold prose-a:text-primary prose-img:rounded-xl">
      {parsedPost.body ? (
        <div className="whitespace-pre-line text-slate-800">
          {parsedPost.body}
        </div>
      ) : null}

      {/* Attachments (Images) */}
      {parsedPost.imageUrls.length > 0 && (
        <div className="mt-8">
          <p className="mb-3 text-[13px] font-bold uppercase tracking-widest text-slate-400">
            {t('forum.detail.attachmentsLabel') || 'Attached Images'}
          </p>
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
            {parsedPost.imageUrls.map((url) => (
              <a key={url} href={url} target="_blank" rel="noopener noreferrer" className="block group">
                <img
                  src={url}
                  alt={t('forum.detail.attachmentImageAlt') || 'Attachment'}
                  loading="lazy"
                  className="max-h-96 w-full rounded-xl border border-slate-200 object-contain shadow-sm transition-all group-hover:border-slate-300 group-hover:shadow-md"
                />
              </a>
            ))}
          </div>
        </div>
      )}

      {/* Attachments (Files) */}
      {parsedPost.fileUrls.length > 0 && (
        <div className="mt-6">
          <p className="mb-2 text-[13px] font-bold uppercase tracking-widest text-slate-400">
            {t('forum.detail.attachmentsLabel') || 'Attached Files'}
          </p>
          <div className="space-y-2">
            {parsedPost.fileUrls.map((url) => (
              <a
                key={url}
                href={url}
                target="_blank"
                rel="noopener noreferrer"
                className="inline-flex items-center gap-2 rounded-lg border border-slate-200 bg-slate-50 px-4 py-2 text-[14px] font-medium text-slate-700 hover:border-slate-300 hover:bg-slate-100 hover:text-primary transition-colors break-all"
              >
                {url}
              </a>
            ))}
          </div>
        </div>
      )}
    </div>
  )
}
