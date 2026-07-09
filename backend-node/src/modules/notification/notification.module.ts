import { Module } from '@nestjs/common';
import { CqrsModule } from '@nestjs/cqrs';
import { IdentityModule } from '../identity/identity.module';
import { NotificationGateway } from './gateways/notification.gateway';
import { NotificationsController } from './controllers/notifications.controller';
import { CreateNotificationHandler } from './commands/create-notification.handler';
import { MarkNotificationReadHandler } from './commands/mark-notification-read.handler';
import { MarkAllReadHandler } from './commands/mark-all-read.handler';
import { UpsertChatNotificationHandler } from './commands/upsert-chat-notification.handler';
import { GetUserNotificationsHandler } from './queries/get-user-notifications.handler';
import { GetUnreadCountHandler } from './queries/get-unread-count.handler';

const CommandHandlers = [
  CreateNotificationHandler,
  MarkNotificationReadHandler,
  MarkAllReadHandler,
  UpsertChatNotificationHandler,
];

const QueryHandlers = [
  GetUserNotificationsHandler,
  GetUnreadCountHandler,
];

@Module({
  imports: [IdentityModule, CqrsModule],
  controllers: [NotificationsController],
  providers: [NotificationGateway, ...CommandHandlers, ...QueryHandlers],
})
export class NotificationModule {}
