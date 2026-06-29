import { IQueryHandler, QueryHandler } from '@nestjs/cqrs';
import { PrismaService } from '../../../common/prisma/prisma.service';
import { NotFoundException } from '@nestjs/common';

export class GetPermissionByIdQuery {
  constructor(public readonly permissionId: string) {}
}

@QueryHandler(GetPermissionByIdQuery)
export class GetPermissionByIdHandler implements IQueryHandler<GetPermissionByIdQuery> {
  constructor(private readonly prisma: PrismaService) {}

  async execute(query: GetPermissionByIdQuery) {
    const permission = await this.prisma.permissions.findUnique({
      where: { id: query.permissionId },
    });

    if (!permission) {
      throw new NotFoundException('Permission not found');
    }

    return {
      id: permission.id,
      code: permission.code,
      name: permission.name,
      description: permission.description,
      module: permission.module,
      resource: permission.resource,
      action: permission.action,
    };
  }
}
