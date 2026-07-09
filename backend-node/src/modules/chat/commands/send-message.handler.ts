import { CommandBus, CommandHandler, ICommandHandler } from '@nestjs/cqrs';
import { PrismaService } from '../../../common/prisma/prisma.service';
import { ChatGateway } from '../gateways/chat.gateway';
import * as crypto from 'crypto';
import { UpsertChatNotificationCommand } from '../../notification/commands/upsert-chat-notification.handler';

export class SendMessageCommand {
  constructor(
    public readonly conversationId: string,
    public readonly senderId: string,
    public readonly content: string,
    public readonly type: number,
    public readonly replyToMessageId?: string,
  ) {}
}

@CommandHandler(SendMessageCommand)
export class SendMessageHandler implements ICommandHandler<SendMessageCommand> {
  constructor(
    private readonly prisma: PrismaService,
    private readonly gateway: ChatGateway,
    private readonly commandBus: CommandBus,
  ) {}

  async execute(command: SendMessageCommand) {
    const message = await this.prisma.messages.create({
      data: {
        id: crypto.randomUUID(),
        conversation_id: command.conversationId,
        sender_id: command.senderId,
        content: command.content,
        type: command.type,
        sent_at: new Date(),
        reply_to_message_id: command.replyToMessageId,
      },
    });

    await this.prisma.conversations.update({
      where: { id: command.conversationId },
      data: { last_message_at: new Date() },
    });

    const sender = await this.prisma.users.findUnique({
      where: { id: command.senderId },
      select: { first_name: true, last_name: true },
    });
    const senderName = sender ? `${sender.first_name} ${sender.last_name}`.trim() : 'Người dùng';

    const messageDto = {
      messageId: message.id, // Usually messageId for hub
      id: message.id,
      conversationId: message.conversation_id,
      senderId: message.sender_id,
      senderName,
      content: message.content,
      type: message.type,
      messageType: message.type,
      sentAt: message.sent_at,
      editedAt: message.edited_at,
      isDeleted: message.is_deleted,
      replyToMessageId: message.reply_to_message_id,
      reactions: {},
      attachments: [],
    };

    this.gateway.broadcastNewMessage(command.conversationId, messageDto);
    
    // Dispatch system notifications for other participants
    const conversation = await this.prisma.conversations.findUnique({
      where: { id: command.conversationId },
      select: { participants: true },
    });
    
    if (conversation && conversation.participants) {
      const participants = conversation.participants as string[];
      for (const participantId of participants) {
        if (participantId !== command.senderId) {
          this.commandBus.execute(
            new UpsertChatNotificationCommand(participantId, command.conversationId, senderName)
          ).catch(e => console.error('Failed to create chat notification', e));
        }
      }
    }

    return {
      messageId: message.id,
      sentAt: message.sent_at,
    };
  }
}
