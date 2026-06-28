import { CommandHandler, ICommandHandler } from '@nestjs/cqrs';
import { PrismaService } from '../../../common/prisma/prisma.service';
import { NotificationGateway } from '../gateways/notification.gateway';
import * as crypto from 'crypto';

export class CreateNotificationCommand {
  constructor(
    public readonly recipientId: string,
    public readonly channel: number,
    public readonly status: number,
    public readonly subject: string,
    public readonly body: string,
    public readonly metadata: any = {},
    public readonly templateId?: string,
    public readonly actionUrl?: string,
    public readonly iconUrl?: string,
  ) {}
}

@CommandHandler(CreateNotificationCommand)
export class CreateNotificationHandler implements ICommandHandler<CreateNotificationCommand> {
  constructor(
    private readonly prisma: PrismaService,
    private readonly gateway: NotificationGateway,
  ) {}

  async execute(command: CreateNotificationCommand) {
    const notification = await this.prisma.notifications.create({
      data: {
        id: crypto.randomUUID(),
        recipient_id: command.recipientId,
        template_id: command.templateId,
        channel: command.channel,
        status: command.status,
        content_subject: command.subject,
        content_body: command.body,
        content_action_url: command.actionUrl,
        content_icon_url: command.iconUrl,
        metadata: command.metadata,
        created_at: new Date(),
      },
    });

    this.gateway.broadcastNotification(command.recipientId, notification);
    return notification;
  }
}
