import { IQueryHandler, QueryHandler } from '@nestjs/cqrs';
import { PrismaService } from '../../../common/prisma/prisma.service';

export class GetPermissionsQuery {}

@QueryHandler(GetPermissionsQuery)
export class GetPermissionsHandler implements IQueryHandler<GetPermissionsQuery> {
  constructor(private readonly prisma: PrismaService) {}

  async execute(query: GetPermissionsQuery) {
    const permissions = await this.prisma.permissions.findMany({
      orderBy: { module: 'asc' },
    });
    
    return permissions.map(p => ({
      id: p.id,
      code: p.code,
      name: p.name,
      description: p.description,
      module: p.module,
      resource: p.resource,
      action: p.action,
    }));
  }
}
