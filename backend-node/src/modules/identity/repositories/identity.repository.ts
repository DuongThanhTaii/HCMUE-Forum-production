import { Injectable } from '@nestjs/common';
import { PrismaService } from '../../../common/prisma/prisma.service';

@Injectable()
export class IdentityRepository {
  constructor(private prisma: PrismaService) {}

  async getUserRolesAndPermissions(
    userId: string,
  ): Promise<{ roles: string[]; permissions: string[] }> {
    const rawResult = await this.prisma.$queryRaw<
      { role_name: string; permission_code: string | null }[]
    >`
      SELECT r.name as role_name, p.code as permission_code
      FROM "identity"."user_roles" ur
      JOIN "identity"."roles" r ON ur.role_id = r.id
      LEFT JOIN "identity"."role_permissions" rp ON rp.role_id = r.id
      LEFT JOIN "identity"."permissions" p ON p.id = rp.permission_id
      WHERE ur.user_id = ${userId}::uuid
    `;

    const roles = [...new Set(rawResult.map((r) => r.role_name))];
    const permissions = [
      ...new Set(
        rawResult.map((r) => r.permission_code).filter(Boolean) as string[],
      ),
    ];

    return { roles, permissions };
  }
}
