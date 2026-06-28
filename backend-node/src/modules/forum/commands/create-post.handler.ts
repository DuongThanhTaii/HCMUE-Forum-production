import { CommandHandler, ICommandHandler } from '@nestjs/cqrs';
import { PrismaService } from '../../../common/prisma/prisma.service';
import * as crypto from 'crypto';

export class CreatePostCommand {
  constructor(
    public readonly title: string,
    public readonly content: string,
    public readonly type: number,
    public readonly status: number,
    public readonly authorId: string,
    public readonly tags: any,
    public readonly categoryId?: string,
    public readonly threadChannelId?: string,
  ) {}
}

@CommandHandler(CreatePostCommand)
export class CreatePostHandler implements ICommandHandler<CreatePostCommand> {
  constructor(private readonly prisma: PrismaService) {}

  async execute(command: CreatePostCommand) {
    const baseSlug = command.title
      .toLowerCase()
      .replace(/[^a-z0-9]+/g, '-')
      .replace(/(^-|-$)+/g, '');
    const slug = `${baseSlug}-${crypto.randomBytes(4).toString('hex')}`;

    return this.prisma.posts.create({
      data: {
        id: crypto.randomUUID(),
        title: command.title,
        content: command.content,
        slug,
        type: command.type,
        status: command.status,
        author_id: command.authorId,
        category_id: command.categoryId,
        thread_channel_id: command.threadChannelId,
        tags: command.tags,
        created_at: new Date(),
      },
    });
  }
}
