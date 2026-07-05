import { Module } from '@nestjs/common';
import { CqrsModule } from '@nestjs/cqrs';
import { AuthorizationController } from './controllers/authorization.controller';
import { ObservabilityController } from './controllers/observability.controller';
import { SettingsController } from './controllers/settings.controller';
import { IdentityModule } from '../identity/identity.module';
import { GetSystemSettingHandler } from './queries/get-system-setting.handler';
import { UpdateSystemSettingHandler } from './commands/update-system-setting.handler';

@Module({
  imports: [IdentityModule, CqrsModule],
  controllers: [AuthorizationController, ObservabilityController, SettingsController],
  providers: [GetSystemSettingHandler, UpdateSystemSettingHandler],
})
export class AdminModule {}

