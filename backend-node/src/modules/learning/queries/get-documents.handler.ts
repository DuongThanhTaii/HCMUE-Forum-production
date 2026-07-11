import { IQueryHandler, QueryHandler } from '@nestjs/cqrs';
import { PrismaService } from '../../../common/prisma/prisma.service';

export class GetDocumentsQuery {
  constructor(
    public readonly pageNumber: number = 1,
    public readonly pageSize: number = 10,
    public readonly status?: number,
  ) {}
}

@QueryHandler(GetDocumentsQuery)
export class GetDocumentsHandler implements IQueryHandler<GetDocumentsQuery> {
  constructor(private readonly prisma: PrismaService) {}

  async execute(query: GetDocumentsQuery) {
    const skip = (query.pageNumber - 1) * query.pageSize;
    const where: any = {};
    if (query.status !== undefined && !isNaN(query.status)) {
      where.status = query.status;
    }

    const [items, totalCount] = await Promise.all([
      this.prisma.documents.findMany({
        where,
        skip,
        take: query.pageSize,
        orderBy: { created_at: 'desc' },
      }),
      this.prisma.documents.count({ where }),
    ]);

    // Prisma returns BigInt for file_size which can't be JSON serialized natively. Let's map it.
    const mappedItems = items.map(item => ({
      ...item,
      file_size: Number(item.file_size),
    }));

    return {
      items: mappedItems,
      totalCount,
      pageNumber: query.pageNumber,
      pageSize: query.pageSize,
      totalPages: Math.ceil(totalCount / query.pageSize),
    };
  }
}
