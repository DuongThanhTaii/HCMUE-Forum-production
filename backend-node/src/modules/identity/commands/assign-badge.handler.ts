import { CommandHandler, ICommandHandler } from '@nestjs/cqrs';
import { PrismaService } from '../../../common/prisma/prisma.service';

export class AssignBadgeCommand {
  constructor(
    public readonly userId: string,
    public readonly badgeType: number,
    public readonly badgeName: string,
    public readonly badgeDescription: string,
  ) {}
}

@CommandHandler(AssignBadgeCommand)
export class AssignBadgeHandler implements ICommandHandler<AssignBadgeCommand> {
  constructor(private readonly prisma: PrismaService) {}

  async execute(command: AssignBadgeCommand) {
    await this.prisma.users.update({
      where: { id: command.userId },
      data: {
        badge_type: command.badgeType,
        badge_name: command.badgeName,
        badge_description: command.badgeDescription,
        badge_verified_at: new Date(),
      }
    });
  }
}
