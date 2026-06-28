import { CommandHandler, ICommandHandler } from '@nestjs/cqrs';
import { PrismaService } from '../../../common/prisma/prisma.service';

export class UpdatePostCommand {
  constructor(
    public readonly postId: string,
    public readonly authorId: string,
    public readonly title: string,
    public readonly content: string,
  ) {}
}

@CommandHandler(UpdatePostCommand)
export class UpdatePostHandler implements ICommandHandler<UpdatePostCommand> {
  constructor(private readonly prisma: PrismaService) {}

  async execute(command: UpdatePostCommand) {
    return this.prisma.posts.update({
      where: { id: command.postId },
      data: {
        title: command.title,
        content: command.content,
        updated_at: new Date(),
      },
    });
  }
}
