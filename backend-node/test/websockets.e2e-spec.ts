import { Test, TestingModule } from '@nestjs/testing';
import { INestApplication } from '@nestjs/common';
import { AppModule } from './../src/app.module';
import { RedisIoAdapter } from './../src/common/websockets/redis-io.adapter';
import { io, Socket } from 'socket.io-client';
import { JwtService } from '@nestjs/jwt';
import { ChatGateway } from './../src/modules/chat/gateways/chat.gateway';
import * as crypto from 'crypto';

describe('WebSockets & Redis Infrastructure (e2e)', () => {
  let app: INestApplication;
  let jwtService: JwtService;
  let chatGateway: ChatGateway;
  let chatSocket: Socket;

  beforeAll(async () => {
    const moduleFixture: TestingModule = await Test.createTestingModule({
      imports: [AppModule],
    }).compile();

    app = moduleFixture.createNestApplication();

    // Wire up the Redis Adapter exactly as we do in main.ts
    const redisIoAdapter = new RedisIoAdapter(app);
    await redisIoAdapter.connectToRedis();
    app.useWebSocketAdapter(redisIoAdapter);

    await app.init();
    await app.listen(0); // Bind to random free port

    jwtService = app.get<JwtService>(JwtService);
    chatGateway = app.get<ChatGateway>(ChatGateway);
  });

  afterAll(async () => {
    if (chatSocket) chatSocket.disconnect();
    await app.close();
  });

  it('should authenticate via JWT, join a conversation room, and receive broadcasted events from Redis adapter', (done) => {
    // 1. Generate valid mock JWT token
    const testUserId = crypto.randomUUID();
    const token = jwtService.sign({ sub: testUserId });

    const httpServer = app.getHttpServer();
    const address = httpServer.address();
    const port = address.port;

    // 2. Connect client to the /chat namespace
    chatSocket = io(`http://localhost:${port}/chat`, {
      auth: { token: `Bearer ${token}` },
    });

    const testConversationId = crypto.randomUUID();

    chatSocket.on('connect', () => {
      // 3. Client joins a conversation room
      chatSocket.emit('join_conversation', {
        conversationId: testConversationId,
      });

      // 4. Simulate a slight delay to ensure Redis sub is active and joined
      setTimeout(() => {
        // Broadcast via gateway to simulate what a CommandHandler does
        chatGateway.broadcastNewMessage(testConversationId, {
          text: 'Hello Distributed Systems!',
        });
      }, 500);
    });

    // 5. Assert client correctly receives the message
    chatSocket.on('new_message', (msg) => {
      try {
        expect(msg.text).toBe('Hello Distributed Systems!');
        done();
      } catch (err) {
        done(err);
      }
    });
  });
});
