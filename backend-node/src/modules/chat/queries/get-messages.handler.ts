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
    const where = { conversation_id: query.conversationId, is_deleted: false };
    
    const [messages, totalCount] = await Promise.all([
      this.prisma.messages.findMany({
        where,
        orderBy: { sent_at: 'desc' },
        skip: query.skip,
        take: query.take,
      }),
      this.prisma.messages.count({ where }),
    ]);

    const senderIds = Array.from(new Set(messages.map(m => m.sender_id)));
    const users = await this.prisma.users.findMany({
      where: { id: { in: senderIds } },
      select: { id: true, first_name: true, last_name: true },
    });
    const userMap = new Map(users.map(u => [u.id, `${u.first_name} ${u.last_name}`.trim()]));

    const items = messages.map(m => ({
      id: m.id,
      conversationId: m.conversation_id,
      senderId: m.sender_id,
      senderDisplayName: userMap.get(m.sender_id) || null,
      content: m.content,
      type: m.type,
      sentAt: m.sent_at,
      editedAt: m.edited_at,
      isDeleted: m.is_deleted,
      replyToMessageId: m.reply_to_message_id,
      reactions: {}, // stub
      attachments: [], // stub
    }));

    return { items, totalCount };
  }
}
