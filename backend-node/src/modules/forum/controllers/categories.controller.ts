import { Controller, Get } from '@nestjs/common';
import { QueryBus } from '@nestjs/cqrs';
import { GetCategoriesQuery } from '../queries/get-categories.handler';

@Controller('categories')
export class CategoriesController {
  constructor(private readonly queryBus: QueryBus) {}

  @Get()
  async getCategories() {
    return this.queryBus.execute(new GetCategoriesQuery());
  }
}
