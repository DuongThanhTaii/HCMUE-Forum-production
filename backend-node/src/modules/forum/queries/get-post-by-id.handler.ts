import { IQueryHandler, QueryHandler } from '@nestjs/cqrs';
import { PrismaService } from '../../../common/prisma/prisma.service';
import { NotFoundException } from '@nestjs/common';

export class GetPostByIdQuery {
  constructor(
    public readonly id: string,
    public readonly userId?: string,
  ) {}
}

@QueryHandler(GetPostByIdQuery)
export class GetPostByIdHandler implements IQueryHandler<GetPostByIdQuery> {
  constructor(private readonly prisma: PrismaService) {}

  async execute(query: GetPostByIdQuery) {
    const post = await this.prisma.posts.update({
      where: { id: query.id },
      data: { view_count: { increment: 1 } },
    }).catch(() => null); // Fallback if not found

    if (!post) {
      throw new NotFoundException('Post not found');
    }
    
    if (!post) {
      throw new NotFoundException('Post not found');
    }

    let authorName;
    let categoryName;

    if (post.author_id) {
      const user = await this.prisma.users.findUnique({ where: { id: post.author_id } });
      authorName = user ? `${user.last_name} ${user.first_name}`.trim() : undefined;
    }

    if (post.category_id) {
      const cat = await this.prisma.categories.findUnique({ where: { id: post.category_id } });
      categoryName = cat ? cat.name : undefined;
    }

    let isBookmarked = false;
    let currentUserVote: number | undefined;

    const bookmarkCount = await this.prisma.bookmarks.count({
      where: { post_id: query.id }
    });

    if (query.userId) {
      const bookmark = await this.prisma.bookmarks.findUnique({
        where: {
          post_id_user_id: {
            post_id: query.id,
            user_id: query.userId,
          },
        },
      });
      isBookmarked = !!bookmark;

      const vote = await this.prisma.post_votes.findUnique({
        where: {
          post_id_user_id: {
            post_id: query.id,
            user_id: query.userId,
          },
        },
      });
      currentUserVote = vote?.vote_type;
    }

    return {
      ...post,
      viewCount: post.view_count, // Explicitly return camelCase for the frontend
      authorName,
      categoryName,
      isBookmarked,
      currentUserVote,
      bookmarkCount,
    };
  }
}
