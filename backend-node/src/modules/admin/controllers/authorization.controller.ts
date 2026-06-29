import {
  Controller,
  Get,
  Post,
  Put,
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

  public static maintenanceMode = {
    isEnabled: false,
    reason: null,
    updatedBy: 'Admin',
    updatedAtUtc: new Date().toISOString(),
    version: 1,
  };

  private static toggles = [
    {
      endpointKey: 'System.Maintenance.Mode',
      isEnabled: true,
      reason: null,
      updatedBy: 'Admin',
      updatedAtUtc: new Date().toISOString(),
      version: 1,
    },
    {
      endpointKey: 'UniHub.Forum.GetPosts',
      isEnabled: true,
      reason: null,
      updatedBy: 'Admin',
      updatedAtUtc: new Date().toISOString(),
      version: 1,
    },
  ];

  // Toggles Stubs
  @Get('toggles')
  async getToggles() {
    return AuthorizationController.toggles;
  }

  @Put('toggles/:endpointKey')
  async setToggle(@Param('endpointKey') endpointKey: string, @Body() body: any) {
    const toggle = AuthorizationController.toggles.find((t) => t.endpointKey === endpointKey);
    if (toggle) {
      toggle.isEnabled = body.isEnabled;
      toggle.reason = body.reason;
      toggle.updatedAtUtc = new Date().toISOString();
      toggle.version += 1;
      return toggle;
    }
    // If not found, add it
    const newToggle = {
      endpointKey,
      isEnabled: body.isEnabled,
      reason: body.reason,
      updatedBy: 'Admin',
      updatedAtUtc: new Date().toISOString(),
      version: 1,
    };
    AuthorizationController.toggles.push(newToggle);
    return newToggle;
  }

  // Maintenance Mode Stubs
  @Get('maintenance-mode')
  async getMaintenanceMode() {
    return AuthorizationController.maintenanceMode;
  }

  @Put('maintenance-mode')
  async setMaintenanceMode(@Body() body: any) {
    AuthorizationController.maintenanceMode.isEnabled = body.isEnabled;
    AuthorizationController.maintenanceMode.reason = body.reason;
    AuthorizationController.maintenanceMode.updatedAtUtc = new Date().toISOString();
    AuthorizationController.maintenanceMode.version += 1;
    return AuthorizationController.maintenanceMode;
  }

  // Audit Logs
  @Get('audit-logs')
  async getAuditLogs() {
    return [];
  }
}
