import { IQueryHandler, QueryHandler } from '@nestjs/cqrs';
import { SearchChatUsersQuery } from './search-chat-users.query';
import { PrismaService } from '../../../common/prisma/prisma.service';

@QueryHandler(SearchChatUsersQuery)
export class SearchChatUsersHandler implements IQueryHandler<SearchChatUsersQuery> {
  constructor(private readonly prisma: PrismaService) {}

  async execute(query: SearchChatUsersQuery): Promise<any> {
    const q = query.q?.trim() || '';
    const take = query.take || 24;

    const whereClause: any = {};

    if (q) {
      whereClause.OR = [
        { email: { contains: q, mode: 'insensitive' } },
        { first_name: { contains: q, mode: 'insensitive' } },
        { last_name: { contains: q, mode: 'insensitive' } },
        // also search full name by splitting? Prisma doesn't have an easy concat search without raw query,
        // but contains on first/last name usually works well enough.
      ];
    } else {
      whereClause.user_roles = {
        some: {
          roles: { name: 'Student' }
        }
      };
    }

    const users = await this.prisma.users.findMany({
      where: whereClause,
      take: take,
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
    if (userIds.length === 0) return [];

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
        roleIds: userRoleIds,
      };
    });
  }
}
