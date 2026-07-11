import { CommandHandler, ICommandHandler } from '@nestjs/cqrs';
import { PrismaService } from '../../../common/prisma/prisma.service';

export class BulkApproveDocumentsCommand {
  constructor(
    public readonly documentIds: string[],
    public readonly moderatorId: string,
  ) {}
}

@CommandHandler(BulkApproveDocumentsCommand)
export class BulkApproveDocumentsHandler implements ICommandHandler<BulkApproveDocumentsCommand> {
  constructor(private readonly prisma: PrismaService) {}

  async execute(command: BulkApproveDocumentsCommand) {
    if (!command.documentIds || command.documentIds.length === 0) {
      return { count: 0 };
    }

    const result = await this.prisma.documents.updateMany({
      where: {
        id: {
          in: command.documentIds,
        },
      },
      data: {
        status: 3, // Approved
        reviewer_id: command.moderatorId,
        reviewed_at: new Date(),
      },
    });

    return { count: result.count };
  }
}
