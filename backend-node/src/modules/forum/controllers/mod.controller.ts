import { Controller, Get, Post, Patch, Param, Query, Body, ParseIntPipe, UseGuards, Req } from '@nestjs/common';
import { CommandBus, QueryBus } from '@nestjs/cqrs';
import { GetReportsQuery } from '../queries/get-reports.handler';
import { GetPendingPostsQuery } from '../queries/get-pending-posts.handler';
import { ResolveReportCommand } from '../commands/resolve-report.handler';
import { RejectPostCommand } from '../commands/reject-post.handler';
import { ApproveBulkPostsCommand } from '../commands/approve-bulk-posts.handler';
import { JwtAuthGuard } from '../../identity/guards/jwt-auth.guard';
import { RolesGuard } from '../../identity/guards/roles.guard';
import { Roles } from '../../../common/decorators/roles.decorator';

@Controller('mod')
export class ModController {
  constructor(
    private readonly queryBus: QueryBus,
    private readonly commandBus: CommandBus,
  ) {}

  @Get('reports')
  async getReports(
    @Query('status') status?: string,
    @Query('pageNumber') pageNumber?: string,
    @Query('pageSize') pageSize?: string,
  ) {
    const page = pageNumber ? parseInt(pageNumber, 10) : 1;
    const size = pageSize ? parseInt(pageSize, 10) : 20;
    return this.queryBus.execute(new GetReportsQuery(status, page, size));
  }

  @Post('reports/:id/resolve')
  async resolveReport(
    @Param('id', ParseIntPipe) id: number,
    @Body('action') action: string,
  ) {
    return this.commandBus.execute(new ResolveReportCommand(id, action));
  }

  @Get('posts')
  async getPendingPosts(
    @Query('pageNumber') pageNumber?: string,
    @Query('pageSize') pageSize?: string,
  ) {
    const page = pageNumber ? parseInt(pageNumber, 10) : 1;
    const size = pageSize ? parseInt(pageSize, 10) : 20;
    return this.queryBus.execute(new GetPendingPostsQuery(page, size));
  }

  @UseGuards(JwtAuthGuard, RolesGuard)
  @Roles('Admin', 'SuperAdmin', 'Moderator')
  @Patch('posts/:id/reject')
  async rejectPost(
    @Param('id') id: string,
    @Body('reason') reason: string,
    @Req() req: any,
  ) {
    return this.commandBus.execute(
      new RejectPostCommand(id, req.user.userId, reason),
    );
  }

  @UseGuards(JwtAuthGuard, RolesGuard)
  @Roles('Admin', 'SuperAdmin', 'Moderator')
  @Post('posts/approve-bulk')
  async approveBulkPosts(
    @Body('postIds') postIds: string[],
    @Req() req: any,
  ) {
    return this.commandBus.execute(
      new ApproveBulkPostsCommand(postIds, req.user.userId),
    );
  }
}
