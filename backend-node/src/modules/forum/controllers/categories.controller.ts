import { Controller, Get, Post, Put, Delete, Body, Param, UseGuards } from '@nestjs/common';
import { CommandBus, QueryBus } from '@nestjs/cqrs';
import { GetCategoriesQuery } from '../queries/get-categories.handler';
import { CreateCategoryCommand } from '../commands/create-category.handler';
import { UpdateCategoryCommand } from '../commands/update-category.handler';
import { DeleteCategoryCommand } from '../commands/delete-category.handler';
import { JwtAuthGuard } from '../../identity/guards/jwt-auth.guard';
import { RolesGuard } from '../../identity/guards/roles.guard';
import { Roles } from '../../../common/decorators/roles.decorator';

@Controller('categories')
export class CategoriesController {
  constructor(
    private readonly queryBus: QueryBus,
    private readonly commandBus: CommandBus,
  ) {}

  @Get()
  async getCategories() {
    return this.queryBus.execute(new GetCategoriesQuery());
  }

  @UseGuards(JwtAuthGuard, RolesGuard)
  @Roles('Admin', 'SuperAdmin', 'Moderator')
  @Post()
  async createCategory(@Body() body: any) {
    return this.commandBus.execute(
      new CreateCategoryCommand(
        body.name,
        body.description,
        body.slug,
        body.displayOrder,
        body.isActive,
        body.parentCategoryId,
        body.moderatorIds,
      ),
    );
  }

  @UseGuards(JwtAuthGuard, RolesGuard)
  @Roles('Admin', 'SuperAdmin', 'Moderator')
  @Put(':id')
  async updateCategory(@Param('id') id: string, @Body() body: any) {
    return this.commandBus.execute(
      new UpdateCategoryCommand(
        id,
        body.name,
        body.description,
        body.slug,
        body.displayOrder,
        body.isActive,
        body.parentCategoryId,
        body.moderatorIds,
      ),
    );
  }

  @UseGuards(JwtAuthGuard, RolesGuard)
  @Roles('Admin', 'SuperAdmin')
  @Delete(':id')
  async deleteCategory(@Param('id') id: string) {
    return this.commandBus.execute(new DeleteCategoryCommand(id));
  }
}
