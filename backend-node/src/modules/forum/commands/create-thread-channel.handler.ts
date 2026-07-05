import { CommandHandler, ICommandHandler } from '@nestjs/cqrs';
import { PrismaService } from '../../../common/prisma/prisma.service';
import * as crypto from 'crypto';

export class CreateThreadChannelCommand {
  constructor(
    public readonly code: string,
    public readonly name: string,
    public readonly description?: string,
    public readonly displayOrder?: number,
    public readonly isActive?: boolean,
    public readonly allowPinnedComments?: boolean,
    public readonly allowAcceptedAnswers?: boolean,
    public readonly allowModeratorActions?: boolean,
  ) {}
}

@CommandHandler(CreateThreadChannelCommand)
export class CreateThreadChannelHandler implements ICommandHandler<CreateThreadChannelCommand> {
  constructor(private readonly prisma: PrismaService) {}

  async execute(command: CreateThreadChannelCommand) {
    const threadChannel = await this.prisma.thread_channels.create({
      data: {
        id: crypto.randomUUID(),
        code: command.code,
        name: command.name,
        description: command.description,
        display_order: command.displayOrder || 0,
        is_active: command.isActive ?? true,
        allow_pinned_comments: command.allowPinnedComments ?? true,
        allow_accepted_answers: command.allowAcceptedAnswers ?? true,
        allow_moderator_actions: command.allowModeratorActions ?? true,
        created_at: new Date(),
      },
    });
    return threadChannel;
  }
}
