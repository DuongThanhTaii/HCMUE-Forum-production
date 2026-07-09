import { CommandHandler, ICommandHandler } from '@nestjs/cqrs';
import { PrismaService } from '../../../common/prisma/prisma.service';
import { NotificationGateway } from '../gateways/notification.gateway';
import * as crypto from 'crypto';

export class UpsertChatNotificationCommand {
  constructor(
    public readonly recipientId: string,
    public readonly conversationId: string,
    public readonly senderName: string,
  ) {}
}

@CommandHandler(UpsertChatNotificationCommand)
export class UpsertChatNotificationHandler implements ICommandHandler<UpsertChatNotificationCommand> {
  constructor(
    private readonly prisma: PrismaService,
    private readonly gateway: NotificationGateway,
  ) {}

  async execute(command: UpsertChatNotificationCommand) {
    const actionUrl = `/chat?conversation=${command.conversationId}`;
    
    // Find existing unread notification
    const existing = await this.prisma.notifications.findFirst({
      where: {
        recipient_id: command.recipientId,
        content_action_url: actionUrl,
        read_at: null,
      },
      orderBy: { created_at: 'desc' },
    });

    if (existing) {
      const metadata = (existing.metadata as any) || {};
      const newCount = (metadata.unreadCount || 1) + 1;
      const newBody = `Bạn có ${newCount} tin nhắn mới từ ${command.senderName}`;
      
      const updated = await this.prisma.notifications.update({
        where: { id: existing.id },
        data: {
          content_body: newBody,
          metadata: { ...metadata, unreadCount: newCount },
          created_at: new Date(),
        },
      });
      
      this.gateway.broadcastNotification(command.recipientId, updated);
      return updated;
    } else {
      const notification = await this.prisma.notifications.create({
        data: {
          id: crypto.randomUUID(),
          recipient_id: command.recipientId,
          channel: 0, // 0 for In-app
          status: 0,  // 0 for Unread
          content_subject: 'Tin nhắn mới',
          content_body: `Bạn có 1 tin nhắn mới từ ${command.senderName}`,
          content_action_url: actionUrl,
          metadata: { unreadCount: 1, conversationId: command.conversationId },
          created_at: new Date(),
        },
      });

      this.gateway.broadcastNotification(command.recipientId, notification);
      return notification;
    }
  }
}
