import { CommandHandler, ICommandHandler } from '@nestjs/cqrs';
import { PrismaService } from '../../../common/prisma/prisma.service';
import { NotFoundException } from '@nestjs/common';

export class DeleteRoleCommand {
  constructor(public readonly id: string) {}
}

@CommandHandler(DeleteRoleCommand)
export class DeleteRoleHandler implements ICommandHandler<DeleteRoleCommand> {
  constructor(private readonly prisma: PrismaService) {}

  async execute(command: DeleteRoleCommand) {
    const role = await this.prisma.roles.findUnique({
      where: { id: command.id }
    });

    if (!role) {
      throw new NotFoundException('Role not found');
    }

    await this.prisma.roles.delete({
      where: { id: command.id }
    });
  }
}
