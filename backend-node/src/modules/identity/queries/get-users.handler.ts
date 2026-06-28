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

    return users;
  }
}
