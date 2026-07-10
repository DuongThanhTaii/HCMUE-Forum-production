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
    const skip = query.skip;
    const take = query.take;
    const convos = await this.prisma.$queryRaw<any[]>`
      SELECT * FROM chat.conversations 
      WHERE participants @> ${`"${query.userId}"`}::jsonb 
      AND is_archived = false
      ORDER BY last_message_at DESC 
      LIMIT ${take} OFFSET ${skip}
    `;

    const mapped = await Promise.all(convos.map(async (c) => {
      // camelCase conversion for DTO
      const result: any = {
        id: c.id,
        type: c.type,
        participantIds: c.participants,
        lastMessageAt: c.last_message_at,
        createdAt: c.created_at,
        isArchived: c.is_archived,
        title: c.title,
        isMuted: false, // Stub
        isBlockedWithPeer: false, // Stub
        unreadCount: 0,
      };

      const notification = await this.prisma.notifications.findFirst({
        where: {
          recipient_id: query.userId,
          content_action_url: `/chat?conversation=${c.id}`,
          read_at: null,
        }
      });
      if (notification && notification.metadata) {
        const meta = notification.metadata as any;
        result.unreadCount = meta.unreadCount || 1;
      }

      if (c.type === 1 && Array.isArray(c.participants)) {
        const otherId = c.participants.find((p: string) => p !== query.userId);
        if (otherId) {
          result.directPeerUserId = otherId;
          const user = await this.prisma.users.findUnique({ where: { id: otherId } });
          if (user) {
            result.directPeerFullName = `${user.first_name || ''} ${user.last_name || ''}`.trim() || user.email;
            result.directPeerEmail = user.email;
          }
        }
      }
      return result;
    }));

    return mapped;
  }
}
