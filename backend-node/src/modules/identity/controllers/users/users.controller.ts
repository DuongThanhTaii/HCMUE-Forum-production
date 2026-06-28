import {
  Controller,
  Get,
  Put,
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
}
