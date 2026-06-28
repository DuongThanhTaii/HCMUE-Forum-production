import { CommandHandler, ICommandHandler } from '@nestjs/cqrs';
import { PrismaService } from '../../../common/prisma/prisma.service';
import * as crypto from 'crypto';

export class CreateFacultyCommand {
  constructor(
    public readonly code: string,
    public readonly name: string,
    public readonly description: string,
    public readonly status: number,
    public readonly createdBy: string,
  ) {}
}

@CommandHandler(CreateFacultyCommand)
export class CreateFacultyHandler implements ICommandHandler<CreateFacultyCommand> {
  constructor(private readonly prisma: PrismaService) {}

  async execute(command: CreateFacultyCommand) {
    return this.prisma.faculties.create({
      data: {
        id: crypto.randomUUID(),
        code: command.code,
        name: command.name,
        description: command.description,
        status: command.status,
        created_at: new Date(),
        created_by: command.createdBy,
      },
    });
  }
}
