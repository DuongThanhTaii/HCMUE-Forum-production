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
    const channels = await this.prisma.thread_channels.findMany({
      where,
      orderBy: { display_order: 'asc' },
    });

    const channelIds = channels.map(c => c.id);
    if (channelIds.length > 0) {
      const postCounts = await this.prisma.posts.groupBy({
        by: ['thread_channel_id'],
        _count: {
          id: true,
        },
        where: {
          thread_channel_id: { in: channelIds },
          status: 2, // Assuming 2 is Published
        },
      });

      const countMap = new Map<string, number>();
      postCounts.forEach(pc => {
        if (pc.thread_channel_id) {
          countMap.set(pc.thread_channel_id, pc._count.id);
        }
      });

      return channels.map(c => ({
        ...c,
        post_count: countMap.get(c.id) || 0,
      }));
    }

    return channels;
  }
}
