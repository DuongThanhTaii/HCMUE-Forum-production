import { CommandHandler, ICommandHandler } from '@nestjs/cqrs';
import { PrismaService } from '../../../common/prisma/prisma.service';
import * as crypto from 'crypto';

export class CreateCourseCommand {
  constructor(
    public readonly code: string,
    public readonly name: string,
    public readonly description: string,
    public readonly semester: string,
    public readonly status: number,
    public readonly credits: number,
    public readonly createdBy: string,
    public readonly facultyId?: string,
  ) {}
}

@CommandHandler(CreateCourseCommand)
export class CreateCourseHandler implements ICommandHandler<CreateCourseCommand> {
  constructor(private readonly prisma: PrismaService) {}

  async execute(command: CreateCourseCommand) {
    const course = await this.prisma.courses.create({
      data: {
        id: crypto.randomUUID(),
        code: command.code,
        name: command.name,
        description: command.description,
        semester: command.semester,
        status: command.status,
        credits: command.credits,
        created_at: new Date(),
        created_by: command.createdBy,
        faculty_id: command.facultyId,
        moderator_ids: [],
      },
    });

    if (command.facultyId) {
      await this.prisma.faculties.update({
        where: { id: command.facultyId },
        data: { course_count: { increment: 1 } },
      });
    }

    return course;
  }
}
