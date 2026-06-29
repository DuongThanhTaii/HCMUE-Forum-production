import { Injectable, CanActivate, ExecutionContext, ServiceUnavailableException } from '@nestjs/common';
import { AuthorizationController } from '../../modules/admin/controllers/authorization.controller';

@Injectable()
export class MaintenanceGuard implements CanActivate {
  canActivate(context: ExecutionContext): boolean {
    const request = context.switchToHttp().getRequest();
    const url = request.url || '';

    // Ignore maintenance mode for admin and auth endpoints to prevent locking ourselves out
    if (url.startsWith('/api/v1/admin') || url.startsWith('/api/v1/auth')) {
      return true;
    }

    if (AuthorizationController.maintenanceMode.isEnabled) {
      throw new ServiceUnavailableException(
        AuthorizationController.maintenanceMode.reason || 'System is under maintenance'
      );
    }

    return true;
  }
}
