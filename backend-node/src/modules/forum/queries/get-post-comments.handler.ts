import { IQueryHandler, QueryHandler } from '@nestjs/cqrs';
import { PrismaService } from '../../../common/prisma/prisma.service';

export class GetPostCommentsQuery {
  constructor(public readonly postId: string) {}
}

@QueryHandler(GetPostCommentsQuery)
export class GetPostCommentsHandler implements IQueryHandler<GetPostCommentsQuery> {
  constructor(private readonly prisma: PrismaService) {}

  async execute(query: GetPostCommentsQuery) {
    const comments = await this.prisma.comments.findMany({
      where: { post_id: query.postId, is_deleted: false },
      orderBy: { created_at: 'asc' },
    });

    if (comments.length === 0) {
      return [];
    }

    const authorIds = [...new Set(comments.map((c) => c.author_id))];
    const users = await this.prisma.users.findMany({
      where: { id: { in: authorIds } },
      select: { id: true, first_name: true, last_name: true, avatar: true },
    });

    const userMap = new Map(users.map((u) => [u.id, u]));

    return comments.map((comment) => {
      const user = userMap.get(comment.author_id);
      return {
        ...comment,
        authorName: user ? `${user.last_name} ${user.first_name}`.trim() : undefined,
        authorAvatar: user?.avatar,
      };
    });
  }
}
