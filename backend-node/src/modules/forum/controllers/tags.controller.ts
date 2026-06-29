import { Controller, Get } from '@nestjs/common';
import { QueryBus } from '@nestjs/cqrs';
import { GetPopularTagsQuery } from '../queries/get-popular-tags.handler';

@Controller('tags')
export class TagsController {
  constructor(private readonly queryBus: QueryBus) {}

  @Get('popular')
  async getPopularTags() {
    return this.queryBus.execute(new GetPopularTagsQuery());
  }
}
