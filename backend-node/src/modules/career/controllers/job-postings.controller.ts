import {
  Controller,
  Post,
  Get,
  Body,
  UseGuards,
  Req,
  Query,
} from '@nestjs/common';
import { CommandBus, QueryBus } from '@nestjs/cqrs';
import { JwtAuthGuard } from '../../identity/guards/jwt-auth.guard';
import { CreateJobPostingCommand } from '../commands/create-job-posting.handler';
import { GetJobPostingsQuery } from '../queries/get-job-postings.handler';

@Controller('jobs')
export class JobPostingsController {
  constructor(
    private readonly commandBus: CommandBus,
    private readonly queryBus: QueryBus,
  ) {}

  @Get()
  async getJobPostings(@Query('companyId') companyId?: string) {
    return this.queryBus.execute(new GetJobPostingsQuery(companyId));
  }

  @Post()
  @UseGuards(JwtAuthGuard)
  async createJobPosting(@Body() dto: any, @Req() req: any) {
    return this.commandBus.execute(
      new CreateJobPostingCommand(
        dto.title,
        dto.description,
        dto.companyId,
        dto.jobType,
        dto.experienceLevel,
        dto.status,
        dto.locationCity,
        req.user.userId,
      ),
    );
  }
}
