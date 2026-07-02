import { IQueryHandler, QueryHandler } from '@nestjs/cqrs';
import { PrismaService } from '../../../common/prisma/prisma.service';

export class GetCategoriesQuery {}

@QueryHandler(GetCategoriesQuery)
export class GetCategoriesHandler implements IQueryHandler<GetCategoriesQuery> {
  constructor(private readonly prisma: PrismaService) {}

  async execute(query: GetCategoriesQuery) {
    const categories = await this.prisma.categories.findMany({
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

    const categoryIds = categories.map((c) => c.id);
    if (categoryIds.length > 0) {
      const postCounts = await this.prisma.posts.groupBy({
        by: ['category_id'],
        _count: {
          id: true,
        },
        where: {
          category_id: { in: categoryIds },
          status: 2, // Assuming 2 is Published
        },
      });

      const countMap = new Map<string, number>();
      postCounts.forEach((pc) => {
        if (pc.category_id) {
          countMap.set(pc.category_id, pc._count.id);
        }
      });

      return categories.map((cat) => ({
        ...cat,
        post_count: countMap.get(cat.id) || 0,
      }));
    }

    return categories;
  }
}
