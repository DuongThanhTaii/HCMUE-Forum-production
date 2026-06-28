import { CommandHandler, ICommandHandler } from '@nestjs/cqrs';
import { PrismaService } from '../../../common/prisma/prisma.service';
import { ChatGateway } from '../gateways/chat.gateway';

export class AddReactionCommand {
  constructor(
    public readonly messageId: string,
    public readonly userId: string,
    public readonly emoji: string,
    public readonly conversationId: string,
  ) {}
}

@CommandHandler(AddReactionCommand)
export class AddReactionHandler implements ICommandHandler<AddReactionCommand> {
  constructor(
    private readonly prisma: PrismaService,
    private readonly gateway: ChatGateway,
  ) {}

  async execute(command: AddReactionCommand) {
    const reaction = await this.prisma.message_reactions.create({
      data: {
        message_id: command.messageId,
        user_id: command.userId,
        emoji: command.emoji,
        reacted_at: new Date(),
      },
    });

    this.gateway.broadcastReaction(command.conversationId, reaction);
    return reaction;
  }
}
