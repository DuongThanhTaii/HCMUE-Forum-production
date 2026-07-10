import { Controller, Get, Post, Body, Param, UseGuards, Request, Query } from '@nestjs/common';
import { CommandBus, QueryBus } from '@nestjs/cqrs';
import { JwtAuthGuard } from '../../identity/guards/jwt-auth.guard';
import { CreateConversationCommand } from '../commands/create-conversation.handler';
import { MarkConversationReadCommand } from '../commands/mark-conversation-read.handler';
import { ArchiveConversationCommand } from '../commands/archive-conversation.handler';
import { GetConversationsQuery } from '../queries/get-conversations.handler';
import { GetAttachmentsQuery } from '../queries/get-attachments.handler';
import { GetLinksQuery } from '../queries/get-links.handler';
import { SearchMessagesQuery } from '../queries/search-messages.handler';

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
    const participants = Array.from(new Set([req.user.userId, ...(body.participantIds || [])]));
    return this.commandBus.execute(
      new CreateConversationCommand(
        1, // direct type
        req.user.userId,
        participants,
        body.title || 'Direct Message',
      ),
    );
  }

  @Post('group')
  async createGroupConversation(@Request() req: any, @Body() body: any) {
    const participants = Array.from(new Set([req.user.userId, ...(body.participantIds || [])]));
    return this.commandBus.execute(
      new CreateConversationCommand(
        2, // group type
        req.user.userId,
        participants,
        body.title,
      ),
    );
  }

  @Post(':id/mute')
  async muteConversation(@Request() req: any, @Param('id') id: string) {
    // Stub
    return { success: true };
  }

  @Post(':id/read')
  async markRead(@Request() req: any, @Param('id') id: string) {
    return this.commandBus.execute(new MarkConversationReadCommand(req.user.userId, id));
  }

  @Post(':id/archive')
  async archiveConversation(@Request() req: any, @Param('id') id: string) {
    return this.commandBus.execute(new ArchiveConversationCommand(req.user.userId, id));
  }

  @Get(':id/attachments')
  async getAttachments(
    @Param('id') id: string,
    @Query('kind') kind?: 'image' | 'video' | 'file' | 'all'
  ) {
    return this.queryBus.execute(new GetAttachmentsQuery(id, kind));
  }

  @Get(':id/links')
  async getLinks(@Param('id') id: string) {
    return this.queryBus.execute(new GetLinksQuery(id));
  }

  @Get(':id/messages/search')
  async searchMessages(
    @Param('id') id: string,
    @Query('q') q: string
  ) {
    return this.queryBus.execute(new SearchMessagesQuery(id, q));
  }
}
