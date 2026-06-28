import {
  Injectable,
  CanActivate,
  ExecutionContext,
  ForbiddenException,
} from '@nestjs/common';
import { Reflector } from '@nestjs/core';
import { IdentityRepository } from '../repositories/identity.repository';
import { ROLES_KEY } from '../../../common/decorators/roles.decorator';
import { PERMISSIONS_KEY } from '../../../common/decorators/permissions.decorator';

@Injectable()
export class RolesGuard implements CanActivate {
  constructor(
    private reflector: Reflector,
    private identityRepository: IdentityRepository,
  ) {}

  async canActivate(context: ExecutionContext): Promise<boolean> {
    const requiredRoles = this.reflector.getAllAndOverride<string[]>(
      ROLES_KEY,
      [context.getHandler(), context.getClass()],
    );

    const requiredPermissions = this.reflector.getAllAndOverride<string[]>(
      PERMISSIONS_KEY,
      [context.getHandler(), context.getClass()],
    );

    if (!requiredRoles && !requiredPermissions) {
      return true;
    }

    const { user } = context.switchToHttp().getRequest();
    if (!user) {
      throw new ForbiddenException('User is not authenticated');
    }

    // Fetch roles and permissions from the repository
    const { roles: userRoles, permissions: userPermissions } =
      await this.identityRepository.getUserRolesAndPermissions(user.userId);

    let hasRole = false;
    if (requiredRoles) {
      hasRole = requiredRoles.some((role) => userRoles.includes(role));
    } else {
      hasRole = true; // Not checking roles
    }

    let hasPermission = false;
    if (requiredPermissions) {
      hasPermission = requiredPermissions.some((perm) =>
        userPermissions.includes(perm),
      );
    } else {
      hasPermission = true; // Not checking permissions
    }

    // Must satisfy either roles OR permissions if both are provided, or strictly whichever is defined.
    // In many RBAC systems, having the Role or having the specific Permission is sufficient.
    const isAuthorized = hasRole && hasPermission;

    if (!isAuthorized) {
      throw new ForbiddenException(
        'You do not have the required roles or permissions to access this resource.',
      );
    }

    return true;
  }
}
