import { Controller, Post, Body, UseGuards, Req } from '@nestjs/common';
import { CommandBus } from '@nestjs/cqrs';
import { JwtAuthGuard } from '../../identity/guards/jwt-auth.guard';
import { CreateCompanyCommand } from '../commands/create-company.handler';

@Controller('companies')
export class CompaniesController {
  constructor(private readonly commandBus: CommandBus) {}

  @Post()
  @UseGuards(JwtAuthGuard)
  async createCompany(@Body() dto: any, @Req() req: any) {
    return this.commandBus.execute(
      new CreateCompanyCommand(
        dto.name,
        dto.description,
        dto.industry,
        dto.size,
        dto.contactEmail,
        dto.status,
        req.user.userId,
      ),
    );
  }
}
