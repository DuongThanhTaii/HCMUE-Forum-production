import { CommandHandler, ICommandHandler } from '@nestjs/cqrs';
import { PrismaService } from '../../../common/prisma/prisma.service';
import { NotFoundException } from '@nestjs/common';

export class UpdateRoleCommand {
  constructor(
    public readonly id: string,
    public readonly name?: string,
    public readonly description?: string,
  ) {}
}

@CommandHandler(UpdateRoleCommand)
export class UpdateRoleHandler implements ICommandHandler<UpdateRoleCommand> {
  constructor(private readonly prisma: PrismaService) {}

  async execute(command: UpdateRoleCommand) {
    const role = await this.prisma.roles.findUnique({
      where: { id: command.id }
    });

    if (!role) {
      throw new NotFoundException('Role not found');
    }

    await this.prisma.roles.update({
      where: { id: command.id },
      data: {
        name: command.name !== undefined ? command.name : role.name,
        description: command.description !== undefined ? command.description : role.description,
        updated_at: new Date(),
      }
    });
  }
}
