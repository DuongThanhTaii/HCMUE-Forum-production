import { CommandHandler, ICommandHandler } from '@nestjs/cqrs';
import { PrismaService } from '../../../common/prisma/prisma.service';
import { NotFoundException, BadRequestException, ForbiddenException } from '@nestjs/common';

export class AcceptAnswerCommand {
  constructor(
    public readonly commentId: string,
    public readonly userId: string,
  ) {}
}

@CommandHandler(AcceptAnswerCommand)
export class AcceptAnswerHandler implements ICommandHandler<AcceptAnswerCommand> {
  constructor(private readonly prisma: PrismaService) {}

  async execute(command: AcceptAnswerCommand): Promise<any> {
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

    // Must be the author of the post or a moderator to accept an answer
    const isAuthor = post?.author_id === userId;
    
    if (!isAuthor) {
      throw new ForbiddenException('Only the author can accept an answer');
    }

    // Toggle the accepted answer
    const newStatus = !comment.is_accepted_answer;

    await this.prisma.comments.update({
      where: { id: commentId },
      data: { is_accepted_answer: newStatus, updated_at: new Date() },
    });

    return { success: true, isAcceptedAnswer: newStatus };
  }
}
