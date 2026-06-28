import { CommandHandler, ICommandHandler } from '@nestjs/cqrs';
import { PrismaService } from '../../../common/prisma/prisma.service';
import * as crypto from 'crypto';

export class CreateCompanyCommand {
  constructor(
    public readonly name: string,
    public readonly description: string,
    public readonly industry: number,
    public readonly size: number,
    public readonly contactEmail: string,
    public readonly status: number,
    public readonly registeredBy: string,
  ) {}
}

@CommandHandler(CreateCompanyCommand)
export class CreateCompanyHandler implements ICommandHandler<CreateCompanyCommand> {
  constructor(private readonly prisma: PrismaService) {}

  async execute(command: CreateCompanyCommand) {
    return this.prisma.companies.create({
      data: {
        id: crypto.randomUUID(),
        name: command.name,
        description: command.description,
        industry: command.industry,
        size: command.size,
        contact_email: command.contactEmail,
        status: command.status,
        registered_by: command.registeredBy,
        registered_at: new Date(),
        benefits: [],
      },
    });
  }
}
