import {
  Controller,
  Get,
  Post,
  Put,
  Delete,
  UseGuards,
  Body,
  Param,
} from '@nestjs/common';
import { CommandBus, QueryBus } from '@nestjs/cqrs';
import { JwtAuthGuard } from '../../guards/jwt-auth.guard';
import { RolesGuard } from '../../guards/roles.guard';
import { Roles } from '../../../../common/decorators/roles.decorator';

import { GetRolesQuery } from '../../queries/get-roles.handler';
import { GetRoleByIdQuery } from '../../queries/get-role-by-id.handler';
import { CreateRoleCommand } from '../../commands/create-role.handler';
import { UpdateRoleCommand } from '../../commands/update-role.handler';
import { DeleteRoleCommand } from '../../commands/delete-role.handler';
import { AssignPermissionToRoleCommand } from '../../commands/assign-permission-to-role.handler';
import { RemovePermissionFromRoleCommand } from '../../commands/remove-permission-from-role.handler';

@Controller('roles')
@UseGuards(JwtAuthGuard, RolesGuard)
@Roles('Admin', 'SuperAdmin')
export class RolesController {
  constructor(
    private readonly commandBus: CommandBus,
    private readonly queryBus: QueryBus,
  ) {}

  @Get()
  async getRoles() {
    return this.queryBus.execute(new GetRolesQuery());
  }

  @Get(':id')
  async getRoleById(@Param('id') id: string) {
    return this.queryBus.execute(new GetRoleByIdQuery(id));
  }

  @Post()
  async createRole(@Body() body: any) {
    return this.commandBus.execute(new CreateRoleCommand(body.name, body.description));
  }

  @Put(':id')
  async updateRole(@Param('id') id: string, @Body() body: any) {
    return this.commandBus.execute(new UpdateRoleCommand(id, body.name, body.description));
  }

  @Delete(':id')
  async deleteRole(@Param('id') id: string) {
    return this.commandBus.execute(new DeleteRoleCommand(id));
  }

  @Post(':id/permissions')
  async assignPermission(@Param('id') id: string, @Body() body: any) {
    return this.commandBus.execute(
      new AssignPermissionToRoleCommand(id, body.permissionId, body.scopeType, body.scopeValue)
    );
  }

  @Delete(':id/permissions/:permissionId')
  async removePermission(@Param('id') id: string, @Param('permissionId') permissionId: string) {
    return this.commandBus.execute(new RemovePermissionFromRoleCommand(id, permissionId));
  }
}
