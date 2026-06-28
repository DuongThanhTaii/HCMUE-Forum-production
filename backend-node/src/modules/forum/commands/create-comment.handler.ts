import { CommandHandler, ICommandHandler } from '@nestjs/cqrs';
import { PrismaService } from '../../../common/prisma/prisma.service';
import * as crypto from 'crypto';

export class CreateCommentCommand {
  constructor(
    public readonly postId: string,
    public readonly authorId: string,
    public readonly content: string,
    public readonly parentCommentId?: string,
  ) {}
}

@CommandHandler(CreateCommentCommand)
export class CreateCommentHandler implements ICommandHandler<CreateCommentCommand> {
  constructor(private readonly prisma: PrismaService) {}

  async execute(command: CreateCommentCommand) {
    const comment = await this.prisma.comments.create({
      data: {
        id: crypto.randomUUID(),
        post_id: command.postId,
        author_id: command.authorId,
        content: command.content,
        parent_comment_id: command.parentCommentId,
        created_at: new Date(),
      },
    });

    await this.prisma.posts.update({
      where: { id: command.postId },
      data: { comment_count: { increment: 1 } },
    });

    return comment;
  }
}
