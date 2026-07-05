import { Controller, Get, Put, Param, Body, UseGuards, Req } from '@nestjs/common';
import { CommandBus, QueryBus } from '@nestjs/cqrs';
import { GetSystemSettingQuery } from '../queries/get-system-setting.handler';
import { UpdateSystemSettingCommand } from '../commands/update-system-setting.handler';
import { JwtAuthGuard } from '../../identity/guards/jwt-auth.guard';
import { RolesGuard } from '../../identity/guards/roles.guard';
import { Roles } from '../../../common/decorators/roles.decorator';

@Controller('admin/settings')
@UseGuards(JwtAuthGuard, RolesGuard)
@Roles('Admin', 'SuperAdmin')
export class SettingsController {
  constructor(
    private readonly queryBus: QueryBus,
    private readonly commandBus: CommandBus,
  ) {}

  @Get(':key')
  async getSetting(@Param('key') key: string) {
    const value = await this.queryBus.execute(new GetSystemSettingQuery(key));
    return { key, value };
  }

  @Put(':key')
  async updateSetting(
    @Param('key') key: string,
    @Body('value') value: string,
    @Body('description') description: string,
    @Req() req: any,
  ) {
    return this.commandBus.execute(
      new UpdateSystemSettingCommand(key, value, description, req.user.userId),
    );
  }
}
