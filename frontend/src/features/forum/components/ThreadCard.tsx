import { Link } from 'react-router-dom';
import { MessageSquare, Eye, ThumbsUp, Bookmark, Pin, Lock, CheckCircle2 } from 'lucide-react';
import type { ForumListItem } from '../types/forum-list';

type ThreadCardProps = {
  thread: ForumListItem;
};

export function ThreadCard({ thread }: ThreadCardProps) {
  // Format the last activity date
  let timeAgo = 'Just now';
  try {
    if (thread.activityAt) {
      const ms = Date.now() - new Date(thread.activityAt).getTime();
      const mins = Math.floor(ms / 60000);
      const hrs = Math.floor(mins / 60);
      const days = Math.floor(hrs / 24);
      if (days > 0) timeAgo = `${days}d ago`;
      else if (hrs > 0) timeAgo = `${hrs}h ago`;
      else if (mins > 0) timeAgo = `${mins}m ago`;
    }
  } catch (e) {
    console.error('Invalid date format', thread.activityAt);
  }

  return (
    <div className="w-full rounded-[12px] bg-white p-4 shadow-sm border border-slate-200 transition-all duration-200 ease-out hover:shadow-md">
      <div className="flex flex-col gap-3">
        {/* Title and Status Indicators */}
        <div className="flex items-start justify-between gap-4">
          <Link to={`/threads/${thread.id}`}>
            <h3 className="text-xl font-bold text-slate-900 hover:text-primary transition-colors">
              {thread.title}
            </h3>
          </Link>
          
          <div className="flex items-center gap-2 shrink-0">
            {thread.isPinned && (
              <span className="flex items-center text-orange-500 bg-orange-50 px-2 py-1 rounded text-xs font-medium" title="Pinned">
                <Pin className="w-3 h-3 mr-1" />
                Pinned
              </span>
            )}
            {thread.isLocked && (
              <span className="flex items-center text-red-500 bg-red-50 px-2 py-1 rounded text-xs font-medium" title="Locked">
                <Lock className="w-3 h-3 mr-1" />
                Locked
              </span>
            )}
            {thread.isSolved && (
              <span className="flex items-center text-green-600 bg-green-50 px-2 py-1 rounded text-xs font-medium" title="Solved">
                <CheckCircle2 className="w-3 h-3 mr-1" />
                Solved
              </span>
            )}
          </div>
        </div>

        {/* Content */}
        {thread.content ? (
          <div 
            className="prose prose-slate max-w-none text-slate-700 mt-2"
            dangerouslySetInnerHTML={{ __html: thread.content }}
          />
        ) : thread.preview && (
          <p className="text-slate-600 text-sm mt-2">
            {thread.preview}
          </p>
        )}

        {/* Meta: Author, Category, Time */}
        <div className="flex items-center gap-2 text-xs text-slate-500 flex-wrap">
          {thread.authorAvatar ? (
            <img src={thread.authorAvatar} alt={thread.authorName || 'Author'} className="w-5 h-5 rounded-full object-cover" />
          ) : (
            <div className="w-5 h-5 rounded-full bg-slate-200 flex items-center justify-center text-slate-600 font-medium">
              {(thread.authorName || 'U').charAt(0).toUpperCase()}
            </div>
          )}
          <span className="font-medium text-slate-700">{thread.authorName || 'Unknown User'}</span>
          <span>•</span>
          <span className="font-medium text-primary bg-primary/5 px-2 py-0.5 rounded">
            {thread.category}
          </span>
          <span>•</span>
          <span>{timeAgo}</span>
        </div>

        {/* Engagement Metrics and Tags */}
        <div className="flex items-center justify-between mt-1 border-t border-slate-100 pt-3 flex-wrap gap-3">
          <div className="flex items-center gap-4 text-slate-500 text-sm">
            <div className="flex items-center gap-1.5" title="Replies">
              <MessageSquare className="w-4 h-4" />
              <span>{thread.replyCount}</span>
            </div>
            <div className="flex items-center gap-1.5" title="Views">
              <Eye className="w-4 h-4" />
              <span>{thread.viewCount || 0}</span>
            </div>
            <div className="flex items-center gap-1.5" title="Likes">
              <ThumbsUp className="w-4 h-4" />
              <span>{thread.likeCount || 0}</span>
            </div>
            <div className="flex items-center gap-1.5" title="Bookmarks">
              <Bookmark className="w-4 h-4" />
              <span>{thread.bookmarkCount || 0}</span>
            </div>
          </div>

          <div className="flex items-center gap-2 flex-wrap">
            {thread.tags?.map((tag) => (
              <span key={tag} className="text-xs font-medium text-slate-600 bg-slate-100 px-2.5 py-1 rounded-full">
                #{tag}
              </span>
            ))}
          </div>
        </div>
      </div>
    </div>
  );
}
