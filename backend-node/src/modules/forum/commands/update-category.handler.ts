import { CommandHandler, ICommandHandler } from '@nestjs/cqrs';
import { PrismaService } from '../../../common/prisma/prisma.service';

export class UpdateCategoryCommand {
  constructor(
    public readonly id: string,
    public readonly name?: string,
    public readonly description?: string,
    public readonly slug?: string,
    public readonly displayOrder?: number,
    public readonly isActive?: boolean,
    public readonly parentCategoryId?: string,
    public readonly moderatorIds?: string[],
  ) {}
}

@CommandHandler(UpdateCategoryCommand)
export class UpdateCategoryHandler implements ICommandHandler<UpdateCategoryCommand> {
  constructor(private readonly prisma: PrismaService) {}

  async execute(command: UpdateCategoryCommand) {
    const data: any = { updated_at: new Date() };
    if (command.name !== undefined) data.name = command.name;
    if (command.description !== undefined) data.description = command.description;
    if (command.slug !== undefined) data.slug = command.slug;
    if (command.displayOrder !== undefined) data.display_order = command.displayOrder;
    if (command.isActive !== undefined) data.is_active = command.isActive;
    if (command.parentCategoryId !== undefined) data.parent_category_id = command.parentCategoryId;
    if (command.moderatorIds !== undefined) data.moderator_ids = command.moderatorIds;

    const category = await this.prisma.categories.update({
      where: { id: command.id },
      data,
    });

    return category;
  }
}
