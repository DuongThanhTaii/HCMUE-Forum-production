import { CommandHandler, ICommandHandler } from '@nestjs/cqrs';
import { PrismaService } from '../../../common/prisma/prisma.service';
import { BadRequestException } from '@nestjs/common';

export class ResolveReportCommand {
  constructor(
    public readonly reportId: number,
    public readonly action: string, // 'keep' or 'remove'
  ) {}
}

@CommandHandler(ResolveReportCommand)
export class ResolveReportHandler implements ICommandHandler<ResolveReportCommand> {
  constructor(private readonly prisma: PrismaService) {}

  async execute(command: ResolveReportCommand) {
    const report = await this.prisma.reports.findUnique({
      where: { id: command.reportId },
    });

    if (!report) {
      throw new BadRequestException('Report not found');
    }

    if (report.status === 2) {
      throw new BadRequestException('Report already resolved');
    }

    const resolutionDecision = command.action === 'remove' ? 2 : 1;

    // Update the reported item
    if (command.action === 'remove') {
      if (report.reported_item_type === 1) { // Post
        await this.prisma.posts.update({
          where: { id: report.reported_item_id },
          data: { status: 3 }, // 3 = Removed/Banned
        });
      } else if (report.reported_item_type === 2) { // Comment
        await this.prisma.comments.update({
          where: { id: report.reported_item_id },
          data: { is_deleted: true },
        });
      }
    }

    // Update report
    await this.prisma.reports.update({
      where: { id: command.reportId },
      data: {
        status: 2, // 2 = Resolved
        resolution_decision: resolutionDecision,
        reviewed_at: new Date(),
      },
    });

    return { success: true };
  }
}
