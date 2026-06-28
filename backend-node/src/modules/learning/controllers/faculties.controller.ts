import { Controller, Post, Get, Body, UseGuards, Req } from '@nestjs/common';
import { CommandBus, QueryBus } from '@nestjs/cqrs';
import { JwtAuthGuard } from '../../identity/guards/jwt-auth.guard';
import { CreateFacultyCommand } from '../commands/create-faculty.handler';
import { GetFacultiesQuery } from '../queries/get-faculties.handler';

@Controller('faculties')
export class FacultiesController {
  constructor(
    private readonly commandBus: CommandBus,
    private readonly queryBus: QueryBus,
  ) {}

  @Get()
  async getFaculties() {
    return this.queryBus.execute(new GetFacultiesQuery());
  }

  @Post()
  @UseGuards(JwtAuthGuard)
  async createFaculty(@Body() dto: any, @Req() req: any) {
    return this.commandBus.execute(
      new CreateFacultyCommand(
        dto.code,
        dto.name,
        dto.description,
        dto.status,
        req.user.userId,
      ),
    );
  }
}
