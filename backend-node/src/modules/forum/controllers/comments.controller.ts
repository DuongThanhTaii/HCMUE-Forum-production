import {
  Controller,
  Post,
  Get,
  Body,
  Param,
  UseGuards,
  Req,
} from '@nestjs/common';
import { CommandBus, QueryBus } from '@nestjs/cqrs';
import { JwtAuthGuard } from '../../identity/guards/jwt-auth.guard';
import { CreateCommentCommand } from '../commands/create-comment.handler';
import { GetPostCommentsQuery } from '../queries/get-post-comments.handler';

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

  @Post()
  @UseGuards(JwtAuthGuard)
  async createComment(@Body() dto: any, @Req() req: any) {
    return this.commandBus.execute(
      new CreateCommentCommand(
        dto.postId,
        req.user.userId,
        dto.content,
        dto.parentCommentId,
      ),
    );
  }
}
