import { Module } from '@nestjs/common';
import { PassportModule } from '@nestjs/passport';
import { JwtModule } from '@nestjs/jwt';
import { CqrsModule } from '@nestjs/cqrs';
import { AuthController } from './controllers/auth/auth.controller';
import { UsersController } from './controllers/users/users.controller';
import { AuthService } from './services/auth/auth.service';
import { LocalStrategy } from './strategies/local.strategy';
import { JwtStrategy } from './strategies/jwt.strategy';
import { IdentityRepository } from './repositories/identity.repository';
import { GetUserByIdHandler } from './queries/get-user-by-id.handler';
import { GetUsersHandler } from './queries/get-users.handler';
import { UpdateUserProfileHandler } from './commands/update-user-profile.handler';
import { BlockUserHandler } from './commands/block-user.handler';
import { AssignRoleToUserHandler } from './commands/assign-role-to-user.handler';
import { RemoveRoleFromUserHandler } from './commands/remove-role-from-user.handler';
import { AssignBadgeHandler } from './commands/assign-badge.handler';
import { RemoveBadgeHandler } from './commands/remove-badge.handler';

import { GetRolesHandler } from './queries/get-roles.handler';
import { GetRoleByIdHandler } from './queries/get-role-by-id.handler';
import { CreateRoleHandler } from './commands/create-role.handler';
import { UpdateRoleHandler } from './commands/update-role.handler';
import { DeleteRoleHandler } from './commands/delete-role.handler';
import { AssignPermissionToRoleHandler } from './commands/assign-permission-to-role.handler';
import { RemovePermissionFromRoleHandler } from './commands/remove-permission-from-role.handler';

import { GetPermissionsHandler } from './queries/get-permissions.handler';
import { GetPermissionByIdHandler } from './queries/get-permission-by-id.handler';

import { RolesController } from './controllers/roles/roles.controller';
import { PermissionsController } from './controllers/permissions/permissions.controller';

const CommandHandlers = [
  UpdateUserProfileHandler,
  BlockUserHandler,
  AssignRoleToUserHandler,
  RemoveRoleFromUserHandler,
  AssignBadgeHandler,
  RemoveBadgeHandler,
  CreateRoleHandler,
  UpdateRoleHandler,
  DeleteRoleHandler,
  AssignPermissionToRoleHandler,
  RemovePermissionFromRoleHandler,
];

const QueryHandlers = [
  GetUserByIdHandler,
  GetUsersHandler,
  GetRolesHandler,
  GetRoleByIdHandler,
  GetPermissionsHandler,
  GetPermissionByIdHandler,
];

@Module({
  imports: [
    PassportModule,
    CqrsModule,
    JwtModule.register({
      secret: process.env.JWT_SECRET || 'fallback_secret_for_dev_only',
      signOptions: { expiresIn: '15m' },
    }),
  ],
  controllers: [
    AuthController,
    UsersController,
    RolesController,
    PermissionsController,
  ],
  providers: [
    AuthService,
    LocalStrategy,
    JwtStrategy,
    IdentityRepository,
    ...CommandHandlers,
    ...QueryHandlers,
  ],
  exports: [IdentityRepository, JwtModule],
})
export class IdentityModule {}
