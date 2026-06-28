import { Module } from '@nestjs/common';
import { CqrsModule } from '@nestjs/cqrs';
import { IdentityModule } from '../identity/identity.module';
import { NotificationGateway } from './gateways/notification.gateway';
import { CreateNotificationHandler } from './commands/create-notification.handler';
import { MarkNotificationReadHandler } from './commands/mark-notification-read.handler';
import { GetUserNotificationsHandler } from './queries/get-user-notifications.handler';

const Handlers = [
  CreateNotificationHandler,
  MarkNotificationReadHandler,
  GetUserNotificationsHandler,
];

@Module({
  imports: [IdentityModule, CqrsModule],
  providers: [NotificationGateway, ...Handlers],
})
export class NotificationModule {}
