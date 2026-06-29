import { Controller, Get, Post, Param, UseGuards, Request } from '@nestjs/common';
import { JwtAuthGuard } from '../../identity/guards/jwt-auth.guard';

@Controller('chat/channels')
@UseGuards(JwtAuthGuard)
export class ChannelsController {
  @Get('public')
  async getPublicChannels() {
    return []; // Stub
  }

  @Get('my-channels')
  async getMyChannels() {
    return []; // Stub
  }

  @Post(':id/join')
  async joinChannel(@Param('id') id: string) {
    return { success: true };
  }

  @Post(':id/leave')
  async leaveChannel(@Param('id') id: string) {
    return { success: true };
  }
}
