import { Controller, Get, Post, Param, Query, Body, ParseIntPipe } from '@nestjs/common';
import { CommandBus, QueryBus } from '@nestjs/cqrs';
import { GetReportsQuery } from '../queries/get-reports.handler';
import { GetPendingPostsQuery } from '../queries/get-pending-posts.handler';
import { ResolveReportCommand } from '../commands/resolve-report.handler';

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
}
