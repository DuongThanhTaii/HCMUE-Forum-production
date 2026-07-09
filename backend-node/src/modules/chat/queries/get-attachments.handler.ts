import { IQueryHandler, QueryHandler } from '@nestjs/cqrs';
import { PrismaService } from '../../../common/prisma/prisma.service';

export class GetAttachmentsQuery {
  constructor(
    public readonly conversationId: string,
    public readonly type?: 'image' | 'video' | 'file' | 'all',
  ) {}
}

@QueryHandler(GetAttachmentsQuery)
export class GetAttachmentsHandler implements IQueryHandler<GetAttachmentsQuery> {
  constructor(private readonly prisma: PrismaService) {}

  async execute(query: GetAttachmentsQuery) {
    const { conversationId, type } = query;

    let mimeTypeFilter: any = undefined;
    if (type === 'image') {
      mimeTypeFilter = { startsWith: 'image/' };
    } else if (type === 'video') {
      mimeTypeFilter = { startsWith: 'video/' };
    } else if (type === 'file') {
      mimeTypeFilter = {
        not: {
          startsWith: 'image/',
        },
      };
    }

    const attachments = await this.prisma.message_attachments.findMany({
      where: {
        messages: {
          conversation_id: conversationId,
        },
        ...(mimeTypeFilter && { mime_type: mimeTypeFilter }),
      },
      include: {
        messages: {
          select: {
            id: true,
            sender_id: true,
            sent_at: true,
          },
        },
      },
      orderBy: {
        messages: {
          sent_at: 'desc',
        },
      },
    });

    return attachments.map((att) => ({
      messageId: att.messages.id,
      fileName: att.file_name,
      fileUrl: att.file_url,
      fileSizeBytes: Number(att.file_size_bytes),
      mimeType: att.mime_type,
      thumbnailUrl: att.thumbnail_url,
      senderId: att.messages.sender_id,
      sentAt: att.messages.sent_at,
    }));
  }
}
