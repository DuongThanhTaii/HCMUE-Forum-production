import { IQueryHandler, QueryHandler } from '@nestjs/cqrs';
import { PrismaService } from '../../../common/prisma/prisma.service';

export class GetPostCommentsQuery {
  constructor(public readonly postId: string) {}
}

@QueryHandler(GetPostCommentsQuery)
export class GetPostCommentsHandler implements IQueryHandler<GetPostCommentsQuery> {
  constructor(private readonly prisma: PrismaService) {}

  async execute(query: GetPostCommentsQuery) {
    return this.prisma.comments.findMany({
      where: { post_id: query.postId, is_deleted: false },
      orderBy: { created_at: 'asc' },
    });
  }
}
