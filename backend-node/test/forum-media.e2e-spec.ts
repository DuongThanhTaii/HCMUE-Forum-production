import { Test, TestingModule } from '@nestjs/testing';
import { INestApplication } from '@nestjs/common';
const request = require('supertest');
import { AppModule } from './../src/app.module';
import { CloudinaryService } from './../src/modules/media/services/cloudinary.service';
import { JwtService } from '@nestjs/jwt';
import * as crypto from 'crypto';

describe('Forum & Media Modules (e2e)', () => {
  let app: INestApplication;
  let jwtService: JwtService;
  let token: string;
  let testUserId: string;

  const mockCloudinaryService = {
    uploadFile: jest.fn().mockResolvedValue({
      secure_url: 'https://res.cloudinary.com/demo/image/upload/v1/test.png',
    }),
  };

  beforeAll(async () => {
    const moduleFixture: TestingModule = await Test.createTestingModule({
      imports: [AppModule],
    })
      .overrideProvider(CloudinaryService)
      .useValue(mockCloudinaryService)
      .compile();

    app = moduleFixture.createNestApplication();
    await app.init();

    jwtService = app.get<JwtService>(JwtService);
    testUserId = crypto.randomUUID();
    token = jwtService.sign({ sub: testUserId });
  });

  afterAll(async () => {
    await app.close();
  });

  it('/media/upload (POST) - should mock upload and return URL', async () => {
    const response = await request(app.getHttpServer())
      .post('/media/upload')
      .set('Authorization', `Bearer ${token}`)
      .attach('file', Buffer.from('fake image data'), 'test.png')
      .expect(201);

    expect(response.body.url).toBe(
      'https://res.cloudinary.com/demo/image/upload/v1/test.png',
    );
    expect(mockCloudinaryService.uploadFile).toHaveBeenCalled();
  });

  it('/posts (POST) - should create a new post', async () => {
    const postPayload = {
      title: 'My First Post',
      content:
        'This is the content with a [link](https://res.cloudinary.com/demo/image/upload/v1/test.png)',
      type: 1,
      status: 1,
      tags: { topics: ['nest', 'migration'] },
    };

    const response = await request(app.getHttpServer())
      .post('/posts')
      .set('Authorization', `Bearer ${token}`)
      .send(postPayload)
      .expect(201);

    expect(response.body).toHaveProperty('id');
    expect(response.body.title).toBe('My First Post');
    expect(response.body.slug).toContain('my-first-post');
  });
});
