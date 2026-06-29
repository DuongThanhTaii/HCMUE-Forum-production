import { IQueryHandler, QueryHandler } from '@nestjs/cqrs';
import { PrismaService } from '../../../common/prisma/prisma.service';
import { NotFoundException } from '@nestjs/common';

export class GetRoleByIdQuery {
  constructor(public readonly roleId: string) {}
}

@QueryHandler(GetRoleByIdQuery)
export class GetRoleByIdHandler implements IQueryHandler<GetRoleByIdQuery> {
  constructor(private readonly prisma: PrismaService) {}

  async execute(query: GetRoleByIdQuery) {
    const role = await this.prisma.roles.findUnique({
      where: { id: query.roleId },
    });

    if (!role) {
      throw new NotFoundException('Role not found');
    }

    const rolePermissions = await this.prisma.role_permissions.findMany({
      where: { role_id: role.id }
    });

    const permissionIds = rolePermissions.map(rp => rp.permission_id);
    const permissions = await this.prisma.permissions.findMany({
      where: { id: { in: permissionIds } }
    });

    const permissionMap = new Map(permissions.map(p => [p.id, p]));

    return {
      id: role.id,
      name: role.name,
      description: role.description,
      isDefault: role.is_default,
      isSystemRole: role.is_system_role,
      permissions: rolePermissions.map(rp => {
        const p = permissionMap.get(rp.permission_id);
        return {
          permissionId: rp.permission_id,
          permissionCode: p?.code || '',
          scopeType: rp.scope_type,
          scopeValue: rp.scope_value || '',
          assignedAt: rp.assigned_at,
        };
      })
    };
  }
}
