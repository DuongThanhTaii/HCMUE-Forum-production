import { Injectable } from '@nestjs/common';
import { ConfigService } from '@nestjs/config';
import { S3Client, PutObjectCommand } from '@aws-sdk/client-s3';
import * as crypto from 'crypto';

@Injectable()
export class AwsS3Service {
  private s3Client: S3Client;

  constructor(private configService: ConfigService) {
    this.s3Client = new S3Client({
      region: this.configService.get<string>('AWS_REGION') || 'us-east-1',
      credentials: {
        accessKeyId:
          this.configService.get<string>('AWS_ACCESS_KEY_ID') || 'dummy-key',
        secretAccessKey:
          this.configService.get<string>('AWS_SECRET_ACCESS_KEY') ||
          'dummy-secret',
      },
    });
  }

  async uploadDocument(
    file: Express.Multer.File,
    folder: string = 'documents',
    overrideBucket?: string,
  ): Promise<string> {
    const bucketName =
      overrideBucket ||
      this.configService.get<string>('AWS_S3_BUCKET_NAME') ||
      'unihub-learning-docs';
    const fileExtension = file.originalname.split('.').pop();
    const uniqueFileName = `${folder}/${crypto.randomUUID()}.${fileExtension}`;

    const command = new PutObjectCommand({
      Bucket: bucketName,
      Key: uniqueFileName,
      Body: file.buffer,
      ContentType: file.mimetype,
    });

    await this.s3Client.send(command);

    return `https://${bucketName}.s3.${this.configService.get<string>('AWS_REGION') || 'us-east-1'}.amazonaws.com/${uniqueFileName}`;
  }
}
