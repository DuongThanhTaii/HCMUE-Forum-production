import { Test, TestingModule } from '@nestjs/testing';
import { INestApplication, ValidationPipe } from '@nestjs/common';
const request = require('supertest');
import { AppModule } from './../src/app.module';
import { AwsS3Service } from './../src/modules/learning/services/aws-s3.service';
import { JwtService } from '@nestjs/jwt';
import * as crypto from 'crypto';

describe('Learning Module (e2e)', () => {
  let app: INestApplication;
  let jwtService: JwtService;
  let token: string;
  let testUserId: string;

  const mockS3Service = {
    uploadDocument: jest
      .fn()
      .mockResolvedValue(
        'https://unihub-learning-docs.s3.us-east-1.amazonaws.com/documents/test.pdf',
      ),
  };

  beforeAll(async () => {
    const moduleFixture: TestingModule = await Test.createTestingModule({
      imports: [AppModule],
    })
      .overrideProvider(AwsS3Service)
      .useValue(mockS3Service)
      .compile();

    app = moduleFixture.createNestApplication();
    app.useGlobalPipes(new ValidationPipe({ transform: true }));
    await app.init();

    jwtService = app.get<JwtService>(JwtService);
    testUserId = crypto.randomUUID();
    token = jwtService.sign({ sub: testUserId });
  });

  afterAll(async () => {
    await app.close();
  });

  let createdFacultyId: string;
  let createdCourseId: string;
  let createdDocumentId: string;

  it('/faculties (POST) - should create a faculty', async () => {
    const response = await request(app.getHttpServer())
      .post('/faculties')
      .set('Authorization', `Bearer ${token}`)
      .send({
        code: `CS-${Date.now()}`,
        name: `Computer Science ${Date.now()}`,
        description: 'CS Faculty',
        status: 1,
      })
      .expect(201);

    expect(response.body).toHaveProperty('id');
    createdFacultyId = response.body.id;
  });

  it('/courses (POST) - should create a course linked to faculty', async () => {
    const response = await request(app.getHttpServer())
      .post('/courses')
      .set('Authorization', `Bearer ${token}`)
      .send({
        code: `CS101-${Date.now()}`,
        name: `Intro to CS ${Date.now()}`,
        description: 'Basic computer science',
        semester: 'Fall 2026',
        status: 1,
        credits: 3,
        facultyId: createdFacultyId,
      })
      .expect(201);

    expect(response.body).toHaveProperty('id');
    expect(response.body.faculty_id).toBe(createdFacultyId);
    createdCourseId = response.body.id;
  });

  it('/documents/upload (POST) - should mock S3 and create document', async () => {
    const response = await request(app.getHttpServer())
      .post('/documents/upload')
      .set('Authorization', `Bearer ${token}`)
      .attach('file', Buffer.from('fake pdf data'), 'test.pdf')
      .field('title', 'Syllabus')
      .field('description', 'Course syllabus')
      .field('type', '1')
      .field('status', '1')
      .field('courseId', createdCourseId)
      .expect(201);

    expect(response.body).toHaveProperty('id');
    expect(response.body.file_path).toBe(
      'https://unihub-learning-docs.s3.us-east-1.amazonaws.com/documents/test.pdf',
    );
    createdDocumentId = response.body.id;
  });

  it('/documents/:id/rate (POST) - should reject rating > 5', async () => {
    const response = await request(app.getHttpServer())
      .post(`/documents/${createdDocumentId}/rate`)
      .set('Authorization', `Bearer ${token}`)
      .send({ rating: 6 })
      .expect(400);

    expect(response.body.message).toEqual(
      expect.arrayContaining([
        expect.stringContaining('rating must not be greater than 5'),
      ]),
    );
  });

  it('/documents/:id/rate (POST) - should reject rating < 1', async () => {
    const response = await request(app.getHttpServer())
      .post(`/documents/${createdDocumentId}/rate`)
      .set('Authorization', `Bearer ${token}`)
      .send({ rating: 0 })
      .expect(400);

    expect(response.body.message).toEqual(
      expect.arrayContaining([
        expect.stringContaining('rating must not be less than 1'),
      ]),
    );
  });

  it('/documents/:id/rate (POST) - should accept valid rating and calculate average', async () => {
    const response = await request(app.getHttpServer())
      .post(`/documents/${createdDocumentId}/rate`)
      .set('Authorization', `Bearer ${token}`)
      .send({ rating: 4 })
      .expect(201);

    expect(response.body.averageRating).toBe(4);
    expect(response.body.ratingCount).toBe(1);
  });
});
