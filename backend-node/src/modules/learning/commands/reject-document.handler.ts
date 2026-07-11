import { CommandHandler, ICommandHandler } from '@nestjs/cqrs';
import { PrismaService } from '../../../common/prisma/prisma.service';
import { NotFoundException } from '@nestjs/common';

export class RejectDocumentCommand {
  constructor(
    public readonly documentId: string,
    public readonly moderatorId: string,
    public readonly reason: string,
  ) {}
}

@CommandHandler(RejectDocumentCommand)
export class RejectDocumentHandler implements ICommandHandler<RejectDocumentCommand> {
  constructor(private readonly prisma: PrismaService) {}

  async execute(command: RejectDocumentCommand) {
    const document = await this.prisma.documents.findUnique({
      where: { id: command.documentId },
    });

    if (!document) {
      throw new NotFoundException(`Document with ID ${command.documentId} not found`);
    }

    const updatedDocument = await this.prisma.documents.update({
      where: { id: command.documentId },
      data: {
        status: 4, // Rejected
        reviewer_id: command.moderatorId,
        reviewed_at: new Date(),
        rejection_reason: command.reason,
      },
    });

    return {
      ...updatedDocument,
      file_size: Number(updatedDocument.file_size),
    };
  }
}
