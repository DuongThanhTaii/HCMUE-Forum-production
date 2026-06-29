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
    const whereClause: any = { status: 2 }; // 2 = Published
    if (query.categoryId) {
      whereClause.category_id = query.categoryId;
    }

    const totalCount = await this.prisma.posts.count({ where: whereClause });
    const postsData = await this.prisma.posts.findMany({
      where: whereClause,
      orderBy: { created_at: 'desc' },
      skip: query.skip,
      take: query.take,
    });

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
      content: p.content,
      type: p.type,
      status: p.status,
      authorName: userMap.get(p.author_id),
      categoryName: p.category_id ? categoryMap.get(p.category_id) : undefined,
      createdAt: p.created_at,
      publishedAt: p.published_at,
      commentCount: p.comment_count,
      viewCount: p.view_count,
      voteScore: p.vote_score,
      threadChannelId: p.thread_channel_id,
    }));

    const page = Math.floor(query.skip / query.take) + 1;

    return {
      posts,
      totalCount,
      pageNumber: page,
      pageSize: query.take,
      totalPages: Math.ceil(totalCount / query.take),
      hasPreviousPage: page > 1,
      hasNextPage: query.skip + query.take < totalCount,
    };
  }
}
