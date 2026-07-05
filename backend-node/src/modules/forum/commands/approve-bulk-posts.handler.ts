import { CommandHandler, ICommandHandler } from '@nestjs/cqrs';
import { PrismaService } from '../../../common/prisma/prisma.service';

export class ApproveBulkPostsCommand {
  constructor(
    public readonly postIds: string[],
    public readonly moderatorId: string,
  ) {}
}

@CommandHandler(ApproveBulkPostsCommand)
export class ApproveBulkPostsHandler implements ICommandHandler<ApproveBulkPostsCommand> {
  constructor(private readonly prisma: PrismaService) {}

  async execute(command: ApproveBulkPostsCommand) {
    if (!command.postIds || command.postIds.length === 0) {
      return { count: 0 };
    }

    // 2 = Published status
    const result = await this.prisma.posts.updateMany({
      where: {
        id: {
          in: command.postIds,
        },
      },
      data: {
        status: 2,
        published_at: new Date(),
        updated_at: new Date(),
      },
    });

    return { count: result.count };
  }
}
