import { CommandHandler, ICommandHandler } from '@nestjs/cqrs';
import { PrismaService } from '../../../common/prisma/prisma.service';
import { NotFoundException } from '@nestjs/common';

export class PublishPostCommand {
  constructor(public readonly id: string) {}
}

@CommandHandler(PublishPostCommand)
export class PublishPostHandler implements ICommandHandler<PublishPostCommand> {
  constructor(private readonly prisma: PrismaService) {}

  async execute(command: PublishPostCommand) {
    const post = await this.prisma.posts.findUnique({
      where: { id: command.id },
    });
    
    if (!post) {
      throw new NotFoundException('Post not found');
    }

    await this.prisma.posts.update({
      where: { id: command.id },
      data: { 
        status: 2, // 2 = Published
        published_at: new Date() 
      },
    });

    return { success: true };
  }
}
