import { CommandHandler, ICommandHandler } from '@nestjs/cqrs';
import { PrismaService } from '../../../common/prisma/prisma.service';

export class MarkNotificationReadCommand {
  constructor(
    public readonly notificationId: string,
    public readonly userId: string,
  ) {}
}

@CommandHandler(MarkNotificationReadCommand)
export class MarkNotificationReadHandler implements ICommandHandler<MarkNotificationReadCommand> {
  constructor(private readonly prisma: PrismaService) {}

  async execute(command: MarkNotificationReadCommand) {
    return this.prisma.notifications.update({
      where: { id: command.notificationId },
      data: { read_at: new Date() },
    });
  }
}
