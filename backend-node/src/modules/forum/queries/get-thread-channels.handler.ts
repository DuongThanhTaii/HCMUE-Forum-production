import { IQueryHandler, QueryHandler } from '@nestjs/cqrs';
import { PrismaService } from '../../../common/prisma/prisma.service';

export class GetThreadChannelsQuery {
  constructor(public readonly includeInactive: boolean = false) {}
}

@QueryHandler(GetThreadChannelsQuery)
export class GetThreadChannelsHandler implements IQueryHandler<GetThreadChannelsQuery> {
  constructor(private readonly prisma: PrismaService) {}

  async execute(query: GetThreadChannelsQuery) {
    const where = query.includeInactive ? {} : { is_active: true };
    return this.prisma.thread_channels.findMany({
      where,
      orderBy: { display_order: 'asc' },
    });
  }
}
