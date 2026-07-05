import { CommandHandler, ICommandHandler } from '@nestjs/cqrs';
import { PrismaService } from '../../../common/prisma/prisma.service';
import { AwsS3Service } from '../services/aws-s3.service';
import * as crypto from 'crypto';

export class UploadDocumentCommand {
  constructor(
    public readonly title: string,
    public readonly description: string,
    public readonly type: number,
    public readonly status: number,
    public readonly uploaderId: string,
    public readonly file?: Express.Multer.File,
    public readonly courseId?: string,
    public readonly driveUrl?: string,
  ) {}
}

@CommandHandler(UploadDocumentCommand)
export class UploadDocumentHandler implements ICommandHandler<UploadDocumentCommand> {
  constructor(
    private readonly prisma: PrismaService,
    private readonly s3Service: AwsS3Service,
  ) {}

  async execute(command: UploadDocumentCommand) {
    let fileUrl = '';
    let fileName = '';
    let fileSize = BigInt(0);
    let contentType = '';
    let fileExtension = '';

    if (command.driveUrl) {
      fileUrl = command.driveUrl;
      fileName = command.title;
      fileSize = BigInt(0);
      contentType = 'link/drive';
      fileExtension = 'link';
    } else if (command.file) {
      fileUrl = await this.s3Service.uploadDocument(command.file);
      fileName = command.file.originalname;
      fileSize = BigInt(command.file.size);
      contentType = command.file.mimetype;
      fileExtension = command.file.originalname.split('.').pop() || '';
    } else {
      throw new Error('Either file or driveUrl must be provided');
    }

    const document = await this.prisma.documents.create({
      data: {
        id: crypto.randomUUID(),
        title: command.title,
        description: command.description,
        file_name: fileName,
        file_path: fileUrl,
        file_size: fileSize,
        content_type: contentType,
        file_extension: fileExtension,
        type: command.type,
        status: command.status,
        uploader_id: command.uploaderId,
        course_id: command.courseId,
        created_at: new Date(),
      },
    });

    if (command.courseId) {
      await this.prisma.courses.update({
        where: { id: command.courseId },
        data: { document_count: { increment: 1 } },
      });
    }

    return {
      ...document,
      file_size: document.file_size.toString(),
    };
  }
}

