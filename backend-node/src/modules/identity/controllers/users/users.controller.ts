import {
  Controller,
  Get,
  Put,
  Post,
  Delete,
  UseGuards,
  Request,
  Body,
  Param,
} from '@nestjs/common';
import { CommandBus, QueryBus } from '@nestjs/cqrs';
import { JwtAuthGuard } from '../../guards/jwt-auth.guard';
import { RolesGuard } from '../../guards/roles.guard';
import { Roles } from '../../../../common/decorators/roles.decorator';
import { GetUserByIdQuery } from '../../queries/get-user-by-id.query';
import { GetUsersQuery } from '../../queries/get-users.query';
import { UpdateUserProfileCommand } from '../../commands/update-user-profile.command';
import { BlockUserCommand } from '../../commands/block-user.command';
import { AssignRoleToUserCommand } from '../../commands/assign-role-to-user.handler';
import { RemoveRoleFromUserCommand } from '../../commands/remove-role-from-user.handler';
import { AssignBadgeCommand } from '../../commands/assign-badge.handler';
import { RemoveBadgeCommand } from '../../commands/remove-badge.handler';
import { UpdateProfileDto } from '../../dtos/update-profile.dto';

@Controller('users')
@UseGuards(JwtAuthGuard)
export class UsersController {
  constructor(
    private readonly commandBus: CommandBus,
    private readonly queryBus: QueryBus,
  ) {}

  @Get('me')
  async getMyProfile(@Request() req: any) {
    return this.queryBus.execute(new GetUserByIdQuery(req.user.userId));
  }

  @Put('me')
  async updateMyProfile(@Request() req: any, @Body() dto: UpdateProfileDto) {
    return this.commandBus.execute(
      new UpdateUserProfileCommand(
        req.user.userId,
        dto.firstName,
        dto.lastName,
        dto.bio,
        dto.phone,
      ),
    );
  }

  // --- Example of RBAC for admin endpoints ---
  @UseGuards(RolesGuard)
  @Roles('Admin', 'SuperAdmin')
  @Get()
  async getUsers() {
    return this.queryBus.execute(new GetUsersQuery());
  }

  @UseGuards(RolesGuard)
  @Roles('Admin', 'SuperAdmin')
  @Get(':id')
  async getUserById(@Param('id') id: string) {
    return this.queryBus.execute(new GetUserByIdQuery(id));
  }

  @UseGuards(RolesGuard)
  @Roles('Admin', 'SuperAdmin')
  @Put(':id/block')
  async blockUser(@Param('id') id: string) {
    return this.commandBus.execute(new BlockUserCommand(id));
  }

  @UseGuards(RolesGuard)
  @Roles('Admin', 'SuperAdmin')
  @Post(':id/roles')
  async assignRole(@Param('id') id: string, @Body() body: any) {
    return this.commandBus.execute(new AssignRoleToUserCommand(id, body.roleId));
  }

  @UseGuards(RolesGuard)
  @Roles('Admin', 'SuperAdmin')
  @Delete(':id/roles/:roleId')
  async removeRole(@Param('id') id: string, @Param('roleId') roleId: string) {
    return this.commandBus.execute(new RemoveRoleFromUserCommand(id, roleId));
  }

  @UseGuards(RolesGuard)
  @Roles('Admin', 'SuperAdmin')
  @Post(':id/badge')
  async assignBadge(@Param('id') id: string, @Body() body: any) {
    return this.commandBus.execute(new AssignBadgeCommand(id, body.badgeType, body.badgeName, body.badgeDescription));
  }

  @UseGuards(RolesGuard)
  @Roles('Admin', 'SuperAdmin')
  @Delete(':id/badge')
  async removeBadge(@Param('id') id: string) {
    return this.commandBus.execute(new RemoveBadgeCommand(id));
  }
}
