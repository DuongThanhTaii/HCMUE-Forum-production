import {
  Controller,
  Post,
  Get,
  Body,
  Param,
  UseGuards,
  Req,
  Delete,
} from '@nestjs/common';
import { CommandBus, QueryBus } from '@nestjs/cqrs';
import { JwtAuthGuard } from '../../identity/guards/jwt-auth.guard';
import { CreateCommentCommand } from '../commands/create-comment.handler';
import { GetPostCommentsQuery } from '../queries/get-post-comments.handler';
import { VoteCommentCommand } from '../commands/vote-comment.handler';
import { AcceptAnswerCommand } from '../commands/accept-answer.handler';
import { PinCommentCommand } from '../commands/pin-comment.handler';
import { DeleteCommentCommand } from '../commands/delete-comment.handler';

@Controller('comments')
export class CommentsController {
  constructor(
    private readonly commandBus: CommandBus,
    private readonly queryBus: QueryBus,
  ) {}

  @Get('post/:postId')
  async getComments(@Param('postId') postId: string) {
    return this.queryBus.execute(new GetPostCommentsQuery(postId));
  }

  @Post('posts/:postId')
  @UseGuards(JwtAuthGuard)
  async createComment(
    @Param('postId') postId: string,
    @Body() dto: any,
    @Req() req: any,
  ) {
    return this.commandBus.execute(
      new CreateCommentCommand(
        postId,
        req.user.userId,
        dto.content,
        dto.parentCommentId,
      ),
    );
  }

  @Post(':id/vote')
  @UseGuards(JwtAuthGuard)
  async voteComment(
    @Param('id') id: string,
    @Body('voteType') voteType: number,
    @Req() req: any,
  ) {
    return this.commandBus.execute(
      new VoteCommentCommand(id, req.user.userId, voteType),
    );
  }

  @Post(':id/accept')
  @UseGuards(JwtAuthGuard)
  async acceptAnswer(
    @Param('id') id: string,
    @Req() req: any,
  ) {
    return this.commandBus.execute(
      new AcceptAnswerCommand(id, req.user.userId),
    );
  }

  @Post(':id/pin')
  @UseGuards(JwtAuthGuard)
  async pinComment(
    @Param('id') id: string,
    @Req() req: any,
  ) {
    return this.commandBus.execute(
      new PinCommentCommand(id, req.user.userId),
    );
  }

  @Delete(':id')
  @UseGuards(JwtAuthGuard)
  async deleteComment(
    @Param('id') id: string,
    @Req() req: any,
  ) {
    return this.commandBus.execute(
      new DeleteCommentCommand(id, req.user.userId, req.user.roles || []),
    );
  }
}
