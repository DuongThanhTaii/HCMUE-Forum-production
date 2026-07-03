import { CommandHandler, ICommandHandler } from '@nestjs/cqrs';
import { PrismaService } from '../../../common/prisma/prisma.service';
import { ChatGateway } from '../gateways/chat.gateway';

export class DeleteMessageCommand {
  constructor(
    public readonly messageId: string,
    public readonly userId: string,
    public readonly conversationId: string,
  ) {}
}

@CommandHandler(DeleteMessageCommand)
export class DeleteMessageHandler implements ICommandHandler<DeleteMessageCommand> {
  constructor(
    private readonly prisma: PrismaService,
    private readonly gateway: ChatGateway,
  ) {}

  async execute(command: DeleteMessageCommand) {
    const message = await this.prisma.messages.findUnique({
      where: { id: command.messageId },
    });

    if (!message || message.sender_id !== command.userId) {
      return { success: false };
    }

    await this.prisma.messages.update({
      where: { id: command.messageId },
      data: { is_deleted: true },
    });

    this.gateway.server
      .to(`conversation_${command.conversationId}`)
      .emit('MessageDeleted', { conversationId: command.conversationId, messageId: command.messageId });

    return { success: true };
  }
}
