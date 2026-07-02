import { IQueryHandler, QueryHandler } from '@nestjs/cqrs';
import { PrismaService } from '../../../common/prisma/prisma.service';

export class GetPostCommentsQuery {
  constructor(public readonly postId: string, public readonly userId?: string) {}
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

    let userVotes = new Map<string, number>();
    if (query.userId) {
      const votes = await this.prisma.comment_votes.findMany({
        where: {
          user_id: query.userId,
          comment_id: { in: comments.map(c => c.id) }
        }
      });
      userVotes = new Map(votes.map(v => [v.comment_id, v.vote_type]));
    }

    const authorIds = [...new Set(comments.map((c) => c.author_id))];
    const users = await this.prisma.users.findMany({
      where: { id: { in: authorIds } },
      select: {
        id: true,
        first_name: true,
        last_name: true,
        avatar: true,
        user_roles: { select: { role_id: true } },
      },
    });

    const userMap = new Map(users.map((u) => [u.id, u]));
    
    // Get all roles
    const roles = await this.prisma.roles.findMany();
    const roleMap = new Map(roles.map(r => [r.id, r.name]));

    return comments.map((comment) => {
      const user = userMap.get(comment.author_id);
      return {
        ...comment,
        authorName: user ? `${user.last_name} ${user.first_name}`.trim() : undefined,
        authorAvatar: user?.avatar,
        authorRoles: user?.user_roles?.map((ur) => roleMap.get(ur.role_id)).filter(Boolean) || [],
        currentUserVote: userVotes.get(comment.id) ?? null,
      };
    });
  }
}
