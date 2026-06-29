import { CommandHandler, ICommandHandler } from '@nestjs/cqrs';
import { PrismaService } from '../../../common/prisma/prisma.service';

export class UnbookmarkPostCommand {
  constructor(
    public readonly postId: string,
    public readonly userId: string,
  ) {}
}

@CommandHandler(UnbookmarkPostCommand)
export class UnbookmarkPostHandler
  implements ICommandHandler<UnbookmarkPostCommand>
{
  constructor(private readonly prisma: PrismaService) {}

  async execute(command: UnbookmarkPostCommand): Promise<any> {
    const { postId, userId } = command;

    try {
      await this.prisma.bookmarks.delete({
        where: {
          post_id_user_id: { post_id: postId, user_id: userId },
        },
      });
    } catch (e) {
      // Ignore if it doesn't exist
    }

    return { success: true };
  }
}
