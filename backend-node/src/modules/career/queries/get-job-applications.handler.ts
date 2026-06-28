import { IQueryHandler, QueryHandler } from '@nestjs/cqrs';
import { PrismaService } from '../../../common/prisma/prisma.service';

export class GetJobApplicationsQuery {
  constructor(public readonly jobPostingId: string) {}
}

@QueryHandler(GetJobApplicationsQuery)
export class GetJobApplicationsHandler implements IQueryHandler<GetJobApplicationsQuery> {
  constructor(private readonly prisma: PrismaService) {}

  async execute(query: GetJobApplicationsQuery) {
    const apps = await this.prisma.applications.findMany({
      where: { job_posting_id: query.jobPostingId },
      orderBy: { submitted_at: 'desc' },
    });

    return apps.map((app) => ({
      ...app,
      resume_file_size_bytes: app.resume_file_size_bytes.toString(),
    }));
  }
}
