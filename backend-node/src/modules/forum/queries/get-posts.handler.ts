import { IQueryHandler, QueryHandler } from '@nestjs/cqrs';
import { PrismaService } from '../../../common/prisma/prisma.service';
import { Prisma } from '@prisma/client';

export class GetPostsQuery {
  constructor(
    public readonly pageNumber: number = 1,
    public readonly pageSize: number = 20,
    public readonly categoryId?: string,
    public readonly threadChannelId?: string,
    public readonly searchTerm?: string,
    public readonly sortBy?: number,
    public readonly isSolved?: boolean,
    public readonly isUnanswered?: boolean,
    public readonly isPinned?: boolean,
    public readonly userId?: string,
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
    if (query.threadChannelId) {
      whereClause.thread_channel_id = query.threadChannelId;
    }
    if (query.searchTerm) {
      // Tags search: if starts with #
      if (query.searchTerm.startsWith('#')) {
        const tag = query.searchTerm.slice(1).trim();
        // Since tags is JSONB, this is a bit tricky in Prisma without raw query, 
        // but for now let's just search title and content if Prisma schema doesn't have an easy array contains.
        // Actually Prisma supports array_contains for scalar lists. Assuming tags is string[]
        whereClause.tags = { has: tag };
      } else {
        whereClause.OR = [
          { title: { contains: query.searchTerm, mode: 'insensitive' } },
          { content: { contains: query.searchTerm, mode: 'insensitive' } },
        ];
      }
    }
    if (query.isPinned !== undefined) {
      whereClause.is_pinned = query.isPinned;
    }
    if (query.isSolved !== undefined) {
      whereClause.is_solved = query.isSolved;
    }
    // if (query.isUnanswered) { whereClause.comment_count = 0; } // Assuming you want 0 replies
    
    const skip = (query.pageNumber - 1) * query.pageSize;
    const take = query.pageSize;

    let totalCount = 0;
    let postsData: any[] = [];

    if (query.sortBy === 1) { // Trending
      // Use Raw SQL for HackerNews hot ranking algorithm
      const conditions: Prisma.Sql[] = [Prisma.sql`status = 2`];
      if (query.categoryId) {
        conditions.push(Prisma.sql`category_id = ${query.categoryId}`);
      }
      if (query.threadChannelId) {
        conditions.push(Prisma.sql`thread_channel_id = ${query.threadChannelId}`);
      }
      if (query.searchTerm) {
        if (query.searchTerm.startsWith('#')) {
          const tag = query.searchTerm.slice(1).trim();
          conditions.push(Prisma.sql`tags @> ARRAY[${tag}]::text[]`);
        } else {
          const search = `%${query.searchTerm}%`;
          conditions.push(Prisma.sql`(title ILIKE ${search} OR content ILIKE ${search})`);
        }
      }
      if (query.isPinned !== undefined) {
        conditions.push(Prisma.sql`is_pinned = ${query.isPinned}`);
      }
      if (query.isSolved !== undefined) {
        conditions.push(Prisma.sql`is_solved = ${query.isSolved}`);
      }

      const whereSql = Prisma.join(conditions, ' AND ');

      const totalCountObj = await this.prisma.$queryRaw<{count: bigint}[]>`
        SELECT COUNT(*) as count 
        FROM forum.posts 
        WHERE ${whereSql}
      `;
      totalCount = Number(totalCountObj[0].count);

      postsData = await this.prisma.$queryRaw<any[]>`
        SELECT * 
        FROM forum.posts 
        WHERE ${whereSql}
        ORDER BY is_pinned DESC, (vote_score / POWER(EXTRACT(EPOCH FROM (NOW() - created_at))/3600 + 2, 1.5)) DESC
        LIMIT ${take} OFFSET ${skip}
      `;
    } else {
      let orderByClause: any = { created_at: 'desc' };
      if (query.sortBy === 2) { // Recently Active (updated_at)
        orderByClause = { updated_at: 'desc' };
      } else if (query.sortBy === 3) { // Most Viewed
        orderByClause = { view_count: 'desc' };
      } else if (query.sortBy === 4) { // Most Liked
        orderByClause = { vote_score: 'desc' };
      }

      totalCount = await this.prisma.posts.count({ where: whereClause });
      postsData = await this.prisma.posts.findMany({
        where: whereClause,
        orderBy: orderByClause,
        skip: skip,
        take: take,
      });
    }

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

    let userBookmarks = new Set<string>();
    let userVotes = new Map<string, number>();
    let bookmarkCounts = new Map<string, number>();

    if (postsData.length > 0) {
      const postIds = postsData.map(p => p.id);

      const countsRaw = await this.prisma.bookmarks.groupBy({
        by: ['post_id'],
        where: { post_id: { in: postIds } },
        _count: { post_id: true }
      });
      bookmarkCounts = new Map(countsRaw.map(b => [b.post_id, b._count.post_id]));

      if (query.userId) {
        const bookmarks = await this.prisma.bookmarks.findMany({
          where: {
            user_id: query.userId,
            post_id: { in: postIds }
          }
        });
        userBookmarks = new Set(bookmarks.map(b => b.post_id));

        const votes = await this.prisma.post_votes.findMany({
          where: {
            user_id: query.userId,
            post_id: { in: postIds }
          }
        });
        userVotes = new Map(votes.map(v => [v.post_id, v.vote_type]));
      }
    }

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
      bookmarkCount: bookmarkCounts.get(p.id) || 0,
      threadChannelId: p.thread_channel_id,
      isBookmarked: userBookmarks.has(p.id),
      currentUserVote: userVotes.get(p.id) ?? null,
    }));

    const page = query.pageNumber;

    return {
      posts,
      totalCount,
      pageNumber: page,
      pageSize: query.pageSize,
      totalPages: Math.ceil(totalCount / query.pageSize),
      hasPreviousPage: page > 1,
      hasNextPage: skip + take < totalCount,
    };
  }
}
