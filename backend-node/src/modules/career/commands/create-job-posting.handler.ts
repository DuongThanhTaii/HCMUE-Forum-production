import { CommandHandler, ICommandHandler } from '@nestjs/cqrs';
import { PrismaService } from '../../../common/prisma/prisma.service';
import * as crypto from 'crypto';

export class CreateJobPostingCommand {
  constructor(
    public readonly title: string,
    public readonly description: string,
    public readonly companyId: string,
    public readonly jobType: number,
    public readonly experienceLevel: number,
    public readonly status: number,
    public readonly locationCity: string,
    public readonly postedBy: string,
  ) {}
}

@CommandHandler(CreateJobPostingCommand)
export class CreateJobPostingHandler implements ICommandHandler<CreateJobPostingCommand> {
  constructor(private readonly prisma: PrismaService) {}

  async execute(command: CreateJobPostingCommand) {
    const job = await this.prisma.job_postings.create({
      data: {
        id: crypto.randomUUID(),
        title: command.title,
        description: command.description,
        company_id: command.companyId,
        posted_by: command.postedBy,
        job_type: command.jobType,
        experience_level: command.experienceLevel,
        status: command.status,
        location_city: command.locationCity,
        created_at: new Date(),
        tags: [],
      },
    });

    await this.prisma.companies.update({
      where: { id: command.companyId },
      data: { total_job_postings: { increment: 1 } },
    });

    return job;
  }
}
