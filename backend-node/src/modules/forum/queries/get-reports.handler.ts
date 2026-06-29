import { IQueryHandler, QueryHandler } from '@nestjs/cqrs';
import { PrismaService } from '../../../common/prisma/prisma.service';

export class GetReportsQuery {
  constructor(
    public readonly statusFilter: string = 'pending',
    public readonly page: number = 1,
    public readonly pageSize: number = 20,
  ) {}
}

@QueryHandler(GetReportsQuery)
export class GetReportsHandler implements IQueryHandler<GetReportsQuery> {
  constructor(private readonly prisma: PrismaService) {}

  async execute(query: GetReportsQuery) {
    const { statusFilter, page, pageSize } = query;
    const skip = (page - 1) * pageSize;

    let dbStatus = 1; // Pending
    let dbResolution: number | undefined;

    if (statusFilter === 'resolved_keep') {
      dbStatus = 2;
      dbResolution = 1; // Keep
    } else if (statusFilter === 'resolved_remove') {
      dbStatus = 2;
      dbResolution = 2; // Remove
    }

    const where: any = { status: dbStatus };
    if (dbResolution !== undefined) {
      where.resolution_decision = dbResolution;
    }

    const totalCount = await this.prisma.reports.count({ where });
    const reportsData = await this.prisma.reports.findMany({
      where,
      orderBy: { created_at: 'desc' },
      skip,
      take: pageSize,
    });

    // Resolve snapshots (just title/content previews)
    const reports = await Promise.all(
      reportsData.map(async (r) => {
        let titlePreview = '';
        let contentPreview = '';
        let isTargetDeleted = false;

        if (r.reported_item_type === 1) { // Post
          const post = await this.prisma.posts.findUnique({ where: { id: r.reported_item_id } });
          if (post) {
            titlePreview = post.title.substring(0, 120);
            contentPreview = post.content.substring(0, 240);
            isTargetDeleted = post.status === 3; // 3 means deleted/banned
          } else {
            isTargetDeleted = true;
          }
        } else if (r.reported_item_type === 2) { // Comment
          const comment = await this.prisma.comments.findUnique({ where: { id: r.reported_item_id } });
          if (comment) {
            contentPreview = comment.content.substring(0, 240);
            isTargetDeleted = comment.is_deleted;
          } else {
            isTargetDeleted = true;
          }
        }

        return {
          id: r.id,
          reportedItemId: r.reported_item_id,
          reportedItemType: r.reported_item_type,
          reporterId: r.reporter_id,
          reason: r.reason,
          description: r.description,
          status: r.status,
          createdAt: r.created_at,
          reviewedAt: r.reviewed_at,
          reviewedBy: r.reviewed_by,
          resolutionDecision: r.resolution_decision === 1 ? 'keep' : r.resolution_decision === 2 ? 'remove' : null,
          titlePreview,
          contentPreview,
          isTargetDeleted,
        };
      })
    );

    return {
      reports,
      totalCount,
      pageNumber: page,
      pageSize,
      totalPages: Math.ceil(totalCount / pageSize),
      hasPreviousPage: page > 1,
      hasNextPage: skip + pageSize < totalCount,
    };
  }
}
