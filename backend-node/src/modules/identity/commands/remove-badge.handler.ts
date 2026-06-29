import { CommandHandler, ICommandHandler } from '@nestjs/cqrs';
import { PrismaService } from '../../../common/prisma/prisma.service';

export class RemoveBadgeCommand {
  constructor(public readonly userId: string) {}
}

@CommandHandler(RemoveBadgeCommand)
export class RemoveBadgeHandler implements ICommandHandler<RemoveBadgeCommand> {
  constructor(private readonly prisma: PrismaService) {}

  async execute(command: RemoveBadgeCommand) {
    await this.prisma.users.update({
      where: { id: command.userId },
      data: {
        badge_type: null,
        badge_name: null,
        badge_description: null,
        badge_verified_at: null,
        badge_verified_by: null,
      }
    });
  }
}
