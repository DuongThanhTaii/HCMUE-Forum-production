import { Controller, Get, Post, Delete, Body, Param, Query, UseGuards, Request } from '@nestjs/common';
import { CommandBus, QueryBus } from '@nestjs/cqrs';
import { JwtAuthGuard } from '../../identity/guards/jwt-auth.guard';
import { SendMessageCommand } from '../commands/send-message.handler';
import { GetMessagesQuery } from '../queries/get-messages.handler';
import { AddReactionCommand } from '../commands/add-reaction.handler';
import { RemoveReactionCommand } from '../commands/remove-reaction.handler';
import { DeleteMessageCommand } from '../commands/delete-message.handler';

@Controller('chat/messages')
@UseGuards(JwtAuthGuard)
export class MessagesController {
  constructor(
    private readonly commandBus: CommandBus,
    private readonly queryBus: QueryBus,
  ) {}

  @Get()
  async getMessages(
    @Query('conversationId') conversationId: string,
    @Query('page') page: string = '1',
    @Query('pageSize') pageSize: string = '50',
  ) {
    const pageNum = parseInt(page, 10) || 1;
    const sizeNum = parseInt(pageSize, 10) || 50;
    const skip = (pageNum - 1) * sizeNum;
    
    const { items, totalCount } = await this.queryBus.execute(
      new GetMessagesQuery(conversationId, skip, sizeNum),
    );

    return {
      items,
      page: pageNum,
      pageSize: sizeNum,
      totalCount,
      totalPages: Math.ceil(totalCount / sizeNum),
    };
  }

  @Post()
  async sendMessage(@Request() req: any, @Body() body: any) {
    return this.commandBus.execute(
      new SendMessageCommand(
        body.conversationId,
        req.user.userId,
        body.content,
        1, // type Text
        body.replyToMessageId,
      ),
    );
  }

  @Post(':id/reactions/:emoji')
  async reactToMessage(
    @Request() req: any,
    @Param('id') messageId: string,
    @Param('emoji') emoji: string,
    @Body('conversationId') conversationId: string,
  ) {
    return this.commandBus.execute(
      new AddReactionCommand(messageId, req.user.userId, emoji, conversationId),
    );
  }

  @Delete(':id/reactions/:emoji')
  async removeReaction(
    @Request() req: any,
    @Param('id') messageId: string,
    @Param('emoji') emoji: string,
    @Body('conversationId') conversationId: string,
  ) {
    return this.commandBus.execute(
      new RemoveReactionCommand(messageId, req.user.userId, emoji, conversationId),
    );
  }

  @Delete(':id')
  async deleteMessage(
    @Request() req: any,
    @Param('id') messageId: string,
    @Body('conversationId') conversationId: string,
  ) {
    return this.commandBus.execute(
      new DeleteMessageCommand(messageId, req.user.userId, conversationId),
    );
  }

  // Stubs for unimplemented endpoints
  @Post('upload')
  async uploadAttachment() { return { success: true }; }

  @Post('with-attachments')
  async sendWithAttachments() { return { success: true }; }

  @Post(':id/read')
  async markRead() { return { success: true }; }

  @Post(':id/report')
  async reportMessage() { return { success: true }; }
}
