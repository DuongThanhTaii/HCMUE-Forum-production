import { Controller, Get, Post, Put, Param, Body, UseGuards } from '@nestjs/common';
import { QueryBus } from '@nestjs/cqrs';
import * as crypto from 'crypto';
import { GetThreadChannelsQuery } from '../queries/get-thread-channels.handler';
import { JwtAuthGuard } from '../../identity/guards/jwt-auth.guard';
import { RolesGuard } from '../../identity/guards/roles.guard';
import { Roles } from '../../../common/decorators/roles.decorator';

@Controller('thread-channels')
export class ThreadChannelsController {
  constructor(private readonly queryBus: QueryBus) {}

  @Get()
  async getThreadChannels() {
    return this.queryBus.execute(new GetThreadChannelsQuery(false));
  }

  @UseGuards(JwtAuthGuard, RolesGuard)
  @Roles('Admin', 'SuperAdmin')
  @Get('admin')
  async getThreadChannelsAdmin() {
    return this.queryBus.execute(new GetThreadChannelsQuery(true));
  }

  @UseGuards(JwtAuthGuard, RolesGuard)
  @Roles('Admin', 'SuperAdmin')
  @Post()
  async createThreadChannel(@Body() body: any) {
    return {
      id: crypto.randomUUID(),
      code: body.code,
      name: body.name,
      description: body.description || '',
      displayOrder: body.displayOrder || 0,
      isActive: body.isActive ?? true,
      allowPinnedComments: body.allowPinnedComments ?? false,
      allowAcceptedAnswers: body.allowAcceptedAnswers ?? false,
      allowModeratorActions: body.allowModeratorActions ?? false,
    };
  }

  @UseGuards(JwtAuthGuard, RolesGuard)
  @Roles('Admin', 'SuperAdmin')
  @Put(':id')
  async updateThreadChannel(@Param('id') id: string, @Body() body: any) {
    return {
      id,
      code: body.code,
      name: body.name,
      description: body.description || '',
      displayOrder: body.displayOrder || 0,
      isActive: body.isActive ?? true,
      allowPinnedComments: body.allowPinnedComments ?? false,
      allowAcceptedAnswers: body.allowAcceptedAnswers ?? false,
      allowModeratorActions: body.allowModeratorActions ?? false,
    };
  }
}
