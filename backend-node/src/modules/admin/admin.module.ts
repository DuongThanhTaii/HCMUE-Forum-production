import { Module } from '@nestjs/common';
import { AuthorizationController } from './controllers/authorization.controller';
import { ObservabilityController } from './controllers/observability.controller';
import { IdentityModule } from '../identity/identity.module';

@Module({
  imports: [IdentityModule],
  controllers: [AuthorizationController, ObservabilityController],
  providers: [],
})
export class AdminModule {}
