import { CommandHandler, ICommandHandler } from '@nestjs/cqrs';
import { PrismaService } from '../../../common/prisma/prisma.service';

export class RecordDocumentDownloadCommand {
  constructor(
    public readonly documentId: string,
    public readonly userId: string,
  ) {}
}

@CommandHandler(RecordDocumentDownloadCommand)
export class RecordDocumentDownloadHandler implements ICommandHandler<RecordDocumentDownloadCommand> {
  constructor(private readonly prisma: PrismaService) {}

  async execute(command: RecordDocumentDownloadCommand) {
    await this.prisma.user_document_downloads.upsert({
      where: {
        user_id_document_id: {
          user_id: command.userId,
          document_id: command.documentId,
        },
      },
      update: { downloaded_at: new Date() },
      create: {
        user_id: command.userId,
        document_id: command.documentId,
        downloaded_at: new Date(),
      },
    });

    return this.prisma.documents.update({
      where: { id: command.documentId },
      data: { download_count: { increment: 1 } },
    });
  }
}
