import {
  Controller,
  Post,
  Get,
  Put,
  Body,
  Param,
  UseGuards,
  Query,
  Req,
} from '@nestjs/common';
import { CommandBus, QueryBus } from '@nestjs/cqrs';
import { JwtAuthGuard } from '../../identity/guards/jwt-auth.guard';
import { CreatePostCommand } from '../commands/create-post.handler';
import { UpdatePostCommand } from '../commands/update-post.handler';
import { VotePostCommand } from '../commands/vote-post.handler';
import { GetPostsQuery } from '../queries/get-posts.handler';

@Controller('posts')
export class PostsController {
  constructor(
    private readonly commandBus: CommandBus,
    private readonly queryBus: QueryBus,
  ) {}

  @Get()
  async getPosts(
    @Query('skip') skip?: string,
    @Query('take') take?: string,
    @Query('category') categoryId?: string,
  ) {
    return this.queryBus.execute(
      new GetPostsQuery(
        skip ? parseInt(skip) : 0,
        take ? parseInt(take) : 20,
        categoryId,
      ),
    );
  }

  @Post()
  @UseGuards(JwtAuthGuard)
  async createPost(@Body() dto: any, @Req() req: any) {
    return this.commandBus.execute(
      new CreatePostCommand(
        dto.title,
        dto.content,
        dto.type,
        dto.status,
        req.user.userId,
        dto.tags || {},
        dto.categoryId,
        dto.threadChannelId,
      ),
    );
  }

  @Put(':id')
  @UseGuards(JwtAuthGuard)
  async updatePost(@Param('id') id: string, @Body() dto: any, @Req() req: any) {
    return this.commandBus.execute(
      new UpdatePostCommand(id, req.user.userId, dto.title, dto.content),
    );
  }

  @Post(':id/vote')
  @UseGuards(JwtAuthGuard)
  async votePost(
    @Param('id') id: string,
    @Body('voteType') voteType: number,
    @Req() req: any,
  ) {
    return this.commandBus.execute(
      new VotePostCommand(id, req.user.userId, voteType),
    );
  }
}
