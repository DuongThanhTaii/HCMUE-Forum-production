import { IQueryHandler, QueryHandler } from '@nestjs/cqrs';
import { PrismaService } from '../../../common/prisma/prisma.service';

export class GetConversationsQuery {
  constructor(
    public readonly userId: string,
    public readonly skip: number = 0,
    public readonly take: number = 20,
  ) {}
}

@QueryHandler(GetConversationsQuery)
export class GetConversationsHandler implements IQueryHandler<GetConversationsQuery> {
  constructor(private readonly prisma: PrismaService) {}

  async execute(query: GetConversationsQuery) {
    // In PostgreSQL, if participants is stored as JSONB array, we can find conversations containing the userId.
    const skip = query.skip;
    const take = query.take;
    // We use a safe parameterized raw query for the JSONB containment operator
    return this.prisma.$queryRaw`
      SELECT * FROM chat.conversations 
      WHERE participants @> ${`"${query.userId}"`}::jsonb 
      ORDER BY last_message_at DESC 
      LIMIT ${take} OFFSET ${skip}
    `;
  }
}
