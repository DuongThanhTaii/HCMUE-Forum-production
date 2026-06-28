import { Module } from '@nestjs/common';
import { CqrsModule } from '@nestjs/cqrs';
import { IdentityModule } from '../identity/identity.module';
import { PostsController } from './controllers/posts.controller';
import { CommentsController } from './controllers/comments.controller';
import { CreatePostHandler } from './commands/create-post.handler';
import { UpdatePostHandler } from './commands/update-post.handler';
import { VotePostHandler } from './commands/vote-post.handler';
import { CreateCommentHandler } from './commands/create-comment.handler';
import { GetPostsHandler } from './queries/get-posts.handler';
import { GetPostCommentsHandler } from './queries/get-post-comments.handler';

const Handlers = [
  CreatePostHandler,
  UpdatePostHandler,
  VotePostHandler,
  CreateCommentHandler,
  GetPostsHandler,
  GetPostCommentsHandler,
];

@Module({
  imports: [IdentityModule, CqrsModule],
  controllers: [PostsController, CommentsController],
  providers: [...Handlers],
})
export class ForumModule {}
