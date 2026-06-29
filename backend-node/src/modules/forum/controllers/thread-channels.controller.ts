import { Controller, Get, UseGuards } from '@nestjs/common';
import { QueryBus } from '@nestjs/cqrs';
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
}
