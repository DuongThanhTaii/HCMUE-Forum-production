import { IQueryHandler, QueryHandler } from '@nestjs/cqrs';
import { PrismaService } from '../../../common/prisma/prisma.service';
import { NotFoundException } from '@nestjs/common';

export class GetDocumentByIdQuery {
  constructor(public readonly id: string) {}
}

@QueryHandler(GetDocumentByIdQuery)
export class GetDocumentByIdHandler implements IQueryHandler<GetDocumentByIdQuery> {
  constructor(private readonly prisma: PrismaService) {}

  async execute(query: GetDocumentByIdQuery) {
    const document = await this.prisma.documents.findUnique({
      where: { id: query.id },
    });

    if (!document) {
      throw new NotFoundException(`Document with ID ${query.id} not found`);
    }

    return {
      ...document,
      file_size: Number(document.file_size),
    };
  }
}
