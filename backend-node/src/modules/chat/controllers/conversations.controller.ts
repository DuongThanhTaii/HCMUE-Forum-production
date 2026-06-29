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
    // Basic mapping, exact params may vary
    return this.commandBus.execute(
      new CreateConversationCommand(
        req.user.userId,
        body.title || 'Direct Message',
        1, // direct type
        body.participantIds
      ),
    );
  }

  @Post('group')
  async createGroupConversation(@Request() req: any, @Body() body: any) {
    return this.commandBus.execute(
      new CreateConversationCommand(
        req.user.userId,
        body.title,
        2, // group type
        body.participantIds
      ),
    );
  }

  @Post(':id/mute')
  async muteConversation(@Request() req: any, @Param('id') id: string) {
    // Stub
    return { success: true };
  }
}
