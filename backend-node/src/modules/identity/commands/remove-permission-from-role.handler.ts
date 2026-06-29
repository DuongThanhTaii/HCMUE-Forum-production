import { CommandHandler, ICommandHandler } from '@nestjs/cqrs';
import { PrismaService } from '../../../common/prisma/prisma.service';

export class RemovePermissionFromRoleCommand {
  constructor(
    public readonly roleId: string,
    public readonly permissionId: string,
  ) {}
}

@CommandHandler(RemovePermissionFromRoleCommand)
export class RemovePermissionFromRoleHandler implements ICommandHandler<RemovePermissionFromRoleCommand> {
  constructor(private readonly prisma: PrismaService) {}

  async execute(command: RemovePermissionFromRoleCommand) {
    const existing = await this.prisma.role_permissions.findUnique({
      where: {
        role_id_permission_id: {
          role_id: command.roleId,
          permission_id: command.permissionId,
        }
      }
    });

    if (existing) {
      await this.prisma.role_permissions.delete({
        where: { id: existing.id }
      });
    }
  }
}
