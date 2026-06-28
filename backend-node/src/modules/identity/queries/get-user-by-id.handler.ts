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
    return {
      ...rest,
      fullName: `${first_name} ${last_name}`.trim(),
    };
  }
}
