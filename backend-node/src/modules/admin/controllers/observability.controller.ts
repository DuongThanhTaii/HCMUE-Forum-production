import { Controller, Get, UseGuards, Query } from '@nestjs/common';
import { JwtAuthGuard } from '../../identity/guards/jwt-auth.guard';
import { RolesGuard } from '../../identity/guards/roles.guard';
import { Roles } from '../../../common/decorators/roles.decorator';

@Controller('admin/observability')
@UseGuards(JwtAuthGuard, RolesGuard)
@Roles('Admin', 'SuperAdmin')
export class ObservabilityController {
  
  @Get('user-actions')
  async getUserActions(@Query() query: any) {
    return {
      items: [],
      total: 0,
      page: query.page ? parseInt(query.page) : 1,
      pageSize: query.pageSize ? parseInt(query.pageSize) : 50,
      viewType: query.viewType || 'Administrator',
      availableViewTypes: ['Administrator', 'Developer'],
      persistToMongo: false,
      mongoCollectionName: null
    };
  }
}
