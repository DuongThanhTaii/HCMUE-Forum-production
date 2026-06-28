import { Test, TestingModule } from '@nestjs/testing';
import { INestApplication, ValidationPipe } from '@nestjs/common';
const request = require('supertest');
import { AppModule } from './../src/app.module';
import { PrismaService } from '../src/common/prisma/prisma.service';
import { JwtService } from '@nestjs/jwt';
import { AwsS3Service } from '../src/modules/learning/services/aws-s3.service';

describe('Career Module (e2e)', () => {
  let app: INestApplication;
  let prisma: PrismaService;
  let jwtService: JwtService;
  let s3Service: AwsS3Service;

  let token: string;
  let userId: string;
  let companyId: string;
  let jobPostingId: string;
  let applicationId: string;

  beforeAll(async () => {
    const moduleFixture: TestingModule = await Test.createTestingModule({
      imports: [AppModule],
    }).compile();

    app = moduleFixture.createNestApplication();
    app.useGlobalPipes(new ValidationPipe({ transform: true }));
    await app.init();

    prisma = app.get<PrismaService>(PrismaService);
    jwtService = app.get<JwtService>(JwtService);
    s3Service = app.get<AwsS3Service>(AwsS3Service);

    // Mock S3 Upload
    jest
      .spyOn(s3Service, 'uploadDocument')
      .mockResolvedValue(
        'https://unihub-career-resumes-private.s3.us-east-1.amazonaws.com/resumes/mock-resume.pdf',
      );

    // Setup Test User
    userId = '12345678-1234-1234-1234-1234567890ab';
    await prisma.users.upsert({
      where: { id: userId },
      update: {},
      create: {
        id: userId,
        email: 'career.test@unihub.com',
        password_hash: 'hash',
        first_name: 'Career',
        last_name: 'Test',
        status: 1,
        created_at: new Date(),
      },
    });

    token = jwtService.sign({ sub: userId });
  });

  afterAll(async () => {
    await app.close();
  });

  it('/companies (POST) - should create a company', async () => {
    const response = await request(app.getHttpServer())
      .post('/companies')
      .set('Authorization', `Bearer ${token}`)
      .send({
        name: `Tech Corp ${Date.now()}`,
        description: 'A great place to work',
        industry: 1,
        size: 2,
        contactEmail: 'contact@techcorp.com',
        status: 1,
      })
      .expect(201);

    expect(response.body).toHaveProperty('id');
    companyId = response.body.id;
  });

  it('/job-postings (POST) - should create a job posting linked to company', async () => {
    const response = await request(app.getHttpServer())
      .post('/job-postings')
      .set('Authorization', `Bearer ${token}`)
      .send({
        title: `Software Engineer ${Date.now()}`,
        description: 'We are looking for a Node.js developer.',
        companyId: companyId,
        jobType: 1,
        experienceLevel: 2,
        status: 1,
        locationCity: 'HCMC',
      })
      .expect(201);

    expect(response.body).toHaveProperty('id');
    jobPostingId = response.body.id;

    // Verify company total_job_postings was incremented
    const company = await prisma.companies.findUnique({
      where: { id: companyId },
    });
    expect(company?.total_job_postings).toBe(1);
  });

  it('/applications/apply (POST) - should submit application with S3 mock', async () => {
    const response = await request(app.getHttpServer())
      .post('/applications/apply')
      .set('Authorization', `Bearer ${token}`)
      .attach('file', Buffer.from('mock pdf content'), 'resume.pdf')
      .field('jobPostingId', jobPostingId)
      .field('status', '0')
      .expect(201);

    expect(response.body).toHaveProperty('id');
    expect(response.body.resume_file_url).toContain('mock-resume.pdf');
    applicationId = response.body.id;

    // Verify job posting application_count was incremented
    const job = await prisma.job_postings.findUnique({
      where: { id: jobPostingId },
    });
    expect(job?.application_count).toBe(1);
  });

  it('/applications/:id/status (PATCH) - should update application status', async () => {
    const response = await request(app.getHttpServer())
      .patch(`/applications/${applicationId}/status`)
      .set('Authorization', `Bearer ${token}`)
      .send({ status: 1 }) // 1 = Interviewing
      .expect(200);

    expect(response.body.status).toBe(1);
  });
});
