import { IQueryHandler, QueryHandler } from '@nestjs/cqrs';
import { PrismaService } from '../../../common/prisma/prisma.service';

export class GetPendingPostsQuery {
  constructor(
    public readonly page: number = 1,
    public readonly pageSize: number = 20,
  ) {}
}

@QueryHandler(GetPendingPostsQuery)
export class GetPendingPostsHandler implements IQueryHandler<GetPendingPostsQuery> {
  constructor(private readonly prisma: PrismaService) {}

  async execute(query: GetPendingPostsQuery) {
    const skip = (query.page - 1) * query.pageSize;
    const where = { status: 1 }; // 1 = Pending

    const totalCount = await this.prisma.posts.count({ where });
    const postsData = await this.prisma.posts.findMany({
      where,
      orderBy: { created_at: 'desc' },
      skip,
      take: query.pageSize,
    });

    // We need to fetch authorName and categoryName for pending posts
    const authorIds = postsData.map(p => p.author_id);
    const categoryIds = postsData.map(p => p.category_id).filter(id => id !== null) as string[];

    const users = await this.prisma.users.findMany({
      where: { id: { in: authorIds } },
      select: { id: true, first_name: true, last_name: true }
    });

    const categories = await this.prisma.categories.findMany({
      where: { id: { in: categoryIds } },
      select: { id: true, name: true }
    });

    const userMap = new Map(users.map(u => [u.id, `${u.last_name} ${u.first_name}`.trim()]));
    const categoryMap = new Map(categories.map(c => [c.id, c.name]));

    const posts = postsData.map(p => ({
      id: p.id,
      title: p.title,
      authorName: userMap.get(p.author_id),
      categoryName: p.category_id ? categoryMap.get(p.category_id) : undefined,
      createdAt: p.created_at,
      commentCount: p.comment_count,
    }));

    return {
      posts,
      totalCount,
      pageNumber: query.page,
      pageSize: query.pageSize,
      totalPages: Math.ceil(totalCount / query.pageSize),
      hasPreviousPage: query.page > 1,
      hasNextPage: skip + query.pageSize < totalCount,
    };
  }
}
