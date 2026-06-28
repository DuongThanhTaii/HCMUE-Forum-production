import { Module } from '@nestjs/common';
import { CqrsModule } from '@nestjs/cqrs';
import { IdentityModule } from '../identity/identity.module';
import { ChatGateway } from './gateways/chat.gateway';
import { SendMessageHandler } from './commands/send-message.handler';
import { CreateConversationHandler } from './commands/create-conversation.handler';
import { AddReactionHandler } from './commands/add-reaction.handler';
import { GetMessagesHandler } from './queries/get-messages.handler';
import { GetConversationsHandler } from './queries/get-conversations.handler';

const Handlers = [
  SendMessageHandler,
  CreateConversationHandler,
  AddReactionHandler,
  GetMessagesHandler,
  GetConversationsHandler,
];

@Module({
  imports: [IdentityModule, CqrsModule],
  providers: [ChatGateway, ...Handlers],
})
export class ChatModule {}
