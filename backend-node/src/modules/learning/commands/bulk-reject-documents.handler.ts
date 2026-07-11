import { CommandHandler, ICommandHandler } from '@nestjs/cqrs';
import { PrismaService } from '../../../common/prisma/prisma.service';

export class BulkRejectDocumentsCommand {
  constructor(
    public readonly documentIds: string[],
    public readonly moderatorId: string,
    public readonly reason: string,
  ) {}
}

@CommandHandler(BulkRejectDocumentsCommand)
export class BulkRejectDocumentsHandler implements ICommandHandler<BulkRejectDocumentsCommand> {
  constructor(private readonly prisma: PrismaService) {}

  async execute(command: BulkRejectDocumentsCommand) {
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
        status: 4, // Rejected
        reviewer_id: command.moderatorId,
        reviewed_at: new Date(),
        rejection_reason: command.reason || 'Rejected by moderator',
      },
    });

    return { count: result.count };
  }
}
