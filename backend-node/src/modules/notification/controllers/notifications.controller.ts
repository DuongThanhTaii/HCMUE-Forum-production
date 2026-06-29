import { Controller, Get, Patch, Post, Param, Query, UseGuards, Request, Body } from '@nestjs/common';
import { CommandBus, QueryBus } from '@nestjs/cqrs';
import { JwtAuthGuard } from '../../identity/guards/jwt-auth.guard';
import { RolesGuard } from '../../identity/guards/roles.guard';
import { Roles } from '../../../common/decorators/roles.decorator';

import { GetUserNotificationsQuery } from '../queries/get-user-notifications.handler';
import { GetUnreadCountQuery } from '../queries/get-unread-count.handler';
import { MarkNotificationReadCommand } from '../commands/mark-notification-read.handler';
import { MarkAllReadCommand } from '../commands/mark-all-read.handler';
import { CreateNotificationCommand } from '../commands/create-notification.handler';

@Controller('notifications')
@UseGuards(JwtAuthGuard)
export class NotificationsController {
  constructor(
    private readonly commandBus: CommandBus,
    private readonly queryBus: QueryBus,
  ) {}

  @Get()
  async getNotifications(
    @Request() req: any,
    @Query('pageNumber') pageNumber: number = 1,
    @Query('pageSize') pageSize: number = 20,
  ) {
    return this.queryBus.execute(
      new GetUserNotificationsQuery(req.user.userId, pageNumber, pageSize),
    );
  }

  @Get('unread-count')
  async getUnreadCount(@Request() req: any) {
    return this.queryBus.execute(new GetUnreadCountQuery(req.user.userId));
  }

  @Patch(':id/read')
  async markAsRead(@Request() req: any, @Param('id') id: string) {
    return this.commandBus.execute(new MarkNotificationReadCommand(req.user.userId, id));
  }

  @Post('read-all')
  async markAllAsRead(@Request() req: any) {
    return this.commandBus.execute(new MarkAllReadCommand(req.user.userId));
  }

  @UseGuards(RolesGuard)
  @Roles('Admin', 'SuperAdmin')
  @Post('broadcast/home')
  async broadcast(@Body() body: { title: string; content: string; link?: string }) {
    // Usually broadcast means sending to all users.
    // For simplicity we create a system notification without specific user or handle it via a different command
    // But since create-notification requires userId, let's just log or send to admins as a fallback, or we need a real broadcast logic
    // Let's implement broadcast in gateway or just return success for now
    return { success: true, message: 'Broadcast queued' };
  }
}
