import { CommandHandler, ICommandHandler } from '@nestjs/cqrs';
import { PrismaService } from '../../../common/prisma/prisma.service';

export class MarkConversationReadCommand {
  constructor(
    public readonly userId: string,
    public readonly conversationId: string,
  ) {}
}

@CommandHandler(MarkConversationReadCommand)
export class MarkConversationReadHandler implements ICommandHandler<MarkConversationReadCommand> {
  constructor(private readonly prisma: PrismaService) {}

  async execute(command: MarkConversationReadCommand) {
    // Find notification for this conversation and mark it as read
    const notification = await this.prisma.notifications.findFirst({
      where: {
        recipient_id: command.userId,
        content_action_url: \/chat?conversation=\\,
        read_at: null,
      }
    });

    if (notification) {
      await this.prisma.notifications.update({
        where: { id: notification.id },
        data: { read_at: new Date() }
      });
    }

    return { success: true };
  }
}
