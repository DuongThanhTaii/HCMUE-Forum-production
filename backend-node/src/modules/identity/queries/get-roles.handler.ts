import { IQueryHandler, QueryHandler } from '@nestjs/cqrs';
import { PrismaService } from '../../../common/prisma/prisma.service';

export class GetRolesQuery {}

@QueryHandler(GetRolesQuery)
export class GetRolesHandler implements IQueryHandler<GetRolesQuery> {
  constructor(private readonly prisma: PrismaService) {}

  async execute(query: GetRolesQuery) {
    const roles = await this.prisma.roles.findMany({
      orderBy: { name: 'asc' },
    });
    
    return roles.map(r => ({
      id: r.id,
      name: r.name,
      description: r.description,
      isDefault: r.is_default,
      isSystemRole: r.is_system_role,
      createdAt: r.created_at,
    }));
  }
}
