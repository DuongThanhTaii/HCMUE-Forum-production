import { IQueryHandler, QueryHandler } from '@nestjs/cqrs';
import { PrismaService } from '../../../common/prisma/prisma.service';

export class GetUserNotificationsQuery {
  constructor(
    public readonly userId: string,
    public readonly skip: number = 0,
    public readonly take: number = 20,
  ) {}
}

@QueryHandler(GetUserNotificationsQuery)
export class GetUserNotificationsHandler implements IQueryHandler<GetUserNotificationsQuery> {
  constructor(private readonly prisma: PrismaService) {}

  async execute(query: GetUserNotificationsQuery) {
    return this.prisma.notifications.findMany({
      where: { recipient_id: query.userId },
      orderBy: { created_at: 'desc' },
      skip: query.skip,
      take: query.take,
    });
  }
}
