import { IQueryHandler, QueryHandler } from '@nestjs/cqrs';
import { GetUserByIdQuery } from './get-user-by-id.query';
import { PrismaService } from '../../../common/prisma/prisma.service';
import { NotFoundException } from '@nestjs/common';

@QueryHandler(GetUserByIdQuery)
export class GetUserByIdHandler implements IQueryHandler<GetUserByIdQuery> {
  constructor(private readonly prisma: PrismaService) {}

  async execute(query: GetUserByIdQuery): Promise<any> {
    const user = await this.prisma.users.findUnique({
      where: { id: query.userId },
      select: {
        id: true,
        email: true,
        first_name: true,
        last_name: true,
        avatar: true,
        bio: true,
        status: true,
        created_at: true,
      },
    });

    if (!user) {
      throw new NotFoundException('User not found');
    }

    const { first_name, last_name, ...rest } = user;
    
    const userRoles = await this.prisma.user_roles.findMany({
      where: { user_id: user.id }
    });
    const roleIds = userRoles.map(ur => ur.role_id);
    const roles = await this.prisma.roles.findMany({
      where: { id: { in: roleIds } }
    });
    const userRoleNames = roles.map(r => r.name);

    return {
      id: user.id,
      email: user.email,
      firstName: first_name,
      lastName: last_name,
      fullName: `${first_name} ${last_name}`.trim(),
      avatar: user.avatar,
      bio: user.bio,
      status: user.status,
      createdAt: user.created_at,
      roles: userRoleNames,
      roleIds: roleIds,
    };
  }
}
