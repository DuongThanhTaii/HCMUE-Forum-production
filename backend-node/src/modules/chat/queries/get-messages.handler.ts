import { IQueryHandler, QueryHandler } from '@nestjs/cqrs';
import { PrismaService } from '../../../common/prisma/prisma.service';

export class GetMessagesQuery {
  constructor(
    public readonly conversationId: string,
    public readonly skip: number = 0,
    public readonly take: number = 50,
  ) {}
}

@QueryHandler(GetMessagesQuery)
export class GetMessagesHandler implements IQueryHandler<GetMessagesQuery> {
  constructor(private readonly prisma: PrismaService) {}

  async execute(query: GetMessagesQuery) {
    return this.prisma.messages.findMany({
      where: { conversation_id: query.conversationId, is_deleted: false },
      orderBy: { sent_at: 'desc' },
      skip: query.skip,
      take: query.take,
    });
  }
}
