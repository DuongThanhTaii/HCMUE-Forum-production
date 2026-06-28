import { Test, TestingModule } from '@nestjs/testing';
import { INestApplication, ValidationPipe } from '@nestjs/common';
const request = require('supertest');
import { AppModule } from '../src/app.module';
import { PrismaService } from '../src/common/prisma/prisma.service';

describe('Identity Module (e2e)', () => {
  let app: INestApplication;
  let prisma: PrismaService;

  const uniqueEmail = `testuser_${Date.now()}@example.com`;
  const password = 'TestPassword123!';
  let jwtToken = '';
  let userId = '';

  beforeAll(async () => {
    const moduleFixture: TestingModule = await Test.createTestingModule({
      imports: [AppModule],
    }).compile();

    app = moduleFixture.createNestApplication();
    app.useGlobalPipes(
      new ValidationPipe({ transform: true, whitelist: true }),
    );

    prisma = app.get<PrismaService>(PrismaService);
    await app.init();
  });

  afterAll(async () => {
    // Cleanup the created test user to not pollute DB
    if (userId) {
      // Must use raw query to bypass constraints or simply delete
      await prisma.users.delete({ where: { id: userId } }).catch(() => null);
    }
    await app?.close();
  });

  it('/auth/register (POST) - should create a new user', async () => {
    const res = await request(app.getHttpServer())
      .post('/auth/register')
      .send({
        email: uniqueEmail,
        password: password,
        firstName: 'Test',
        lastName: 'User',
      })
      .expect(201);

    expect(res.body).toHaveProperty('id');
    expect(res.body).toHaveProperty('email', uniqueEmail);
    expect(res.body).not.toHaveProperty('password_hash');

    userId = res.body.id;
  });

  it('/auth/register (POST) - should fail with duplicate email', async () => {
    await request(app.getHttpServer())
      .post('/auth/register')
      .send({
        email: uniqueEmail,
        password: password,
        firstName: 'Duplicate',
        lastName: 'User',
      })
      .expect(409); // ConflictException
  });

  it('/auth/login (POST) - should return JWT token', async () => {
    const res = await request(app.getHttpServer())
      .post('/auth/login')
      .send({
        email: uniqueEmail,
        password: password,
      })
      .expect(201);

    expect(res.body).toHaveProperty('access_token');
    jwtToken = res.body.access_token;
  });

  it('/users/me (GET) - should return current user profile', async () => {
    const res = await request(app.getHttpServer())
      .get('/users/me')
      .set('Authorization', `Bearer ${jwtToken}`)
      .expect(200);

    expect(res.body).toHaveProperty('id', userId);
    expect(res.body).toHaveProperty('email', uniqueEmail);
  });

  it('/auth/admin-dashboard (GET) - should fail without token', async () => {
    await request(app.getHttpServer()).get('/auth/admin-dashboard').expect(401);
  });

  it('/users (GET) - should forbid regular user from accessing Admin endpoint', async () => {
    // The test user created does NOT have Admin/SuperAdmin roles in the database.
    await request(app.getHttpServer())
      .get('/users')
      .set('Authorization', `Bearer ${jwtToken}`)
      .expect(403); // ForbiddenException due to RolesGuard
  });
});
