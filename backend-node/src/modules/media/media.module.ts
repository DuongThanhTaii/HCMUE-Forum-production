import { Module } from '@nestjs/common';
import { CloudinaryService } from './services/cloudinary.service';
import { MediaController } from './controllers/media.controller';
import { IdentityModule } from '../identity/identity.module';

@Module({
  imports: [IdentityModule],
  controllers: [MediaController],
  providers: [CloudinaryService],
  exports: [CloudinaryService],
})
export class MediaModule {}
