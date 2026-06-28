import { CommandHandler, ICommandHandler } from '@nestjs/cqrs';
import { PrismaService } from '../../../common/prisma/prisma.service';

export class UpdateApplicationStatusCommand {
  constructor(
    public readonly applicationId: string,
    public readonly status: number,
    public readonly updatedBy: string,
  ) {}
}

@CommandHandler(UpdateApplicationStatusCommand)
export class UpdateApplicationStatusHandler implements ICommandHandler<UpdateApplicationStatusCommand> {
  constructor(private readonly prisma: PrismaService) {}

  async execute(command: UpdateApplicationStatusCommand) {
    const updated = await this.prisma.applications.update({
      where: { id: command.applicationId },
      data: {
        status: command.status,
        last_status_changed_at: new Date(),
        last_status_changed_by: command.updatedBy,
      },
    });

    return {
      ...updated,
      resume_file_size_bytes: updated.resume_file_size_bytes.toString(),
    };
  }
}
