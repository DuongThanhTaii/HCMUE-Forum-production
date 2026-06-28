import { IQueryHandler, QueryHandler } from '@nestjs/cqrs';
import { PrismaService } from '../../../common/prisma/prisma.service';

export class GetCourseDocumentsQuery {
  constructor(public readonly courseId: string) {}
}

@QueryHandler(GetCourseDocumentsQuery)
export class GetCourseDocumentsHandler implements IQueryHandler<GetCourseDocumentsQuery> {
  constructor(private readonly prisma: PrismaService) {}

  async execute(query: GetCourseDocumentsQuery) {
    return this.prisma.documents.findMany({
      where: { course_id: query.courseId },
      orderBy: { created_at: 'desc' },
    });
  }
}
