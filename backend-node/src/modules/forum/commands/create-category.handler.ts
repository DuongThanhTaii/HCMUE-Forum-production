import { CommandHandler, ICommandHandler } from '@nestjs/cqrs';
import { PrismaService } from '../../../common/prisma/prisma.service';
import * as crypto from 'crypto';

export class CreateCategoryCommand {
  constructor(
    public readonly name: string,
    public readonly description: string,
    public readonly slug: string,
    public readonly displayOrder: number,
    public readonly isActive: boolean,
    public readonly parentCategoryId?: string,
    public readonly moderatorIds?: string[],
  ) {}
}

@CommandHandler(CreateCategoryCommand)
export class CreateCategoryHandler implements ICommandHandler<CreateCategoryCommand> {
  constructor(private readonly prisma: PrismaService) {}

  async execute(command: CreateCategoryCommand) {
    const category = await this.prisma.categories.create({
      data: {
        id: crypto.randomUUID(),
        name: command.name,
        description: command.description,
        slug: command.slug,
        parent_category_id: command.parentCategoryId || null,
        display_order: command.displayOrder || 0,
        is_active: command.isActive !== undefined ? command.isActive : true,
        moderator_ids: command.moderatorIds || [],
        created_at: new Date(),
      },
    });

    return category;
  }
}
