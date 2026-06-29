import { CommandHandler, ICommandHandler } from '@nestjs/cqrs';
import { PrismaService } from '../../../common/prisma/prisma.service';
import * as crypto from 'crypto';

export class AssignPermissionToRoleCommand {
  constructor(
    public readonly roleId: string,
    public readonly permissionId: string,
    public readonly scopeType: number,
    public readonly scopeValue?: string,
  ) {}
}

@CommandHandler(AssignPermissionToRoleCommand)
export class AssignPermissionToRoleHandler implements ICommandHandler<AssignPermissionToRoleCommand> {
  constructor(private readonly prisma: PrismaService) {}

  async execute(command: AssignPermissionToRoleCommand) {
    const existing = await this.prisma.role_permissions.findUnique({
      where: {
        role_id_permission_id: {
          role_id: command.roleId,
          permission_id: command.permissionId,
        }
      }
    });

    if (existing) {
      await this.prisma.role_permissions.update({
        where: { id: existing.id },
        data: {
          scope_type: command.scopeType,
          scope_value: command.scopeValue || null,
        }
      });
    } else {
      await this.prisma.role_permissions.create({
        data: {
          id: crypto.randomUUID(),
          role_id: command.roleId,
          permission_id: command.permissionId,
          scope_type: command.scopeType,
          scope_value: command.scopeValue || null,
          assigned_at: new Date(),
        }
      });
    }
  }
}
