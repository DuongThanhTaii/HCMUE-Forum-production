import { CommandHandler, ICommandHandler } from '@nestjs/cqrs';
import { PrismaService } from '../../../common/prisma/prisma.service';
import * as crypto from 'crypto';
import { BadRequestException } from '@nestjs/common';

export class CreateRoleCommand {
  constructor(
    public readonly name: string,
    public readonly description?: string,
  ) {}
}

@CommandHandler(CreateRoleCommand)
export class CreateRoleHandler implements ICommandHandler<CreateRoleCommand> {
  constructor(private readonly prisma: PrismaService) {}

  async execute(command: CreateRoleCommand) {
    const existing = await this.prisma.roles.findUnique({
      where: { name: command.name }
    });

    if (existing) {
      throw new BadRequestException('Role with this name already exists');
    }

    const role = await this.prisma.roles.create({
      data: {
        id: crypto.randomUUID(),
        name: command.name,
        description: command.description,
        is_default: false,
        is_system_role: false,
        created_at: new Date(),
      }
    });

    return {
      id: role.id,
      name: role.name,
      description: role.description,
      isDefault: role.is_default,
      isSystemRole: role.is_system_role,
    };
  }
}
