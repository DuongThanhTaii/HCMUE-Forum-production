import { Controller, Get, Patch, Post, Param, Query, UseGuards, Request, Body } from '@nestjs/common';
import { CommandBus, QueryBus } from '@nestjs/cqrs';
import { JwtAuthGuard } from '../../identity/guards/jwt-auth.guard';
import { RolesGuard } from '../../identity/guards/roles.guard';
import { Roles } from '../../../common/decorators/roles.decorator';
import { PrismaService } from '../../../common/prisma/prisma.service';
import * as crypto from 'crypto';

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
    private readonly prisma: PrismaService,
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
  async broadcast(@Body() body: { title: string; message: string; sendEmail?: boolean }) {
    const users = await this.prisma.users.findMany({ select: { id: true, email: true } });
    const now = new Date();
    
    const notifications = users.map(u => ({
      id: crypto.randomUUID(),
      recipient_id: u.id,
      channel: 1, // 1 = InApp
      status: 0, // 0 = Unread
      content_subject: body.title,
      content_body: body.message,
      content_action_url: '?announcement=true',
      metadata: {},
      created_at: now,
    }));
    
    await this.prisma.notifications.createMany({ data: notifications });
    
    let sentEmail = 0;
    if (body.sendEmail) {
       // Mock sending emails
       sentEmail = users.length;
    }
    
    return { 
      success: true, 
      message: 'Broadcast successful', 
      data: { sentInApp: users.length, sentEmail } 
    };
  }
}
