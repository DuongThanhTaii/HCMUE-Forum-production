import { CommandHandler, ICommandHandler } from '@nestjs/cqrs';
import { PrismaService } from '../../../common/prisma/prisma.service';

export class RemoveRoleFromUserCommand {
  constructor(
    public readonly userId: string,
    public readonly roleId: string,
  ) {}
}

@CommandHandler(RemoveRoleFromUserCommand)
export class RemoveRoleFromUserHandler implements ICommandHandler<RemoveRoleFromUserCommand> {
  constructor(private readonly prisma: PrismaService) {}

  async execute(command: RemoveRoleFromUserCommand) {
    const existing = await this.prisma.user_roles.findUnique({
      where: {
        user_id_role_id: {
          user_id: command.userId,
          role_id: command.roleId,
        }
      }
    });

    if (existing) {
      await this.prisma.user_roles.delete({
        where: { id: existing.id }
      });
    }
  }
}
