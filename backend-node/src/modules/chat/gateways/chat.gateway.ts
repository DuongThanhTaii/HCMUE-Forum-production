import {
  WebSocketGateway,
  WebSocketServer,
  OnGatewayConnection,
  OnGatewayDisconnect,
  SubscribeMessage,
  MessageBody,
  ConnectedSocket,
} from '@nestjs/websockets';
import { Server, Socket } from 'socket.io';
import { JwtService } from '@nestjs/jwt';
import { QueryBus } from '@nestjs/cqrs';
import { GetUserByIdQuery } from '../../identity/queries/get-user-by-id.query';

@WebSocketGateway({
  namespace: 'chat',
  cors: { origin: '*' },
})
export class ChatGateway implements OnGatewayConnection, OnGatewayDisconnect {
  @WebSocketServer()
  server: Server;

  constructor(
    private readonly jwtService: JwtService,
    private readonly queryBus: QueryBus,
  ) {}

  async handleConnection(client: Socket) {
    try {
      const token =
        client.handshake.auth.token?.split(' ')[1] ||
        client.handshake.headers.authorization?.split(' ')[1];
      if (!token) {
        client.disconnect();
        return;
      }
      const payload = this.jwtService.verify(token);
      client.data.userId = payload.sub;
    } catch (error) {
      client.disconnect();
    }
  }

  handleDisconnect(client: Socket) {}

  @SubscribeMessage('join_conversation')
  handleJoinConversation(
    @ConnectedSocket() client: Socket,
    @MessageBody() data: { conversationId: string },
  ) {
    if (data?.conversationId) {
      client.join(`conversation_${data.conversationId}`);
    }
  }

  @SubscribeMessage('leave_conversation')
  handleLeaveConversation(
    @ConnectedSocket() client: Socket,
    @MessageBody() data: { conversationId: string },
  ) {
    if (data?.conversationId) {
      client.leave(`conversation_${data.conversationId}`);
    }
  }

  @SubscribeMessage('SendTypingIndicator')
  async handleTyping(
    @ConnectedSocket() client: Socket,
    @MessageBody() payload: any, // array of args [conversationId, isTyping] or object
  ) {
    let conversationId: string;
    let isTyping: boolean;

    // Support both positional args emitted by frontend: emit('SendTypingIndicator', conversationId, isTyping)
    // and object payload
    if (Array.isArray(payload)) {
      conversationId = payload[0];
      isTyping = payload[1];
    } else if (payload && typeof payload === 'object') {
      conversationId = payload.conversationId;
      isTyping = payload.isTyping;
    } else {
      return; // invalid payload
    }

    if (!conversationId) return;

    const userId = client.data.userId;
    if (!userId) return;

    // Get user details for userName
    let userName = 'User';
    try {
      const user = await this.queryBus.execute(new GetUserByIdQuery(userId));
      if (user) {
        userName = `${user.firstName} ${user.lastName}`.trim();
      }
    } catch (e) {
      // fallback to default
    }

    client.to(`conversation_${conversationId}`).emit('UserTyping', {
      userId,
      userName,
      conversationId,
      isTyping,
    });
  }

  broadcastNewMessage(conversationId: string, message: any) {
    this.server
      .to(`conversation_${conversationId}`)
      .emit('new_message', message);
  }

  broadcastReaction(conversationId: string, reaction: any) {
    this.server
      .to(`conversation_${conversationId}`)
      .emit('message_reaction_added', reaction);
  }
}
