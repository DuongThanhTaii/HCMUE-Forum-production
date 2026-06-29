import { CommandHandler, ICommandHandler } from '@nestjs/cqrs';
import { PrismaService } from '../../../common/prisma/prisma.service';
import * as crypto from 'crypto';

export class AssignRoleToUserCommand {
  constructor(
    public readonly userId: string,
    public readonly roleId: string,
  ) {}
}

@CommandHandler(AssignRoleToUserCommand)
export class AssignRoleToUserHandler implements ICommandHandler<AssignRoleToUserCommand> {
  constructor(private readonly prisma: PrismaService) {}

  async execute(command: AssignRoleToUserCommand) {
    const existing = await this.prisma.user_roles.findUnique({
      where: {
        user_id_role_id: {
          user_id: command.userId,
          role_id: command.roleId,
        }
      }
    });

    if (!existing) {
      await this.prisma.user_roles.create({
        data: {
          id: crypto.randomUUID(),
          user_id: command.userId,
          role_id: command.roleId,
          assigned_at: new Date(),
        }
      });
    }
  }
}
