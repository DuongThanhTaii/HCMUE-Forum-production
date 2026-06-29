import { CommandHandler, ICommandHandler } from '@nestjs/cqrs';
import { PrismaService } from '../../../common/prisma/prisma.service';

export class MarkAllReadCommand {
  constructor(public readonly userId: string) {}
}

@CommandHandler(MarkAllReadCommand)
export class MarkAllReadHandler implements ICommandHandler<MarkAllReadCommand> {
  constructor(private readonly prisma: PrismaService) {}

  async execute(command: MarkAllReadCommand) {
    await this.prisma.notifications.updateMany({
      where: {
        recipient_id: command.userId,
        read_at: null,
      },
      data: {
        read_at: new Date(),
      },
    });
    return { success: true };
  }
}
