import {
  Controller,
  Post,
  Get,
  Patch,
  Body,
  Param,
  UseGuards,
  Req,
  UseInterceptors,
  UploadedFile,
  BadRequestException,
} from '@nestjs/common';
import { CommandBus, QueryBus } from '@nestjs/cqrs';
import { FileInterceptor } from '@nestjs/platform-express';
import { JwtAuthGuard } from '../../identity/guards/jwt-auth.guard';
import { ApplyToJobCommand } from '../commands/apply-to-job.handler';
import { UpdateApplicationStatusCommand } from '../commands/update-application-status.handler';
import { GetJobApplicationsQuery } from '../queries/get-job-applications.handler';

@Controller('applications')
@UseGuards(JwtAuthGuard)
export class ApplicationsController {
  constructor(
    private readonly commandBus: CommandBus,
    private readonly queryBus: QueryBus,
  ) {}

  @Get('job/:jobPostingId')
  async getApplications(@Param('jobPostingId') jobPostingId: string) {
    return this.queryBus.execute(new GetJobApplicationsQuery(jobPostingId));
  }

  @Post('apply')
  @UseInterceptors(FileInterceptor('file'))
  async applyToJob(
    @UploadedFile() file: Express.Multer.File,
    @Body() dto: any,
    @Req() req: any,
  ) {
    if (!file) throw new BadRequestException('Resume file is required');
    return this.commandBus.execute(
      new ApplyToJobCommand(
        dto.jobPostingId,
        req.user.userId,
        parseInt(dto.status),
        file,
      ),
    );
  }

  @Patch(':id/status')
  async updateStatus(
    @Param('id') id: string,
    @Body() dto: any,
    @Req() req: any,
  ) {
    return this.commandBus.execute(
      new UpdateApplicationStatusCommand(id, dto.status, req.user.userId),
    );
  }
}
