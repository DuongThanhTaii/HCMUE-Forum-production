import { CommandHandler, ICommandHandler } from '@nestjs/cqrs';
import { PrismaService } from '../../../common/prisma/prisma.service';

export class DeleteCategoryCommand {
  constructor(public readonly id: string) {}
}

@CommandHandler(DeleteCategoryCommand)
export class DeleteCategoryHandler implements ICommandHandler<DeleteCategoryCommand> {
  constructor(private readonly prisma: PrismaService) {}

  async execute(command: DeleteCategoryCommand) {
    await this.prisma.categories.delete({
      where: { id: command.id },
    });

    return { success: true };
  }
}
