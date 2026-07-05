import { IQueryHandler, QueryHandler } from '@nestjs/cqrs';
import { PrismaService } from '../../../common/prisma/prisma.service';

export class GetSystemSettingQuery {
  constructor(public readonly key: string) {}
}

@QueryHandler(GetSystemSettingQuery)
export class GetSystemSettingHandler implements IQueryHandler<GetSystemSettingQuery> {
  constructor(private readonly prisma: PrismaService) {}

  async execute(query: GetSystemSettingQuery): Promise<string | null> {
    const setting = await this.prisma.system_settings.findUnique({
      where: { key: query.key },
    });
    return setting?.value || null;
  }
}
