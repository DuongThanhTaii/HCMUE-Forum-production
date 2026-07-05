import { Controller, Get, Post, Put, Delete, Param, Body, UseGuards } from '@nestjs/common';
import { CommandBus, QueryBus } from '@nestjs/cqrs';
import { GetThreadChannelsQuery } from '../queries/get-thread-channels.handler';
import { CreateThreadChannelCommand } from '../commands/create-thread-channel.handler';
import { UpdateThreadChannelCommand } from '../commands/update-thread-channel.handler';
import { DeleteThreadChannelCommand } from '../commands/delete-thread-channel.handler';
import { JwtAuthGuard } from '../../identity/guards/jwt-auth.guard';
import { RolesGuard } from '../../identity/guards/roles.guard';
import { Roles } from '../../../common/decorators/roles.decorator';

@Controller('thread-channels')
export class ThreadChannelsController {
  constructor(
    private readonly queryBus: QueryBus,
    private readonly commandBus: CommandBus,
  ) {}

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
    return this.commandBus.execute(
      new CreateThreadChannelCommand(
        body.code,
        body.name,
        body.description,
        body.displayOrder,
        body.isActive,
        body.allowPinnedComments,
        body.allowAcceptedAnswers,
        body.allowModeratorActions,
      ),
    );
  }

  @UseGuards(JwtAuthGuard, RolesGuard)
  @Roles('Admin', 'SuperAdmin')
  @Put(':id')
  async updateThreadChannel(@Param('id') id: string, @Body() body: any) {
    return this.commandBus.execute(
      new UpdateThreadChannelCommand(
        id,
        body.code,
        body.name,
        body.description,
        body.displayOrder,
        body.isActive,
        body.allowPinnedComments,
        body.allowAcceptedAnswers,
        body.allowModeratorActions,
      ),
    );
  }

  @UseGuards(JwtAuthGuard, RolesGuard)
  @Roles('Admin', 'SuperAdmin')
  @Delete(':id')
  async deleteThreadChannel(@Param('id') id: string) {
    return this.commandBus.execute(new DeleteThreadChannelCommand(id));
  }
}
