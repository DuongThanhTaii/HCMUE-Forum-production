import { CommandHandler, ICommandHandler } from '@nestjs/cqrs';
import { BlockUserCommand } from './block-user.command';
import { PrismaService } from '../../../common/prisma/prisma.service';

@CommandHandler(BlockUserCommand)
export class BlockUserHandler implements ICommandHandler<BlockUserCommand> {
  constructor(private readonly prisma: PrismaService) {}

  async execute(command: BlockUserCommand): Promise<any> {
    return this.prisma.users.update({
      where: { id: command.userId },
      data: {
        status: 0, // 0 = Inactive / Blocked
        updated_at: new Date(),
      },
      select: {
        id: true,
        email: true,
        status: true,
      },
    });
  }
}
