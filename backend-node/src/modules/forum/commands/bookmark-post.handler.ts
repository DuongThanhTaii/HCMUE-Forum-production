import { CommandHandler, ICommandHandler } from '@nestjs/cqrs';
import { PrismaService } from '../../../common/prisma/prisma.service';
import { NotFoundException, BadRequestException } from '@nestjs/common';

export class BookmarkPostCommand {
  constructor(
    public readonly postId: string,
    public readonly userId: string,
  ) {}
}

@CommandHandler(BookmarkPostCommand)
export class BookmarkPostHandler
  implements ICommandHandler<BookmarkPostCommand>
{
  constructor(private readonly prisma: PrismaService) {}

  async execute(command: BookmarkPostCommand): Promise<any> {
    const { postId, userId } = command;

    const post = await this.prisma.posts.findUnique({
      where: { id: postId },
    });

    if (!post) {
      throw new NotFoundException('Post not found');
    }

    const existing = await this.prisma.bookmarks.findUnique({
      where: {
        post_id_user_id: { post_id: postId, user_id: userId },
      },
    });

    if (!existing) {
      await this.prisma.bookmarks.create({
        data: {
          post_id: postId,
          user_id: userId,
          created_at: new Date(),
        },
      });
    }

    return { success: true };
  }
}
