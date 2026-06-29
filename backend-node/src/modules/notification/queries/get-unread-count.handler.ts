import { IQueryHandler, QueryHandler } from '@nestjs/cqrs';
import { PrismaService } from '../../../common/prisma/prisma.service';

export class GetUnreadCountQuery {
  constructor(public readonly userId: string) {}
}

@QueryHandler(GetUnreadCountQuery)
export class GetUnreadCountHandler implements IQueryHandler<GetUnreadCountQuery> {
  constructor(private readonly prisma: PrismaService) {}

  async execute(query: GetUnreadCountQuery) {
    const count = await this.prisma.notifications.count({
      where: {
        recipient_id: query.userId,
        read_at: null,
      },
    });
    return count; // returns a number, will be wrapped by TransformInterceptor to { data: count }
  }
}
