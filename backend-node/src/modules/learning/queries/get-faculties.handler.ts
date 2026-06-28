import { IQueryHandler, QueryHandler } from '@nestjs/cqrs';
import { PrismaService } from '../../../common/prisma/prisma.service';

export class GetFacultiesQuery {}

@QueryHandler(GetFacultiesQuery)
export class GetFacultiesHandler implements IQueryHandler<GetFacultiesQuery> {
  constructor(private readonly prisma: PrismaService) {}

  async execute(query: GetFacultiesQuery) {
    return this.prisma.faculties.findMany({
      orderBy: { name: 'asc' },
    });
  }
}
