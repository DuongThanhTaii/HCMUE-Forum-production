import { IQueryHandler, QueryHandler } from '@nestjs/cqrs';
import { PrismaClient } from '@prisma/client';

export class SearchMessagesQuery {
  constructor(
    public readonly conversationId: string,
    public readonly q: string,
  ) {}
}

@QueryHandler(SearchMessagesQuery)
export class SearchMessagesHandler implements IQueryHandler<SearchMessagesQuery> {
  private prisma = new PrismaClient();

  async execute(query: SearchMessagesQuery) {
    const { conversationId, q } = query;

    if (!q || q.trim() === '') {
      return [];
    }

    const messages = await this.prisma.messages.findMany({
      where: {
        conversation_id: conversationId,
        content: {
          contains: q,
          mode: 'insensitive',
        },
        is_deleted: false,
      },
      orderBy: {
        sent_at: 'desc',
      },
      take: 50,
      include: {
        message_attachments: true,
      },
    });

    return messages.map((m) => ({
      id: m.id,
      conversationId: m.conversation_id,
      senderId: m.sender_id,
      content: m.content,
      type: m.type,
      sentAt: m.sent_at,
      editedAt: m.edited_at,
      isDeleted: m.is_deleted,
      replyToMessageId: m.reply_to_message_id,
      attachments: m.message_attachments.map((a) => ({
        fileName: a.file_name,
        fileUrl: a.file_url,
        fileSizeBytes: Number(a.file_size_bytes),
        mimeType: a.mime_type,
        thumbnailUrl: a.thumbnail_url,
      })),
      reactions: [], // Provide empty array to match interface if needed
    }));
  }
}
