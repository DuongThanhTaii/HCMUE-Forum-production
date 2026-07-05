import { CommandHandler, ICommandHandler } from '@nestjs/cqrs';
import { PrismaService } from '../../../common/prisma/prisma.service';

export class DeleteThreadChannelCommand {
  constructor(public readonly id: string) {}
}

@CommandHandler(DeleteThreadChannelCommand)
export class DeleteThreadChannelHandler implements ICommandHandler<DeleteThreadChannelCommand> {
  constructor(private readonly prisma: PrismaService) {}

  async execute(command: DeleteThreadChannelCommand) {
    await this.prisma.thread_channels.delete({
      where: { id: command.id },
    });
    return { success: true };
  }
}
