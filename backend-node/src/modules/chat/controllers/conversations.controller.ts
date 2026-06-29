import { Controller, Get, Post, Body, Param, UseGuards, Request } from '@nestjs/common';
import { CommandBus, QueryBus } from '@nestjs/cqrs';
import { JwtAuthGuard } from '../../identity/guards/jwt-auth.guard';
import { CreateConversationCommand } from '../commands/create-conversation.handler';
import { GetConversationsQuery } from '../queries/get-conversations.handler';

@Controller('chat/conversations')
@UseGuards(JwtAuthGuard)
export class ConversationsController {
  constructor(
    private readonly commandBus: CommandBus,
    private readonly queryBus: QueryBus,
  ) {}

  @Get()
  async getConversations(@Request() req: any) {
    return this.queryBus.execute(new GetConversationsQuery(req.user.userId));
  }

  @Post('direct')
  async createDirectConversation(@Request() req: any, @Body() body: any) {
    return this.commandBus.execute(
      new CreateConversationCommand(
        1, // direct type
        req.user.userId,
        body.participantIds || [],
        body.title || 'Direct Message',
      ),
    );
  }

  @Post('group')
  async createGroupConversation(@Request() req: any, @Body() body: any) {
    return this.commandBus.execute(
      new CreateConversationCommand(
        2, // group type
        req.user.userId,
        body.participantIds || [],
        body.title,
      ),
    );
  }

  @Post(':id/mute')
  async muteConversation(@Request() req: any, @Param('id') id: string) {
    // Stub
    return { success: true };
  }
}
