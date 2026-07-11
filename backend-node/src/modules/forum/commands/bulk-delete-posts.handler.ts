import { CommandHandler, ICommandHandler } from '@nestjs/cqrs';
import { PrismaService } from '../../../common/prisma/prisma.service';

export class BulkDeletePostsCommand {
  constructor(
    public readonly postIds: string[],
    public readonly requesterId: string,
  ) {}
}

@CommandHandler(BulkDeletePostsCommand)
export class BulkDeletePostsHandler implements ICommandHandler<BulkDeletePostsCommand> {
  constructor(private readonly prisma: PrismaService) {}

  async execute(command: BulkDeletePostsCommand) {
    if (!command.postIds || command.postIds.length === 0) {
      return { count: 0 };
    }

    // Hard delete the posts
    const result = await this.prisma.posts.deleteMany({
      where: {
        id: {
          in: command.postIds,
        },
      },
    });

    return { count: result.count };
  }
}
