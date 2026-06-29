import { Controller, Get, Post, Body, Param, Query, UseGuards, Request } from '@nestjs/common';
import { CommandBus, QueryBus } from '@nestjs/cqrs';
import { JwtAuthGuard } from '../../identity/guards/jwt-auth.guard';
import { SendMessageCommand } from '../commands/send-message.handler';
import { GetMessagesQuery } from '../queries/get-messages.handler';
import { AddReactionCommand } from '../commands/add-reaction.handler';

@Controller('chat/messages')
@UseGuards(JwtAuthGuard)
export class MessagesController {
  constructor(
    private readonly commandBus: CommandBus,
    private readonly queryBus: QueryBus,
  ) {}

  @Get()
  async getMessages(@Query('conversationId') conversationId: string) {
    return this.queryBus.execute(new GetMessagesQuery(conversationId));
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
