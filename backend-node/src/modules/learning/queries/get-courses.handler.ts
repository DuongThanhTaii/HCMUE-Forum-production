import { IQueryHandler, QueryHandler } from '@nestjs/cqrs';
import { PrismaService } from '../../../common/prisma/prisma.service';

export class GetCoursesQuery {
  constructor(public readonly facultyId?: string) {}
}

@QueryHandler(GetCoursesQuery)
export class GetCoursesHandler implements IQueryHandler<GetCoursesQuery> {
  constructor(private readonly prisma: PrismaService) {}

  async execute(query: GetCoursesQuery) {
    const where = query.facultyId ? { faculty_id: query.facultyId } : {};
    return this.prisma.courses.findMany({
      where,
      orderBy: { name: 'asc' },
    });
  }
}
