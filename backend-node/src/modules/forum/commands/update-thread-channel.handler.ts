import { CommandHandler, ICommandHandler } from '@nestjs/cqrs';
import { PrismaService } from '../../../common/prisma/prisma.service';

export class UpdateThreadChannelCommand {
  constructor(
    public readonly id: string,
    public readonly code?: string,
    public readonly name?: string,
    public readonly description?: string,
    public readonly displayOrder?: number,
    public readonly isActive?: boolean,
    public readonly allowPinnedComments?: boolean,
    public readonly allowAcceptedAnswers?: boolean,
    public readonly allowModeratorActions?: boolean,
  ) {}
}

@CommandHandler(UpdateThreadChannelCommand)
export class UpdateThreadChannelHandler implements ICommandHandler<UpdateThreadChannelCommand> {
  constructor(private readonly prisma: PrismaService) {}

  async execute(command: UpdateThreadChannelCommand) {
    const data: any = { updated_at: new Date() };
    if (command.code !== undefined) data.code = command.code;
    if (command.name !== undefined) data.name = command.name;
    if (command.description !== undefined) data.description = command.description;
    if (command.displayOrder !== undefined) data.display_order = command.displayOrder;
    if (command.isActive !== undefined) data.is_active = command.isActive;
    if (command.allowPinnedComments !== undefined) data.allow_pinned_comments = command.allowPinnedComments;
    if (command.allowAcceptedAnswers !== undefined) data.allow_accepted_answers = command.allowAcceptedAnswers;
    if (command.allowModeratorActions !== undefined) data.allow_moderator_actions = command.allowModeratorActions;

    const threadChannel = await this.prisma.thread_channels.update({
      where: { id: command.id },
      data,
    });
    return threadChannel;
  }
}
