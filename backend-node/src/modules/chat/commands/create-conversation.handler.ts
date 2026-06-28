import { CommandHandler, ICommandHandler } from '@nestjs/cqrs';
import { PrismaService } from '../../../common/prisma/prisma.service';
import * as crypto from 'crypto';

export class CreateConversationCommand {
  constructor(
    public readonly type: number,
    public readonly createdBy: string,
    public readonly participants: string[],
    public readonly title?: string,
  ) {}
}

@CommandHandler(CreateConversationCommand)
export class CreateConversationHandler implements ICommandHandler<CreateConversationCommand> {
  constructor(private readonly prisma: PrismaService) {}

  async execute(command: CreateConversationCommand) {
    return this.prisma.conversations.create({
      data: {
        id: crypto.randomUUID(),
        type: command.type,
        title: command.title,
        created_by: command.createdBy,
        created_at: new Date(),
        participants: command.participants,
      },
    });
  }
}
