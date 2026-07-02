import { CommandHandler, ICommandHandler } from '@nestjs/cqrs';
import { PrismaService } from '../../../common/prisma/prisma.service';
import { ForbiddenException, NotFoundException } from '@nestjs/common';

export class DeleteCommentCommand {
  constructor(
    public readonly commentId: string,
    public readonly userId: string,
    public readonly userRoles: string[],
  ) {}
}

@CommandHandler(DeleteCommentCommand)
export class DeleteCommentHandler implements ICommandHandler<DeleteCommentCommand> {
  constructor(private readonly prisma: PrismaService) {}

  async execute(command: DeleteCommentCommand) {
    const comment = await this.prisma.comments.findUnique({
      where: { id: command.commentId },
    });

    if (!comment) {
      throw new NotFoundException('Comment not found');
    }

    const isAuthor = comment.author_id === command.userId;
    const isModeratorOrAdmin = command.userRoles.some((r) =>
      ['Admin', 'Moderator'].includes(r),
    );

    if (!isAuthor && !isModeratorOrAdmin) {
      throw new ForbiddenException('You do not have permission to delete this comment');
    }

    await this.prisma.comments.update({
      where: { id: command.commentId },
      data: { is_deleted: true },
    });

    return { success: true };
  }
}
