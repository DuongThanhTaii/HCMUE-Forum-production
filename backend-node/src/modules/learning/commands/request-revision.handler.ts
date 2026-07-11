import { CommandHandler, ICommandHandler } from '@nestjs/cqrs';
import { PrismaService } from '../../../common/prisma/prisma.service';
import { NotFoundException } from '@nestjs/common';

export class RequestRevisionCommand {
  constructor(
    public readonly documentId: string,
    public readonly moderatorId: string,
    public readonly comment: string,
  ) {}
}

@CommandHandler(RequestRevisionCommand)
export class RequestRevisionHandler implements ICommandHandler<RequestRevisionCommand> {
  constructor(private readonly prisma: PrismaService) {}

  async execute(command: RequestRevisionCommand) {
    const document = await this.prisma.documents.findUnique({
      where: { id: command.documentId },
    });

    if (!document) {
      throw new NotFoundException(`Document with ID ${command.documentId} not found`);
    }

    const updatedDocument = await this.prisma.documents.update({
      where: { id: command.documentId },
      data: {
        status: 1, // Draft (Return to author)
        reviewer_id: command.moderatorId,
        reviewed_at: new Date(),
        review_comment: command.comment,
      },
    });

    return {
      ...updatedDocument,
      file_size: Number(updatedDocument.file_size),
    };
  }
}
