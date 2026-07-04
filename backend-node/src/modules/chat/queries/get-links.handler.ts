import { IQueryHandler, QueryHandler } from '@nestjs/cqrs';
import { PrismaClient } from '@prisma/client';

export class GetLinksQuery {
  constructor(
    public readonly conversationId: string,
  ) {}
}

@QueryHandler(GetLinksQuery)
export class GetLinksHandler implements IQueryHandler<GetLinksQuery> {
  private prisma = new PrismaClient();

  async execute(query: GetLinksQuery) {
    const { conversationId } = query;

    // Prisma doesn't have a regex search, so we'll do a basic LIKE search for http
    // and extract the URLs in memory.
    const messages = await this.prisma.messages.findMany({
      where: {
        conversation_id: conversationId,
        content: {
          contains: 'http',
        },
        is_deleted: false,
      },
      select: {
        id: true,
        content: true,
        sender_id: true,
        sent_at: true,
      },
      orderBy: {
        sent_at: 'desc',
      },
    });

    const links: any[] = [];
    const urlRegex = /(https?:\/\/[^\s]+)/g;

    for (const msg of messages) {
      const found = msg.content.match(urlRegex);
      if (found) {
        for (const url of found) {
          links.push({
            messageId: msg.id,
            url,
            senderId: msg.sender_id,
            sentAt: msg.sent_at,
          });
        }
      }
    }

    return links;
  }
}
