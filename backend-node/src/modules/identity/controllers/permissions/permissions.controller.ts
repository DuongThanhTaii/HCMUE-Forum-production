import {
  Controller,
  Get,
  UseGuards,
  Param,
} from '@nestjs/common';
import { QueryBus } from '@nestjs/cqrs';
import { JwtAuthGuard } from '../../guards/jwt-auth.guard';
import { RolesGuard } from '../../guards/roles.guard';
import { Roles } from '../../../../common/decorators/roles.decorator';

import { GetPermissionsQuery } from '../../queries/get-permissions.handler';
import { GetPermissionByIdQuery } from '../../queries/get-permission-by-id.handler';

@Controller('permissions')
@UseGuards(JwtAuthGuard, RolesGuard)
@Roles('Admin', 'SuperAdmin')
export class PermissionsController {
  constructor(private readonly queryBus: QueryBus) {}

  @Get()
  async getPermissions() {
    return this.queryBus.execute(new GetPermissionsQuery());
  }

  @Get(':id')
  async getPermissionById(@Param('id') id: string) {
    return this.queryBus.execute(new GetPermissionByIdQuery(id));
  }
}
