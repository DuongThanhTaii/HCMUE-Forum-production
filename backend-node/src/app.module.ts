import { Module } from '@nestjs/common';
import { ConfigModule } from '@nestjs/config';
import { LoggerModule } from 'nestjs-pino';
import { AppController } from './app.controller';
import { AppService } from './app.service';
import { PrismaModule } from './common/prisma/prisma.module';
import { IdentityModule } from './modules/identity/identity.module';
import { NotificationModule } from './modules/notification/notification.module';
import { ChatModule } from './modules/chat/chat.module';
import { MediaModule } from './modules/media/media.module';
import { ForumModule } from './modules/forum/forum.module';
import { LearningModule } from './modules/learning/learning.module';
import { CareerModule } from './modules/career/career.module';

@Module({
  imports: [
    ConfigModule.forRoot({
      isGlobal: true,
    }),
    LoggerModule.forRoot({
      pinoHttp: {
        transport: {
          target: 'pino-pretty',
          options: {
            singleLine: true,
          },
        },
      },
    }),
    PrismaModule,
    IdentityModule,
    NotificationModule,
    ChatModule,
    MediaModule,
    ForumModule,
    LearningModule,
    CareerModule,
  ],
  controllers: [AppController],
  providers: [AppService],
})
export class AppModule {}
