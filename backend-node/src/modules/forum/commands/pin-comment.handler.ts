import { CommandHandler, ICommandHandler } from '@nestjs/cqrs';
import { PrismaService } from '../../../common/prisma/prisma.service';
import { NotFoundException, ForbiddenException } from '@nestjs/common';

export class PinCommentCommand {
  constructor(
    public readonly commentId: string,
    public readonly userId: string,
  ) {}
}

@CommandHandler(PinCommentCommand)
export class PinCommentHandler implements ICommandHandler<PinCommentCommand> {
  constructor(private readonly prisma: PrismaService) {}

  async execute(command: PinCommentCommand): Promise<any> {
    const { commentId, userId } = command;

    const comment = await this.prisma.comments.findUnique({
      where: { id: commentId },
    });

    if (!comment) {
      throw new NotFoundException('Comment not found');
    }

    const post = await this.prisma.posts.findUnique({
      where: { id: comment.post_id },
    });

    const isAuthor = post?.author_id === userId;
    
    if (!isAuthor) {
      // Typically moderators can pin, but for now we enforce author.
      throw new ForbiddenException('Only the author or moderator can pin a comment');
    }

    const newStatus = !comment.is_pinned;

    await this.prisma.comments.update({
      where: { id: commentId },
      data: { is_pinned: newStatus, updated_at: new Date() },
    });

    return { success: true, isPinned: newStatus };
  }
}
