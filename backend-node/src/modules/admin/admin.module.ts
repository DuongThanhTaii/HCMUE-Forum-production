import { Module } from '@nestjs/common';
import { AuthorizationController } from './controllers/authorization.controller';
import { IdentityModule } from '../identity/identity.module';

@Module({
  imports: [IdentityModule],
  controllers: [AuthorizationController],
  providers: [],
})
export class AdminModule {}
