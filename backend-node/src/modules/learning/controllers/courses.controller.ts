import {
  Controller,
  Post,
  Get,
  Body,
  UseGuards,
  Req,
  Query,
} from '@nestjs/common';
import { CommandBus, QueryBus } from '@nestjs/cqrs';
import { JwtAuthGuard } from '../../identity/guards/jwt-auth.guard';
import { CreateCourseCommand } from '../commands/create-course.handler';
import { GetCoursesQuery } from '../queries/get-courses.handler';

@Controller('courses')
export class CoursesController {
  constructor(
    private readonly commandBus: CommandBus,
    private readonly queryBus: QueryBus,
  ) {}

  @Get()
  async getCourses(@Query('facultyId') facultyId?: string) {
    return this.queryBus.execute(new GetCoursesQuery(facultyId));
  }

  @Post()
  @UseGuards(JwtAuthGuard)
  async createCourse(@Body() dto: any, @Req() req: any) {
    return this.commandBus.execute(
      new CreateCourseCommand(
        dto.code,
        dto.name,
        dto.description,
        dto.semester,
        dto.status,
        dto.credits,
        req.user.userId,
        dto.facultyId,
      ),
    );
  }
}
