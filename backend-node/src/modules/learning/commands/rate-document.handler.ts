import { CommandHandler, ICommandHandler } from '@nestjs/cqrs';
import { PrismaService } from '../../../common/prisma/prisma.service';

export class RateDocumentCommand {
  constructor(
    public readonly documentId: string,
    public readonly userId: string,
    public readonly rating: number,
  ) {}
}

@CommandHandler(RateDocumentCommand)
export class RateDocumentHandler implements ICommandHandler<RateDocumentCommand> {
  constructor(private readonly prisma: PrismaService) {}

  async execute(command: RateDocumentCommand) {
    const existingRating = await this.prisma.user_document_ratings.findUnique({
      where: {
        user_id_document_id: {
          user_id: command.userId,
          document_id: command.documentId,
        },
      },
    });

    if (existingRating) {
      await this.prisma.user_document_ratings.update({
        where: {
          user_id_document_id: {
            user_id: command.userId,
            document_id: command.documentId,
          },
        },
        data: { rating: command.rating, rated_at: new Date() },
      });
    } else {
      await this.prisma.user_document_ratings.create({
        data: {
          user_id: command.userId,
          document_id: command.documentId,
          rating: command.rating,
          rated_at: new Date(),
        },
      });
    }

    const result = await this.prisma.user_document_ratings.aggregate({
      where: { document_id: command.documentId },
      _avg: { rating: true },
      _count: { rating: true },
    });

    const averageRating = result._avg.rating || 0;
    const ratingCount = result._count.rating || 0;

    await this.prisma.documents.update({
      where: { id: command.documentId },
      data: {
        average_rating: averageRating,
        rating_count: ratingCount,
      },
    });

    return { averageRating, ratingCount };
  }
}
