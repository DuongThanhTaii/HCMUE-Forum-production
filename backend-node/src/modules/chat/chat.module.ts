import { Module } from '@nestjs/common';
import { CqrsModule } from '@nestjs/cqrs';
import { IdentityModule } from '../identity/identity.module';
import { ChatGateway } from './gateways/chat.gateway';
import { ConversationsController } from './controllers/conversations.controller';
import { MessagesController } from './controllers/messages.controller';
import { ChannelsController } from './controllers/channels.controller';

import { SendMessageHandler } from './commands/send-message.handler';
import { CreateConversationHandler } from './commands/create-conversation.handler';
import { AddReactionHandler } from './commands/add-reaction.handler';
import { RemoveReactionHandler } from './commands/remove-reaction.handler';
import { DeleteMessageHandler } from './commands/delete-message.handler';

import { GetMessagesHandler } from './queries/get-messages.handler';
import { GetConversationsHandler } from './queries/get-conversations.handler';

const Handlers = [
  SendMessageHandler,
  CreateConversationHandler,
  AddReactionHandler,
  RemoveReactionHandler,
  DeleteMessageHandler,
  GetMessagesHandler,
  GetConversationsHandler,
];

@Module({
  imports: [IdentityModule, CqrsModule],
  controllers: [ConversationsController, MessagesController, ChannelsController],
  providers: [ChatGateway, ...Handlers],
})
export class ChatModule {}
