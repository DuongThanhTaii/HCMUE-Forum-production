import { IQueryHandler, QueryHandler } from '@nestjs/cqrs';
import { PrismaService } from '../../../common/prisma/prisma.service';

export class GetPostsQuery {
  constructor(
    public readonly skip: number = 0,
    public readonly take: number = 20,
    public readonly categoryId?: string,
  ) {}
}

@QueryHandler(GetPostsQuery)
export class GetPostsHandler implements IQueryHandler<GetPostsQuery> {
  constructor(private readonly prisma: PrismaService) {}

  async execute(query: GetPostsQuery) {
    const whereClause = query.categoryId
      ? { category_id: query.categoryId }
      : {};
    return this.prisma.posts.findMany({
      where: whereClause,
      orderBy: { created_at: 'desc' },
      skip: query.skip,
      take: query.take,
    });
  }
}
