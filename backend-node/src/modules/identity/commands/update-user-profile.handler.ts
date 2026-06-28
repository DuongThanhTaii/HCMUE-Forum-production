import { CommandHandler, ICommandHandler } from '@nestjs/cqrs';
import { UpdateUserProfileCommand } from './update-user-profile.command';
import { PrismaService } from '../../../common/prisma/prisma.service';

@CommandHandler(UpdateUserProfileCommand)
export class UpdateUserProfileHandler implements ICommandHandler<UpdateUserProfileCommand> {
  constructor(private readonly prisma: PrismaService) {}

  async execute(command: UpdateUserProfileCommand): Promise<any> {
    return this.prisma.users.update({
      where: { id: command.userId },
      data: {
        first_name: command.firstName,
        last_name: command.lastName,
        bio: command.bio,
        phone: command.phone,
        updated_at: new Date(),
      },
      select: {
        id: true,
        email: true,
        first_name: true,
        last_name: true,
        bio: true,
      },
    });
  }
}
