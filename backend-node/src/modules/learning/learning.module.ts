import { Module } from '@nestjs/common';
import { CqrsModule } from '@nestjs/cqrs';
import { IdentityModule } from '../identity/identity.module';
import { AwsS3Service } from './services/aws-s3.service';
import { FacultiesController } from './controllers/faculties.controller';
import { CoursesController } from './controllers/courses.controller';
import { DocumentsController } from './controllers/documents.controller';
import { CreateFacultyHandler } from './commands/create-faculty.handler';
import { CreateCourseHandler } from './commands/create-course.handler';
import { UploadDocumentHandler } from './commands/upload-document.handler';
import { RateDocumentHandler } from './commands/rate-document.handler';
import { RecordDocumentDownloadHandler } from './commands/record-document-download.handler';
import { GetFacultiesHandler } from './queries/get-faculties.handler';
import { GetCoursesHandler } from './queries/get-courses.handler';
import { GetCourseDocumentsHandler } from './queries/get-course-documents.handler';
import { GetDocumentsHandler } from './queries/get-documents.handler';

const Handlers = [
  CreateFacultyHandler,
  CreateCourseHandler,
  UploadDocumentHandler,
  RateDocumentHandler,
  RecordDocumentDownloadHandler,
  GetFacultiesHandler,
  GetCoursesHandler,
  GetCourseDocumentsHandler,
  GetDocumentsHandler,
];

@Module({
  imports: [IdentityModule, CqrsModule],
  controllers: [FacultiesController, CoursesController, DocumentsController],
  providers: [...Handlers, AwsS3Service],
  exports: [AwsS3Service],
})
export class LearningModule {}
