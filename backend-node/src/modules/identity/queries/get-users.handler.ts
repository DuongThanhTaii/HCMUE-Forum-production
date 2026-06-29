import { IQueryHandler, QueryHandler } from '@nestjs/cqrs';
import { GetUsersQuery } from './get-users.query';
import { PrismaService } from '../../../common/prisma/prisma.service';

@QueryHandler(GetUsersQuery)
export class GetUsersHandler implements IQueryHandler<GetUsersQuery> {
  constructor(private readonly prisma: PrismaService) {}

  async execute(query: GetUsersQuery): Promise<any> {
    const users = await this.prisma.users.findMany({
      skip: query.skip || 0,
      take: query.take || 20,
      select: {
        id: true,
        email: true,
        first_name: true,
        last_name: true,
        status: true,
        created_at: true,
      },
      orderBy: { created_at: 'desc' },
    });

    const userIds = users.map(u => u.id);
    const userRoles = await this.prisma.user_roles.findMany({
      where: { user_id: { in: userIds } }
    });
    
    const roleIds = Array.from(new Set(userRoles.map(ur => ur.role_id)));
    const roles = await this.prisma.roles.findMany({
      where: { id: { in: roleIds } }
    });
    const roleMap = new Map(roles.map(r => [r.id, r.name]));

    return users.map(u => {
      const userRoleIds = userRoles.filter(ur => ur.user_id === u.id).map(ur => ur.role_id);
      const userRoleNames = userRoleIds.map(rid => roleMap.get(rid)).filter(Boolean);

      return {
        id: u.id,
        email: u.email,
        firstName: u.first_name,
        lastName: u.last_name,
        fullName: `${u.first_name} ${u.last_name}`.trim(),
        status: u.status,
        createdAt: u.created_at,
        roles: userRoleNames,
      };
    });
  }
}
