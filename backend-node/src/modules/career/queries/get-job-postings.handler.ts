import { IQueryHandler, QueryHandler } from '@nestjs/cqrs';
import { PrismaService } from '../../../common/prisma/prisma.service';

export class GetJobPostingsQuery {
  constructor(public readonly companyId?: string) {}
}

@QueryHandler(GetJobPostingsQuery)
export class GetJobPostingsHandler implements IQueryHandler<GetJobPostingsQuery> {
  constructor(private readonly prisma: PrismaService) {}

  async execute(query: GetJobPostingsQuery) {
    const where = query.companyId ? { company_id: query.companyId } : {};
    return this.prisma.job_postings.findMany({
      where,
      orderBy: { created_at: 'desc' },
    });
  }
}
