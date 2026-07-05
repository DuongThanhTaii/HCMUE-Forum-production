import { CommandHandler, ICommandHandler } from '@nestjs/cqrs';
import { PrismaService } from '../../../common/prisma/prisma.service';
import { NotFoundException } from '@nestjs/common';

export class RejectPostCommand {
  constructor(
    public readonly postId: string,
    public readonly moderatorId: string,
    public readonly reason?: string,
  ) {}
}

@CommandHandler(RejectPostCommand)
export class RejectPostHandler implements ICommandHandler<RejectPostCommand> {
  constructor(private readonly prisma: PrismaService) {}

  async execute(command: RejectPostCommand) {
    const post = await this.prisma.posts.findUnique({
      where: { id: command.postId },
    });

    if (!post) {
      throw new NotFoundException('Post not found');
    }

    // 3 = Rejected status (assuming 1 = Pending, 2 = Published, 3 = Rejected based on typical enum)
    const updatedPost = await this.prisma.posts.update({
      where: { id: command.postId },
      data: {
        status: 3,
        updated_at: new Date(),
      },
    });

    return updatedPost;
  }
}
