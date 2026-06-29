import { Module } from '@nestjs/common';
import { CqrsModule } from '@nestjs/cqrs';
import { IdentityModule } from '../identity/identity.module';
import { PostsController } from './controllers/posts.controller';
import { CommentsController } from './controllers/comments.controller';
import { CategoriesController } from './controllers/categories.controller';
import { TagsController } from './controllers/tags.controller';
import { ThreadChannelsController } from './controllers/thread-channels.controller';

import { CreatePostHandler } from './commands/create-post.handler';
import { UpdatePostHandler } from './commands/update-post.handler';
import { VotePostHandler } from './commands/vote-post.handler';
import { CreateCommentHandler } from './commands/create-comment.handler';

import { GetPostsHandler } from './queries/get-posts.handler';
import { GetPostCommentsHandler } from './queries/get-post-comments.handler';
import { GetCategoriesHandler } from './queries/get-categories.handler';
import { GetPopularTagsHandler } from './queries/get-popular-tags.handler';
import { GetThreadChannelsHandler } from './queries/get-thread-channels.handler';

import { ModController } from './controllers/mod.controller';

import { GetReportsHandler } from './queries/get-reports.handler';
import { GetPendingPostsHandler } from './queries/get-pending-posts.handler';
import { ResolveReportHandler } from './commands/resolve-report.handler';
import { GetPostByIdHandler } from './queries/get-post-by-id.handler';
import { PublishPostHandler } from './commands/publish-post.handler';

import { BookmarkPostHandler } from './commands/bookmark-post.handler';
import { UnbookmarkPostHandler } from './commands/unbookmark-post.handler';
import { ReportPostHandler } from './commands/report-post.handler';
import { VoteCommentHandler } from './commands/vote-comment.handler';
import { AcceptAnswerHandler } from './commands/accept-answer.handler';
import { PinCommentHandler } from './commands/pin-comment.handler';
import { GetBookmarkedPostsHandler } from './queries/get-bookmarked-posts.handler';

const CommandHandlers = [
  CreatePostHandler,
  UpdatePostHandler,
  VotePostHandler,
  CreateCommentHandler,
  ResolveReportHandler,
  PublishPostHandler,
  BookmarkPostHandler,
  UnbookmarkPostHandler,
  ReportPostHandler,
  VoteCommentHandler,
  AcceptAnswerHandler,
  PinCommentHandler,
];

const QueryHandlers = [
  GetPostsHandler,
  GetPostCommentsHandler,
  GetCategoriesHandler,
  GetPopularTagsHandler,
  GetThreadChannelsHandler,
  GetReportsHandler,
  GetPendingPostsHandler,
  GetPostByIdHandler,
  GetBookmarkedPostsHandler,
];

@Module({
  imports: [IdentityModule, CqrsModule],
  controllers: [
    PostsController,
    CommentsController,
    CategoriesController,
    TagsController,
    ThreadChannelsController,
    ModController,
  ],
  providers: [...CommandHandlers, ...QueryHandlers],
})
export class ForumModule {}
