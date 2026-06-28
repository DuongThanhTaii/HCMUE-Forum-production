export interface PermissionDto {
  id: string
  code: string
  name: string
  description: string
  module: string
  resource: string
  action: string
}

export interface RoleDto {
  id: string
  name: string
  description: string
  isDefault: boolean
  isSystemRole: boolean
  permissionCount: number
  createdAt: string
}

export interface RolePermissionAssignmentDto {
  permissionId?: string
  id?: string
  scopeType?: string
  scopeValue?: string | null
}

export interface RoleDetailDto extends RoleDto {
  permissions?: RolePermissionAssignmentDto[]
}

export interface CreateRoleRequest {
  name: string
  description: string
}

export interface UpdateRoleRequest {
  name: string
  description: string
}

export interface AssignPermissionRequest {
  permissionId: string
  scopeType: string
  scopeValue: string | null
}

export interface RemovePermissionRequest {
  roleId: string
  permissionId: string
  scopeType: string
  scopeValue: string | null
}

export interface BadgeDto {
  type: string
  name: string
  description: string
  emoji: string
}

export type UserStatus = 'Active' | 'Inactive' | 'Banned'

export interface UserDto {
  id: string
  email: string
  fullName: string
  bio: string | null
  status: UserStatus
  badge: BadgeDto | null
  roleIds?: string[]
  createdAt: string
}

export interface UserGroupDto {
  id: string
  name: string
  description?: string | null
  isActive: boolean
  memberCount: number
}

export interface AssignRoleToUserRequest {
  roleId: string
}

export interface AssignBadgeRequest {
  badgeType: string
  badgeName: string
  description: string
}

export type OverrideEffect = 'Allow' | 'Deny'

export interface PermissionOverrideDto {
  overrideId: string
  permissionId: string
  permissionCode: string
  scopeType: string
  scopeValue: string | null
  effect: OverrideEffect
  reason: string | null
  expiresAtUtc: string | null
  createdAtUtc: string
  updatedAtUtc: string | null
  isRevoked: boolean
}

export interface UpsertPermissionOverrideRequest {
  permissionId: string
  scopeType: string
  scopeValue: string | null
  effect: OverrideEffect
  reason: string | null
  expiresAtUtc: string | null
}

export interface RevokePermissionOverrideRequest {
  permissionId: string
  scopeType: string
  scopeValue: string | null
}

export interface EndpointToggleDto {
  endpointKey: string
  isEnabled: boolean
  reason: string | null
  updatedBy: string
  updatedAtUtc: string
  version: number
}

export interface SetEndpointToggleRequest {
  isEnabled: boolean
  reason: string | null
}

export interface MaintenanceModeDto {
  isEnabled: boolean
  reason: string | null
  updatedBy: string
  updatedAtUtc: string
  version: number
}

export interface SetMaintenanceModeRequest {
  isEnabled: boolean
  reason: string | null
}

export interface ThreadChannelDto {
  id: string
  code: string
  name: string
  description: string
  displayOrder: number
  isActive: boolean
  allowPinnedComments: boolean
  allowAcceptedAnswers: boolean
  allowModeratorActions: boolean
}

export interface UpsertThreadChannelRequest {
  code: string
  name: string
  description: string | null
  displayOrder: number
  isActive: boolean
  allowPinnedComments: boolean
  allowAcceptedAnswers: boolean
  allowModeratorActions: boolean
}

export interface AuditLogDto {
  auditLogId: string
  actorUserId: string | null
  action: string
  targetType: string
  targetKey: string
  isSuccess: boolean
  detail: string
  occurredAtUtc: string
}

export interface AuditLogsFilterParams {
  userId?: string
  endpointKey?: string
  isSuccess?: boolean
  fromUtc?: string
  toUtc?: string
  take?: number
}

export type UserActionLogsViewType = 'Developer' | 'Administrator'

export interface UserActionLogItemDto {
  id: string
  actorUserId: string
  method: string
  path: string
  queryString: string
  endpoint: string
  statusCode: number
  durationMs: number
  traceId: string
  correlationId: string
  remoteIp: string
  userAgent: string
  scheme: string
  host: string
  startedAtUtc: string
  completedAtUtc: string
  result: string
  exceptionType: string | null
  exceptionMessage: string | null
  terminalLine: string
  requestHeadersJson: string
  requestContentType: string | null
  requestBodyPreview: string | null
  requestBodyTruncated: boolean
  responseHeadersJson: string
  responseContentType: string | null
  responseBodyPreview: string | null
  responseBodyTruncated: boolean
}

export interface UserActionLogsResponse {
  items: UserActionLogItemDto[]
  total: number
  page: number
  pageSize: number
  viewType: UserActionLogsViewType
  availableViewTypes: UserActionLogsViewType[]
  persistToMongo: boolean
  mongoCollectionName: string | null
}

export interface UserActionLogsFilterParams {
  actorUserId?: string
  correlationId?: string
  traceId?: string
  method?: string
  pathContains?: string
  minStatusCode?: number
  maxStatusCode?: number
  fromUtc?: string
  toUtc?: string
  viewType?: UserActionLogsViewType
  page?: number
  pageSize?: number
}
