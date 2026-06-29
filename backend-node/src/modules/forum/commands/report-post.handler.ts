import { CommandHandler, ICommandHandler } from '@nestjs/cqrs';
import { PrismaService } from '../../../common/prisma/prisma.service';
import { NotFoundException } from '@nestjs/common';

export class ReportPostCommand {
  constructor(
    public readonly postId: string,
    public readonly reporterId: string,
    public readonly reason: number,
    public readonly description?: string,
  ) {}
}

@CommandHandler(ReportPostCommand)
export class ReportPostHandler implements ICommandHandler<ReportPostCommand> {
  constructor(private readonly prisma: PrismaService) {}

  async execute(command: ReportPostCommand): Promise<any> {
    const { postId, reporterId, reason, description } = command;

    const post = await this.prisma.posts.findUnique({
      where: { id: postId },
    });

    if (!post) {
      throw new NotFoundException('Post not found');
    }

    await this.prisma.reports.create({
      data: {
        reported_item_id: postId,
        reported_item_type: 1, // 1 = Post
        reporter_id: reporterId,
        reason: reason,
        description: description,
        status: 1, // 1 = Pending
        created_at: new Date(),
      },
    });

    return { success: true };
  }
}
