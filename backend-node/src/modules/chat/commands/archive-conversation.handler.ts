import { CommandHandler, ICommandHandler } from '@nestjs/cqrs';
import { PrismaService } from '../../../common/prisma/prisma.service';
import { UnauthorizedException } from '@nestjs/common';

export class ArchiveConversationCommand {
  constructor(
    public readonly userId: string,
    public readonly conversationId: string,
  ) {}
}

@CommandHandler(ArchiveConversationCommand)
export class ArchiveConversationHandler implements ICommandHandler<ArchiveConversationCommand> {
  constructor(private readonly prisma: PrismaService) {}

  async execute(command: ArchiveConversationCommand) {
    const convo = await this.prisma.conversations.findUnique({
      where: { id: command.conversationId },
    });
    if (!convo) throw new Error('Conversation not found');

    const participants = convo.participants as string[];
    if (!participants.includes(command.userId)) {
      throw new UnauthorizedException('Not a participant');
    }

    await this.prisma.conversations.update({
      where: { id: command.conversationId },
      data: { is_archived: true },
    });

    return { success: true };
  }
}
