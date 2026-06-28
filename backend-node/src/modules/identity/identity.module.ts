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

const CommandHandlers = [UpdateUserProfileHandler, BlockUserHandler];
const QueryHandlers = [GetUserByIdHandler, GetUsersHandler];

@Module({
  imports: [
    PassportModule,
    CqrsModule,
    JwtModule.register({
      secret: process.env.JWT_SECRET || 'fallback_secret_for_dev_only',
      signOptions: { expiresIn: '15m' },
    }),
  ],
  controllers: [AuthController, UsersController],
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
