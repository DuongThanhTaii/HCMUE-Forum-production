import { CommandHandler, ICommandHandler } from '@nestjs/cqrs';
import { PrismaService } from '../../../common/prisma/prisma.service';

export class BulkDeleteDocumentsCommand {
  constructor(
    public readonly documentIds: string[],
    public readonly requesterId: string,
  ) {}
}

@CommandHandler(BulkDeleteDocumentsCommand)
export class BulkDeleteDocumentsHandler implements ICommandHandler<BulkDeleteDocumentsCommand> {
  constructor(private readonly prisma: PrismaService) {}

  async execute(command: BulkDeleteDocumentsCommand) {
    if (!command.documentIds || command.documentIds.length === 0) {
      return { count: 0 };
    }

    // Hard delete the documents
    const result = await this.prisma.documents.deleteMany({
      where: {
        id: {
          in: command.documentIds,
        },
      },
    });

    return { count: result.count };
  }
}
