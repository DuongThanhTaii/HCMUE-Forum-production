import { IQueryHandler, QueryHandler } from '@nestjs/cqrs';
import { PrismaService } from '../../../common/prisma/prisma.service';

export class GetBookmarkedPostsQuery {
  constructor(
    public readonly skip: number,
    public readonly take: number,
    public readonly userId: string,
  ) {}
}

@QueryHandler(GetBookmarkedPostsQuery)
export class GetBookmarkedPostsHandler
  implements IQueryHandler<GetBookmarkedPostsQuery>
{
  constructor(private readonly prisma: PrismaService) {}

  async execute(query: GetBookmarkedPostsQuery): Promise<any> {
    const { skip, take, userId } = query;

    const bookmarks = await this.prisma.bookmarks.findMany({
      where: { user_id: userId },
      select: { post_id: true },
      orderBy: { created_at: 'desc' },
      skip,
      take,
    });

    const postIds = bookmarks.map((b) => b.post_id);

    if (postIds.length === 0) {
      return { items: [], total: 0 };
    }

    const posts = await this.prisma.posts.findMany({
      where: { id: { in: postIds } },
      include: {
        thread_channels: true,
      },
    });

    const categoryIds = posts.map(p => p.category_id).filter((id): id is string => Boolean(id));
    const categories = await this.prisma.categories.findMany({
      where: { id: { in: categoryIds } }
    });

    // We must sort the posts to match the order of `postIds`
    const sortedPosts = postIds
      .map((id) => posts.find((p) => p.id === id))
      .filter((p): p is NonNullable<typeof p> => Boolean(p));

    const total = await this.prisma.bookmarks.count({
      where: { user_id: userId },
    });

    return {
      items: sortedPosts.map((post) => {
        const category = categories.find(c => c.id === post.category_id);
        return {
          id: post.id,
          title: post.title,
          type: post.type,
          status: post.status,
          authorId: post.author_id,
          tags: post.tags,
          categoryId: post.category_id,
          categoryName: category?.name,
          threadChannelId: post.thread_channel_id,
          threadChannelCode: post.thread_channels?.code,
          threadChannelName: post.thread_channels?.name,
          commentCount: post.comment_count,
          voteScore: post.vote_score,
          viewCount: post.view_count,
          isPinned: post.is_pinned,
          isLocked: post.is_locked,
          createdAt: post.created_at,
          updatedAt: post.updated_at,
          publishedAt: post.published_at,
          isBookmarked: true,
        };
      }),
      total,
    };
  }
}
