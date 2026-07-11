import { CommandHandler, ICommandHandler } from '@nestjs/cqrs';
import { PrismaService } from '../../../common/prisma/prisma.service';
import { NotFoundException } from '@nestjs/common';

export class ApproveDocumentCommand {
  constructor(
    public readonly documentId: string,
    public readonly moderatorId: string,
  ) {}
}

@CommandHandler(ApproveDocumentCommand)
export class ApproveDocumentHandler implements ICommandHandler<ApproveDocumentCommand> {
  constructor(private readonly prisma: PrismaService) {}

  async execute(command: ApproveDocumentCommand) {
    const document = await this.prisma.documents.findUnique({
      where: { id: command.documentId },
    });

    if (!document) {
      throw new NotFoundException(`Document with ID ${command.documentId} not found`);
    }

    const updatedDocument = await this.prisma.documents.update({
      where: { id: command.documentId },
      data: {
        status: 3, // Approved
        reviewer_id: command.moderatorId,
        reviewed_at: new Date(),
      },
    });

    return {
      ...updatedDocument,
      file_size: Number(updatedDocument.file_size),
    };
  }
}
