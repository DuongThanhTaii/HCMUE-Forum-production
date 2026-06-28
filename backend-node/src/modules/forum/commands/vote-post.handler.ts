import { CommandHandler, ICommandHandler } from '@nestjs/cqrs';
import { PrismaService } from '../../../common/prisma/prisma.service';

export class VotePostCommand {
  constructor(
    public readonly postId: string,
    public readonly userId: string,
    public readonly voteType: number,
  ) {}
}

@CommandHandler(VotePostCommand)
export class VotePostHandler implements ICommandHandler<VotePostCommand> {
  constructor(private readonly prisma: PrismaService) {}

  async execute(command: VotePostCommand) {
    const existingVote = await this.prisma.post_votes.findUnique({
      where: {
        post_id_user_id: { post_id: command.postId, user_id: command.userId },
      },
    });

    let voteDiff = command.voteType;
    if (existingVote) {
      if (existingVote.vote_type === command.voteType) return null;
      voteDiff = command.voteType - existingVote.vote_type;
      await this.prisma.post_votes.update({
        where: {
          post_id_user_id: { post_id: command.postId, user_id: command.userId },
        },
        data: { vote_type: command.voteType, updated_at: new Date() },
      });
    } else {
      await this.prisma.post_votes.create({
        data: {
          post_id: command.postId,
          user_id: command.userId,
          vote_type: command.voteType,
          created_at: new Date(),
        },
      });
    }

    return this.prisma.posts.update({
      where: { id: command.postId },
      data: { vote_score: { increment: voteDiff } },
    });
  }
}
