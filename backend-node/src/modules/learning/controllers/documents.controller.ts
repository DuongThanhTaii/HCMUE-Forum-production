import {
  Controller,
  Post,
  Get,
  Query,
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
import { UploadDocumentCommand } from '../commands/upload-document.handler';
import { RateDocumentCommand } from '../commands/rate-document.handler';
import { RecordDocumentDownloadCommand } from '../commands/record-document-download.handler';
import { GetCourseDocumentsQuery } from '../queries/get-course-documents.handler';
import { GetDocumentsQuery } from '../queries/get-documents.handler';
import { RateDocumentDto } from '../dtos/rate-document.dto';
import { GetSystemSettingQuery } from '../../admin/queries/get-system-setting.handler';

@Controller('documents')
export class DocumentsController {
  constructor(
    private readonly commandBus: CommandBus,
    private readonly queryBus: QueryBus,
  ) {}

  @Get()
  async getDocuments(
    @Query('pageNumber') pageNumber?: string,
    @Query('pageSize') pageSize?: string,
  ) {
    const page = parseInt(pageNumber || '') || 1;
    const size = parseInt(pageSize || '') || 5;
    return this.queryBus.execute(new GetDocumentsQuery(page, size));
  }

  @Get('course/:courseId')
  async getCourseDocuments(@Param('courseId') courseId: string) {
    return this.queryBus.execute(new GetCourseDocumentsQuery(courseId));
  }

  @Post('upload')
  @UseGuards(JwtAuthGuard)
  @UseInterceptors(FileInterceptor('file'))
  async uploadDocument(
    @UploadedFile() file: Express.Multer.File,
    @Body() dto: any,
    @Req() req: any,
  ) {
    if (!file && !dto.driveUrl) {
      throw new BadRequestException('Either file or driveUrl must be provided');
    }

    if (file) {
      // Check if file upload is maintained/enabled by checking system settings
      const maintainFileUpload = await this.queryBus.execute(new GetSystemSettingQuery('MAINTAIN_FILE_UPLOAD'));
      if (maintainFileUpload === 'false') {
        throw new BadRequestException('File upload is currently disabled by admin. Please use a Google Drive link (driveUrl) instead.');
      }
    }

    return this.commandBus.execute(
      new UploadDocumentCommand(
        dto.title,
        dto.description,
        parseInt(dto.type),
        parseInt(dto.status),
        req.user.userId,
        file,
        dto.courseId,
        dto.driveUrl,
      ),
    );
  }


  @Post(':id/rate')
  @UseGuards(JwtAuthGuard)
  async rateDocument(
    @Param('id') id: string,
    @Body() dto: RateDocumentDto,
    @Req() req: any,
  ) {
    return this.commandBus.execute(
      new RateDocumentCommand(id, req.user.userId, dto.rating),
    );
  }

  @Post(':id/download')
  @UseGuards(JwtAuthGuard)
  async recordDownload(@Param('id') id: string, @Req() req: any) {
    return this.commandBus.execute(
      new RecordDocumentDownloadCommand(id, req.user.userId),
    );
  }
}
