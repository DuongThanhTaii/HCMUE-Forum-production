import {
  Controller,
  Post,
  Get,
  Put,
  Delete,
  Body,
  Param,
  UseGuards,
  Query,
  Req,
} from '@nestjs/common';
import { CommandBus, QueryBus } from '@nestjs/cqrs';
import { JwtAuthGuard } from '../../identity/guards/jwt-auth.guard';
import { OptionalJwtAuthGuard } from '../../identity/guards/optional-jwt-auth.guard';
import { CreatePostCommand } from '../commands/create-post.handler';
import { UpdatePostCommand } from '../commands/update-post.handler';
import { VotePostCommand } from '../commands/vote-post.handler';
import { GetPostsQuery } from '../queries/get-posts.handler';
import { GetPostByIdQuery } from '../queries/get-post-by-id.handler';
import { PublishPostCommand } from '../commands/publish-post.handler';
import { GetBookmarkedPostsQuery } from '../queries/get-bookmarked-posts.handler';
import { BookmarkPostCommand } from '../commands/bookmark-post.handler';
import { UnbookmarkPostCommand } from '../commands/unbookmark-post.handler';
import { ReportPostCommand } from '../commands/report-post.handler';

@Controller('posts')
export class PostsController {
  constructor(
    private readonly commandBus: CommandBus,
    private readonly queryBus: QueryBus,
  ) {}

  @Get()
  @UseGuards(OptionalJwtAuthGuard)
  async getPosts(
    @Query('pageNumber') pageNumber?: string,
    @Query('pageSize') pageSize?: string,
    @Query('categoryId') categoryId?: string,
    @Query('threadChannelId') threadChannelId?: string,
    @Query('searchTerm') searchTerm?: string,
    @Query('sortBy') sortBy?: string,
    @Query('isSolved') isSolved?: string,
    @Query('isUnanswered') isUnanswered?: string,
    @Query('isPinned') isPinned?: string,
    @Req() req?: any,
  ) {
    return this.queryBus.execute(
      new GetPostsQuery(
        pageNumber ? parseInt(pageNumber) : 1,
        pageSize ? parseInt(pageSize) : 20,
        categoryId,
        threadChannelId,
        searchTerm,
        sortBy ? parseInt(sortBy) : undefined,
        isSolved === 'true' ? true : isSolved === 'false' ? false : undefined,
        isUnanswered === 'true' ? true : isUnanswered === 'false' ? false : undefined,
        isPinned === 'true' ? true : isPinned === 'false' ? false : undefined,
        req?.user?.userId,
      ),
    );
  }

  @Get('bookmarks')
  @UseGuards(JwtAuthGuard)
  async getBookmarkedPosts(
    @Query('pageNumber') pageNumber?: string,
    @Query('pageSize') pageSize?: string,
    @Req() req?: any,
  ) {
    const page = pageNumber ? parseInt(pageNumber) : 1;
    const size = pageSize ? parseInt(pageSize) : 20;
    const skip = (page - 1) * size;
    return this.queryBus.execute(
      new GetBookmarkedPostsQuery(
        skip,
        size,
        req.user.userId,
      ),
    );
  }

  @Get(':id')
  @UseGuards(OptionalJwtAuthGuard)
  async getPostById(@Param('id') id: string, @Req() req?: any) {
    return this.queryBus.execute(new GetPostByIdQuery(id, req?.user?.userId));
  }

  @Post(':id/publish')
  @UseGuards(JwtAuthGuard)
  async publishPost(@Param('id') id: string) {
    return this.commandBus.execute(new PublishPostCommand(id));
  }

  @Post()
  @UseGuards(JwtAuthGuard)
  async createPost(@Body() dto: any, @Req() req: any) {
    return this.commandBus.execute(
      new CreatePostCommand(
        dto.title,
        dto.content,
        dto.type || 1, // Default type
        dto.status || 2, // Default status (2 = Published)
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

  @Post(':id/bookmark')
  @UseGuards(JwtAuthGuard)
  async bookmarkPost(@Param('id') id: string, @Req() req: any) {
    return this.commandBus.execute(
      new BookmarkPostCommand(id, req.user.userId),
    );
  }

  @Delete(':id/bookmark')
  @UseGuards(JwtAuthGuard)
  async unbookmarkPost(@Param('id') id: string, @Req() req: any) {
    return this.commandBus.execute(
      new UnbookmarkPostCommand(id, req.user.userId),
    );
  }

  @Post(':id/report')
  @UseGuards(JwtAuthGuard)
  async reportPost(
    @Param('id') id: string,
    @Body() dto: any,
    @Req() req: any,
  ) {
    return this.commandBus.execute(
      new ReportPostCommand(
        id,
        req.user.userId,
        dto.reason,
        dto.description,
      ),
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
