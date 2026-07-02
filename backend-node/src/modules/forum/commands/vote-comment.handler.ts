import { CommandHandler, ICommandHandler } from '@nestjs/cqrs';
import { PrismaService } from '../../../common/prisma/prisma.service';
import { NotFoundException } from '@nestjs/common';

export class VoteCommentCommand {
  constructor(
    public readonly commentId: string,
    public readonly userId: string,
    public readonly voteType: number, // 0 = None, 1 = Upvote, 2 = Downvote
  ) {}
}

@CommandHandler(VoteCommentCommand)
export class VoteCommentHandler implements ICommandHandler<VoteCommentCommand> {
  constructor(private readonly prisma: PrismaService) {}

  async execute(command: VoteCommentCommand): Promise<any> {
    const { commentId, userId, voteType } = command;

    const comment = await this.prisma.comments.findUnique({
      where: { id: commentId },
    });

    if (!comment) {
      throw new NotFoundException('Comment not found');
    }

    const existingVote = await this.prisma.comment_votes.findUnique({
      where: {
        comment_id_user_id: { comment_id: commentId, user_id: userId },
      },
    });

    let scoreDelta = 0;

    const getVoteValue = (type: number) => {
      if (type === 1) return 1;
      if (type === 2) return -1;
      return 0;
    };

    if (existingVote) {
      const oldVoteValue = getVoteValue(existingVote.vote_type);
      
      // Whether they click the same vote type OR the opposite vote type,
      // we just remove their existing vote. This prevents the -2 delta
      // which causes confusion (e.g. going from 1 to -1).
      scoreDelta = -oldVoteValue;
      
      await this.prisma.comment_votes.delete({
        where: { comment_id_user_id: { comment_id: commentId, user_id: userId } },
      });
    } else {
      scoreDelta = getVoteValue(voteType);
      
      await this.prisma.comment_votes.create({
        data: {
          comment_id: commentId,
          user_id: userId,
          vote_type: voteType,
          created_at: new Date(),
        },
      });
    }

    if (scoreDelta !== 0) {
      await this.prisma.comments.update({
        where: { id: commentId },
        data: { vote_score: { increment: scoreDelta } },
      });
    }

    return { success: true, delta: scoreDelta };
  }
}
