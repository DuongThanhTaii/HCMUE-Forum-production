import { IQueryHandler, QueryHandler } from '@nestjs/cqrs';
import { PrismaService } from '../../../common/prisma/prisma.service';

export class GetPopularTagsQuery {}

@QueryHandler(GetPopularTagsQuery)
export class GetPopularTagsHandler implements IQueryHandler<GetPopularTagsQuery> {
  constructor(private readonly prisma: PrismaService) {}

  async execute(query: GetPopularTagsQuery) {
    return this.prisma.tags.findMany({
      orderBy: { post_count: 'desc' },
      take: 10,
      select: {
        id: true,
        name: true,
        post_count: true,
      },
    });
  }
}
