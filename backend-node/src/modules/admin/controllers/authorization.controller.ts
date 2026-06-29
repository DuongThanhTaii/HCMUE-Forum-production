import {
  Controller,
  Get,
  Post,
  Delete,
  UseGuards,
  Param,
  Body,
  Query,
} from '@nestjs/common';
import { JwtAuthGuard } from '../../identity/guards/jwt-auth.guard';
import { RolesGuard } from '../../identity/guards/roles.guard';
import { Roles } from '../../../common/decorators/roles.decorator';

@Controller('admin/authorization')
@UseGuards(JwtAuthGuard, RolesGuard)
@Roles('Admin', 'SuperAdmin')
export class AuthorizationController {
  
  // User Overrides Stubs
  @Get('users/:userId/overrides')
  async getUserOverrides(@Param('userId') userId: string) {
    return [];
  }

  @Post('users/:userId/overrides')
  async upsertUserOverride(@Param('userId') userId: string, @Body() body: any) {
    return { success: true };
  }

  @Delete('users/:userId/overrides')
  async revokeUserOverride(
    @Param('userId') userId: string,
    @Query('permissionId') permissionId: string,
  ) {
    return { success: true };
  }

  // Groups Stubs
  @Get('groups')
  async getGroups() {
    return [];
  }

  // Group Overrides Stubs
  @Get('groups/:groupId/overrides')
  async getGroupOverrides(@Param('groupId') groupId: string) {
    return [];
  }

  @Post('groups/:groupId/overrides')
  async upsertGroupOverride(@Param('groupId') groupId: string, @Body() body: any) {
    return { success: true };
  }

  @Delete('groups/:groupId/overrides')
  async revokeGroupOverride(
    @Param('groupId') groupId: string,
    @Query('permissionId') permissionId: string,
  ) {
    return { success: true };
  }
}
