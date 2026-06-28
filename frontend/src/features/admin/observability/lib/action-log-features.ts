/** Path prefixes for Mongo `pathContains` (case-insensitive regex on server). */

export type ActionLogFeatureCluster = {
  id: string
  /** When omitted, does not narrow by path. */
  pathContains?: string
}

export const ACTION_LOG_FEATURE_CLUSTERS: ActionLogFeatureCluster[] = [
  { id: 'all' },
  { id: 'posts', pathContains: '/api/v1/posts' },
  { id: 'comments', pathContains: '/api/v1/comments' },
  { id: 'categories', pathContains: '/api/v1/categories' },
  { id: 'tags', pathContains: '/api/v1/tags' },
  { id: 'search', pathContains: '/api/v1/search' },
  { id: 'threadChannels', pathContains: '/api/v1/thread-channels' },
  { id: 'mod', pathContains: '/api/v1/mod' },
  { id: 'adminAuth', pathContains: '/api/v1/admin/authorization' },
  { id: 'adminObservability', pathContains: '/api/v1/admin/observability' },
  { id: 'users', pathContains: '/api/v1/users' },
  { id: 'auth', pathContains: '/api/v1/auth' },
  { id: 'roles', pathContains: '/api/v1/roles' },
  { id: 'permissions', pathContains: '/api/v1/permissions' },
  { id: 'jobs', pathContains: '/api/v1/jobs' },
  { id: 'applications', pathContains: '/api/v1/applications' },
  { id: 'companies', pathContains: '/api/v1/companies' },
  { id: 'recruiters', pathContains: '/api/v1/recruiters' },
  { id: 'documents', pathContains: '/api/v1/documents' },
  { id: 'courses', pathContains: '/api/v1/courses' },
  { id: 'faculties', pathContains: '/api/v1/faculties' },
  { id: 'chat', pathContains: '/api/v1/chat' },
  { id: 'notifications', pathContains: '/api/v1/notifications' },
  { id: 'ai', pathContains: '/api/v1/ai' },
  { id: 'assistant', pathContains: '/api/v1/assistant' },
  { id: 'integrations', pathContains: '/api/v1/integrations' },
]

export function getActionLogPathFilter(featureId: string): string | undefined {
  if (!featureId || featureId === 'all') return undefined
  return ACTION_LOG_FEATURE_CLUSTERS.find((c) => c.id === featureId)?.pathContains
}

/** Best-effort label id for table badges (first matching cluster). */
export function inferActionLogFeatureId(path: string): string {
  const lower = path.toLowerCase()
  for (const c of ACTION_LOG_FEATURE_CLUSTERS) {
    if (!c.pathContains) continue
    if (lower.includes(c.pathContains.toLowerCase())) return c.id
  }
  return 'other'
}
