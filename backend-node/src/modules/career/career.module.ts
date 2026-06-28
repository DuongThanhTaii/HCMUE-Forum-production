import { Module } from '@nestjs/common';
import { CqrsModule } from '@nestjs/cqrs';
import { IdentityModule } from '../identity/identity.module';
import { LearningModule } from '../learning/learning.module';
import { ConfigModule } from '@nestjs/config';

import { CompaniesController } from './controllers/companies.controller';
import { JobPostingsController } from './controllers/job-postings.controller';
import { ApplicationsController } from './controllers/applications.controller';

import { CreateCompanyHandler } from './commands/create-company.handler';
import { CreateJobPostingHandler } from './commands/create-job-posting.handler';
import { ApplyToJobHandler } from './commands/apply-to-job.handler';
import { UpdateApplicationStatusHandler } from './commands/update-application-status.handler';
import { GetJobPostingsHandler } from './queries/get-job-postings.handler';
import { GetJobApplicationsHandler } from './queries/get-job-applications.handler';

const Handlers = [
  CreateCompanyHandler,
  CreateJobPostingHandler,
  ApplyToJobHandler,
  UpdateApplicationStatusHandler,
  GetJobPostingsHandler,
  GetJobApplicationsHandler,
];

@Module({
  imports: [IdentityModule, LearningModule, CqrsModule, ConfigModule],
  controllers: [
    CompaniesController,
    JobPostingsController,
    ApplicationsController,
  ],
  providers: [...Handlers],
})
export class CareerModule {}
