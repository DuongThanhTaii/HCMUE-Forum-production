import { CommandHandler, ICommandHandler } from '@nestjs/cqrs';
import { PrismaService } from '../../../common/prisma/prisma.service';
import { AwsS3Service } from '../../learning/services/aws-s3.service';
import { ConfigService } from '@nestjs/config';
import * as crypto from 'crypto';

export class ApplyToJobCommand {
  constructor(
    public readonly jobPostingId: string,
    public readonly applicantId: string,
    public readonly status: number,
    public readonly file: Express.Multer.File,
  ) {}
}

@CommandHandler(ApplyToJobCommand)
export class ApplyToJobHandler implements ICommandHandler<ApplyToJobCommand> {
  constructor(
    private readonly prisma: PrismaService,
    private readonly s3Service: AwsS3Service,
    private readonly configService: ConfigService,
  ) {}

  async execute(command: ApplyToJobCommand) {
    const bucketName =
      this.configService.get<string>('CAREER_S3_BUCKET_NAME') ||
      'unihub-career-resumes-private';
    const fileUrl = await this.s3Service.uploadDocument(
      command.file,
      'resumes',
      bucketName,
    );

    const application = await this.prisma.applications.create({
      data: {
        id: crypto.randomUUID(),
        job_posting_id: command.jobPostingId,
        applicant_id: command.applicantId,
        status: command.status,
        resume_file_name: command.file.originalname,
        resume_file_url: fileUrl,
        resume_file_size_bytes: BigInt(command.file.size),
        resume_content_type: command.file.mimetype,
        submitted_at: new Date(),
      },
    });

    await this.prisma.job_postings.update({
      where: { id: command.jobPostingId },
      data: { application_count: { increment: 1 } },
    });

    return {
      ...application,
      resume_file_size_bytes: application.resume_file_size_bytes.toString(),
    };
  }
}
