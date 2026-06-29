import { IQueryHandler, QueryHandler } from '@nestjs/cqrs';
import { PrismaService } from '../../../common/prisma/prisma.service';

export class GetCategoriesQuery {}

@QueryHandler(GetCategoriesQuery)
export class GetCategoriesHandler implements IQueryHandler<GetCategoriesQuery> {
  constructor(private readonly prisma: PrismaService) {}

  async execute(query: GetCategoriesQuery) {
    return this.prisma.categories.findMany({
      where: { is_active: true },
      orderBy: { display_order: 'asc' },
      select: {
        id: true,
        name: true,
        description: true,
        slug: true,
        parent_category_id: true,
        post_count: true,
      },
    });
  }
}
