import { CommandHandler, ICommandHandler } from '@nestjs/cqrs';
import { PrismaService } from '../../../common/prisma/prisma.service';
import { ChatGateway } from '../gateways/chat.gateway';

export class RemoveReactionCommand {
  constructor(
    public readonly messageId: string,
    public readonly userId: string,
    public readonly emoji: string,
    public readonly conversationId: string,
  ) {}
}

@CommandHandler(RemoveReactionCommand)
export class RemoveReactionHandler implements ICommandHandler<RemoveReactionCommand> {
  constructor(
    private readonly prisma: PrismaService,
    private readonly gateway: ChatGateway,
  ) {}

  async execute(command: RemoveReactionCommand) {
    await this.prisma.message_reactions.deleteMany({
      where: {
        message_id: command.messageId,
        user_id: command.userId,
        emoji: command.emoji,
      },
    });

    this.gateway.broadcastReaction(command.conversationId, {
      message_id: command.messageId,
      user_id: command.userId,
      emoji: command.emoji,
      conversationId: command.conversationId
    });
    return { success: true };
  }
}
