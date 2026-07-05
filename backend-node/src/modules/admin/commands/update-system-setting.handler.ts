import { CommandHandler, ICommandHandler } from '@nestjs/cqrs';
import { PrismaService } from '../../../common/prisma/prisma.service';

export class UpdateSystemSettingCommand {
  constructor(
    public readonly key: string,
    public readonly value: string,
    public readonly description?: string,
    public readonly adminId?: string,
  ) {}
}

@CommandHandler(UpdateSystemSettingCommand)
export class UpdateSystemSettingHandler implements ICommandHandler<UpdateSystemSettingCommand> {
  constructor(private readonly prisma: PrismaService) {}

  async execute(command: UpdateSystemSettingCommand) {
    const setting = await this.prisma.system_settings.upsert({
      where: { key: command.key },
      update: {
        value: command.value,
        description: command.description,
        updated_at: new Date(),
        updated_by: command.adminId,
      },
      create: {
        key: command.key,
        value: command.value,
        description: command.description,
        updated_at: new Date(),
        updated_by: command.adminId,
      },
    });

    return setting;
  }
}
